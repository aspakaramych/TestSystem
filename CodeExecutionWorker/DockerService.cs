using System.Formats.Tar;
using System.Text;
using System.Text.Json;
using CodeExecutionWorker;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace CodeExecutionWorker;

public class DockerService
{
    private readonly DockerClient _dockerClient;
    private readonly ILogger<DockerService> _logger;

    public DockerService(ILogger<DockerService> logger)
    {
        _logger = logger;
        _dockerClient = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock")).CreateClient();
    }

    public async Task<CodeExecutionResult> ExecuteCodeAsync(CodeExecutionRequest request)
    {
        _logger.LogInformation(request.Tests);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var tests = JsonSerializer.Deserialize<List<TestModel>>(request.Tests, options);
        _logger.LogInformation($"{tests[0].Out}");
        string dockerImage = "";
        string codeFileName = "";
        string executionCommand = "";
        string containerWorkingDir = "/app";
        var result = new CodeExecutionResult 
        { 
            CorrelationId = request.CorrelationId, 
            PackageId = request.PackageId,
            PassedTests = 0,
            FailedTests = 0,
        };
        switch (request.Language.ToLower())
        {
            case "python":
                dockerImage = "python:3.9-slim-buster";
                codeFileName = "main.py";
                executionCommand = $"python {containerWorkingDir}/{codeFileName}";
                break;
            case "csharp":
                dockerImage = "mcr.microsoft.com/dotnet/sdk:10.0";
                codeFileName = "Program.cs";
                executionCommand = $"dotnet run {containerWorkingDir}/{codeFileName}";
                break;
            case "java":
                dockerImage = "amazoncorretto:17-alpine";
                codeFileName = "app.java";
                executionCommand = $"cd {containerWorkingDir} && javac {codeFileName} && java app";
                break;
            case "cpp":
                dockerImage = "gcc:latest";
                codeFileName = "main.cpp";
                executionCommand = $"gcc {containerWorkingDir}/{codeFileName} -o main && main";
                break;
        }

        await EnsureDockerImageExist(dockerImage);

        var createContainerParametrs = new CreateContainerParameters
        {
            Image = dockerImage,
            AttachStderr = true,
            AttachStdin = true,
            AttachStdout = true,
            WorkingDir = containerWorkingDir,
            HostConfig = new HostConfig
            {
                NetworkMode = "none",
                Memory = 256 * 256 * 1024,
                CPUQuota = 10000,
            },
            Cmd = new List<string> { "tail", "-f", "/dev/null" }
        };
        
        var container = await _dockerClient.Containers.CreateContainerAsync(createContainerParametrs);
        string containerId = container.ID;
        _logger.LogInformation("Creating container {containerId}", containerId);

        try
        {
            await _dockerClient.Containers.StartContainerAsync(containerId, null);
            using (var tarStream = CreateTar(codeFileName, request.Code))
            {
                await _dockerClient.Containers.ExtractArchiveToContainerAsync(containerId,
                    new ContainerPathStatParameters { Path = containerWorkingDir }, tarStream);
            }

            foreach (var test in tests)
            {
                var execParams = new ContainerExecCreateParameters
                {
                    AttachStdin = true,
                    AttachStdout = true,
                    AttachStderr = true,
                    Cmd = new List<string> { "sh", "-c", executionCommand }
                };
                var exec = await _dockerClient.Exec.ExecCreateContainerAsync(containerId, execParams);
                using (var stream = await _dockerClient.Exec.StartAndAttachContainerExecAsync(exec.ID, false))
                {
                    var inputBytes = Encoding.UTF8.GetBytes(test.In + "\n");
                    await stream.WriteAsync(inputBytes, 0, inputBytes.Length, CancellationToken.None);
                    var (stdout, stderr) = await stream.ReadOutputToEndAsync(CancellationToken.None);
                    if (string.IsNullOrWhiteSpace(stderr) && stdout.Trim() == test.Out.Trim())
                    {
                        result.PassedTests++;
                    }
                    else
                    {
                        result.FailedTests++;
                        _logger.LogInformation("Test {test} failed", test);
                    }
                }
            }
        }
        finally
        {
            await _dockerClient.Containers.RemoveContainerAsync(containerId,
                new ContainerRemoveParameters { Force = true });
        }
        return result;
    }

    private async Task EnsureDockerImageExist(string dockerImage)
    {
        try
        {
            var images = await _dockerClient.Images.ListImagesAsync(new ImagesListParameters
            {
                Filters = new Dictionary<string, IDictionary<string, bool>>
                {
                    { "reference", new Dictionary<string, bool> { { dockerImage, true } } }
                }
            });

            if (images.Count == 0)
            {
                _logger.LogInformation("Docker image not found, pulling");
                await _dockerClient.Images.CreateImageAsync(new ImagesCreateParameters
                {
                    FromImage = dockerImage.Split(':')[0],
                    Tag = dockerImage.Contains(":") ? dockerImage.Split(':')[1] : "latest"
                }, null, new Progress<JSONMessage>());
                _logger.LogInformation("Docker image found, pulling succeeded");
            }
            else
            {
                _logger.LogInformation("Docker image found locally");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in find docker image");
            throw;
        }
    }

    private Stream CreateTar(string filename, string content)
    {
        var tarStream = new MemoryStream();
        using (var tarArchive = new TarWriter(tarStream, TarEntryFormat.Pax, leaveOpen: true))
        {
            var entry = new PaxTarEntry(TarEntryType.RegularFile, filename)
            {
                DataStream = new MemoryStream(Encoding.UTF8.GetBytes(content)),
            };
            tarArchive.WriteEntry(entry);
        }
        tarStream.Position = 0;
        return tarStream;
    }
}
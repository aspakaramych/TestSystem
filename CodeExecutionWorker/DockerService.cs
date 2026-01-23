using System.Formats.Tar;
using System.Text;
using System.Text.Json;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace CodeExecutionWorker;

public class DockerService
{
    private readonly DockerClient _dockerClient;
    private readonly ILogger<DockerService> _logger;
    private readonly SemaphoreSlim _testSemaphore = new SemaphoreSlim(5);

    public DockerService(ILogger<DockerService> logger)
    {
        _logger = logger;
        _dockerClient = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock")).CreateClient();
    }

    public async Task<CodeExecutionResult> ExecuteCodeAsync(CodeExecutionRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Processing request {CorrelationId}", request.CorrelationId);
        
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var tests = JsonSerializer.Deserialize<List<TestModel>>(request.Tests, options) ?? new();
        _logger.LogInformation(tests.Count.ToString());
        var (dockerImage, codeFileName, executionCommand) = GetLanguageSettings(request.Language);
        const string containerWorkingDir = "/app";

        await EnsureDockerImageExist(dockerImage, ct);

        var createParams = new CreateContainerParameters
        {
            Image = dockerImage,
            AttachStderr = true,
            AttachStdin = true,
            AttachStdout = true,
            WorkingDir = containerWorkingDir,
            HostConfig = new HostConfig
            {
                NetworkMode = "none",
                Memory = 256 * 1024 * 1024,
                CPUQuota = 10000,
            },
            Cmd = new List<string> { "tail", "-f", "/dev/null" }
        };

        var container = await _dockerClient.Containers.CreateContainerAsync(createParams, ct);
        string containerId = container.ID;

        try
        {
            await _dockerClient.Containers.StartContainerAsync(containerId, null, ct);
            using (var tarStream = CreateTar(codeFileName, request.Code))
            {
                await _dockerClient.Containers.ExtractArchiveToContainerAsync(containerId,
                    new ContainerPathStatParameters { Path = containerWorkingDir }, tarStream, ct);
            }

            var testTasks = tests.Select(test => RunSingleTestWithSemaphoreAsync(containerId, executionCommand, test, ct));
            
            var results = await Task.WhenAll(testTasks);

            return new CodeExecutionResult
            {
                CorrelationId = request.CorrelationId,
                PackageId = request.PackageId,
                PassedTests = results.Count(r => r == true),
                FailedTests = results.Count(r => r == false)
            };
        }
        finally
        {
            await _dockerClient.Containers.RemoveContainerAsync(containerId,
                new ContainerRemoveParameters { Force = true }, ct);
        }
    }

    private async Task<bool> RunSingleTestWithSemaphoreAsync(string containerId, string command, TestModel test, CancellationToken ct)
    {
        await _testSemaphore.WaitAsync(ct);
        try
        {
            var execParams = new ContainerExecCreateParameters
            {
                AttachStdin = true,
                AttachStdout = true,
                AttachStderr = true,
                Cmd = new List<string> { "sh", "-c", command }
            };

            var exec = await _dockerClient.Exec.ExecCreateContainerAsync(containerId, execParams, ct);
            
            using var stream = await _dockerClient.Exec.StartAndAttachContainerExecAsync(exec.ID, false, ct);
            
            var inputBytes = Encoding.UTF8.GetBytes(test.In + "\n");
            await stream.WriteAsync(inputBytes, 0, inputBytes.Length, ct);

            var (stdout, stderr) = await stream.ReadOutputToEndAsync(ct);

            if (!string.IsNullOrWhiteSpace(stderr))
            {
                _logger.LogWarning("Error: {stderr}", stderr);
            }

            return string.IsNullOrWhiteSpace(stderr) && stdout.Trim() == test.Out.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during single test execution");
            return false;
        }
        finally
        {
            _testSemaphore.Release();
        }
    }

    private (string Image, string FileName, string Command) GetLanguageSettings(string language)
    {
        return language.ToLower() switch
        {
            "python" => ("python:3.9-slim-buster", "main.py", "python /app/main.py"),
            "csharp" => ("mcr.microsoft.com/dotnet/sdk:9.0", "Program.cs", 
                "dotnet new console --force -n app > /dev/null && mv /app/Program.cs app/Program.cs && dotnet run --project app/app.csproj"),
            "java"   => ("amazoncorretto:17-alpine", "app.java", "javac /app/app.java && java -cp /app app"),
            "cpp"    => ("gcc:latest", "main.cpp", "g++ /app/main.cpp -o main && ./main"),
            "go"     => ("golang:latest", "main.go", "go run /app/main.go"),
            _        => throw new NotSupportedException($"Language {language} is not supported")
        };
    }

    private async Task EnsureDockerImageExist(string dockerImage, CancellationToken ct)
    {
        var images = await _dockerClient.Images.ListImagesAsync(new ImagesListParameters
        {
            Filters = new Dictionary<string, IDictionary<string, bool>>
            {
                { "reference", new Dictionary<string, bool> { { dockerImage, true } } }
            }
        }, ct);

        if (images.Count == 0)
        {
            _logger.LogInformation("Pulling image: {dockerImage}", dockerImage);
            var parts = dockerImage.Split(':');
            await _dockerClient.Images.CreateImageAsync(new ImagesCreateParameters
            {
                FromImage = parts[0],
                Tag = parts.Length > 1 ? parts[1] : "latest"
            }, null, new Progress<JSONMessage>(), ct);
        }
    }

    private Stream CreateTar(string filename, string content)
    {
        var tarStream = new MemoryStream();
        using (var tarArchive = new TarWriter(tarStream, TarEntryFormat.Pax, leaveOpen: true))
        {
            var entry = new PaxTarEntry(TarEntryType.RegularFile, filename)
            {
                DataStream = new MemoryStream(Encoding.UTF8.GetBytes(content))
            };
            tarArchive.WriteEntry(entry);
        }
        tarStream.Position = 0;
        return tarStream;
    }
}
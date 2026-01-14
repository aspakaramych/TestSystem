using CodeExecutionWorker.Models;
using Docker.DotNet;

namespace CodeExecutionWorker;

public class DockerService
{
    private readonly DockerClient _dockerClient;

    public DockerService()
    {
        _dockerClient = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock")).CreateClient();
    }
    
    public async Task<CodeExecutionResult> ExecuteCodeAsync(CodeExecutionRequest request)
    {
        await Task.Delay(1000); 

        return new CodeExecutionResult
        {
            CorrelationId = request.CorrelationId, 
            PackageId = request.PackageId,
            PassedTests = 1,
            FailedTests = 0,
        };
    }
}
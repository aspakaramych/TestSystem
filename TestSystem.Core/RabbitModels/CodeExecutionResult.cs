namespace TestSystem.Core.RabbitModels;

public class CodeExecutionResult
{
    public int PassedTests { get; set; }
    public int FailedTests { get; set; }
    public string CorrelationId { get; set; }
    public Guid PackageId { get; set; }
}
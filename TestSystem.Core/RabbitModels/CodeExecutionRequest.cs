namespace TestSystem.Core.RabbitModels;

public class CodeExecutionRequest
{
    public string Code { get; set; }
    public string Language { get; set; }
    public string Tests { get; set; }
    public string CorrelationId { get; set; }
    public Guid PackageId { get; set; }
}
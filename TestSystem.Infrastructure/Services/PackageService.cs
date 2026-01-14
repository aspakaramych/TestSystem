using Microsoft.Extensions.Logging;
using TestSystem.Core.DTOs.PackageService;
using TestSystem.Core.Entity;
using TestSystem.Core.Interfaces;
using TestSystem.Core.KafkaModels;
using TestSystem.Infrastructure.KafkaServices;

namespace TestSystem.Infrastructure.Services;

public class PackageService : IPackageService
{
    private readonly IPackageRepository _packageRepository;
    private readonly ILogger<PackageService> _logger;
    private readonly KafkaProducer _kafkaProducer;
    private readonly IDapperPackageRepository _dapperPackageRepository;
    private readonly ITaskEntityRepository _taskRepository;

    public PackageService(IPackageRepository packageRepository, ILogger<PackageService> logger, KafkaProducer kafkaProducer, ITaskEntityRepository taskRepository, IDapperPackageRepository dapperPackageRepository)
    {
        _packageRepository = packageRepository;
        _logger = logger;
        _kafkaProducer = kafkaProducer;
        _taskRepository = taskRepository;
        _dapperPackageRepository = dapperPackageRepository;
    }
    
    public async Task CreatePackage(Guid taskId, Guid userId, PackageRequest packageRequest)
    {
        if (Enum.TryParse(packageRequest.Language, out Language language))
        {
            throw new ArgumentException("Invalid programming language specified.");
        }
        var package = new Package
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            Status = PackageStatus.Pending,
            Code = packageRequest.Code,
            Language = language
        };
        await _packageRepository.AddAsync(package);
        var task = await _taskRepository.GetByIdAsync(taskId);
        var message = new CodeExecutionRequest
        {
            Code = packageRequest.Code,
            Language = packageRequest.Language,
            CorrelationId = Guid.NewGuid().ToString(),
            Tests = task.Tests,
            PackageId = package.Id
        };
        await _kafkaProducer.ProduceAsync(message: message, topic: null);
        _logger.LogInformation("message sent to Kafka for package {PackageId}", package.Id);
    }

    public async Task<ICollection<PackageResponse>> GetPaginatedPackages(int page, int pageSize, Guid userId)
    {
        var packages = await _dapperPackageRepository.GetPackagesAsync(page, pageSize, userId);
        return packages.Select(p => new PackageResponse
        {
            Id = p.Id,
            CreatedAt = p.CreatedAt,
            Status = p.Status.ToString(),
            TaskTitle = p.Task.Title,
            Language = p.Language.ToString(),
        }).ToList();
    }
}
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using TestSystem.Core.DTOs.PackageService;
using TestSystem.Core.Entity;
using TestSystem.Core.Interfaces;
using TestSystem.Core.RabbitModels;
using TestSystem.Infrastructure.RabbitMqService;

namespace TestSystem.Infrastructure.Services;

public class PackageService : IPackageService
{
    private readonly IPackageRepository _packageRepository;
    private readonly ILogger<PackageService> _logger;
    private readonly RabbitMqPublisher _rabbitPublisher;
    private readonly IDapperPackageRepository _dapperPackageRepository;
    private readonly ITaskEntityRepository _taskRepository;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<CodeExecutionResult>> _callbacks;

    public PackageService(
        IPackageRepository packageRepository, 
        ILogger<PackageService> logger, 
        RabbitMqPublisher rabbitPublisher,
        ITaskEntityRepository taskRepository, 
        IDapperPackageRepository dapperPackageRepository,
        ConcurrentDictionary<string, TaskCompletionSource<CodeExecutionResult>> callbacks) 
    {
        _packageRepository = packageRepository;
        _logger = logger;
        _rabbitPublisher = rabbitPublisher;
        _taskRepository = taskRepository;
        _dapperPackageRepository = dapperPackageRepository;
        _callbacks = callbacks;
    }
    
    public async Task CreatePackage(Guid taskId, Guid userId, PackageRequest packageRequest)
    {
        if (!Enum.TryParse(packageRequest.Language, out Language language))
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
        var correlationId = Guid.NewGuid().ToString();
    
        var tsc = new TaskCompletionSource<CodeExecutionResult>();
        _callbacks.TryAdd(correlationId, tsc);

        var message = new CodeExecutionRequest
        {
            Code = packageRequest.Code,
            Language = packageRequest.Language,
            CorrelationId = correlationId,
            Tests = task.Tests,
            PackageId = package.Id
        };

        try 
        {
            await _rabbitPublisher.PublishAsync(message); 
            _logger.LogInformation("Message sent to RabbitMQ for package {PackageId} with CorrelationId {CorrId}", package.Id, correlationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to RabbitMQ");
            _callbacks.TryRemove(correlationId, out _);
            throw;
        }
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
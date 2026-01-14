using TestSystem.Core.DTOs.PackageService;

namespace TestSystem.Core.Interfaces;

public interface IPackageService
{
    Task CreatePackage(Guid taskId, Guid userId, PackageRequest packageRequest);
    Task<ICollection<PackageResponse>> GetPaginatedPackages(int page, int pageSize, Guid userId);
}
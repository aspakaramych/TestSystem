using TestSystem.Core.DTOs.ClassRoomService;

namespace TestSystem.Core.Interfaces;

public interface IClassRoomService
{
    Task<IEnumerable<ClassRoomResponse>> GetClassRoomsAsync(Guid userId);
    Task CreateClassRoomAsync(ClassRoomCreateRequest request);
}
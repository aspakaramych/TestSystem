using TestSystem.Core.DTOs.ClassRoomService;
using TestSystem.Core.Entity;
using TestSystem.Core.Interfaces;

namespace TestSystem.Infrastructure.Services;

public class ClassRoomService : IClassRoomService
{
    private readonly IDapperClassRoomsRepository _dapperClassRoomsRepository;
    private readonly IClassRoomsRepository _classRoomsRepository;

    public ClassRoomService(IDapperClassRoomsRepository dapperClassRoomsRepository, IClassRoomsRepository classRoomsRepository)
    {
        _dapperClassRoomsRepository = dapperClassRoomsRepository;
        _classRoomsRepository = classRoomsRepository;
    }
    
    public async Task<IEnumerable<ClassRoomResponse>> GetClassRoomsAsync(Guid userId)
    {
        var classRooms = await _dapperClassRoomsRepository.GetClassRoomsAsync(userId);
        if (classRooms == null)
            return Enumerable.Empty<ClassRoomResponse>();

        return classRooms.Select(cr => new ClassRoomResponse
        {
            Id = cr.Id,
            Title = cr.Title,
        });
    }

    public async Task CreateClassRoomAsync(ClassRoomCreateRequest request)
    {
        var newClassRoom = new ClassRoom
        {
            Id = Guid.NewGuid(),
            Title = request.Title
        };
        await _classRoomsRepository.AddAsync(newClassRoom);
    }
}
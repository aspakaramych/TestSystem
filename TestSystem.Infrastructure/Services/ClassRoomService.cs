using TestSystem.Core.DTOs.ClassRoomService;
using TestSystem.Core.Entity;
using TestSystem.Core.Interfaces;

namespace TestSystem.Infrastructure.Services;

public class ClassRoomService : IClassRoomService
{
    private readonly IDapperClassRoomsRepository _dapperClassRoomsRepository;
    private readonly IClassRoomsRepository _classRoomsRepository;
    private readonly IUserClassroomsRepository _userClassroomsRepository;

    public ClassRoomService(IDapperClassRoomsRepository dapperClassRoomsRepository, IClassRoomsRepository classRoomsRepository, IUserClassroomsRepository userClassroomsRepository)
    {
        _dapperClassRoomsRepository = dapperClassRoomsRepository;
        _classRoomsRepository = classRoomsRepository;
        _userClassroomsRepository = userClassroomsRepository;
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

    public async Task CreateClassRoomAsync(ClassRoomCreateRequest request, Guid userId)
    {
        var newClassRoom = new ClassRoom
        {
            Id = Guid.NewGuid(),
            Title = request.Title
        };
        await _classRoomsRepository.AddAsync(newClassRoom);
        var userClassRoom = new UserClassRoom
        {
            UserId = userId,
            ClassRoomId = newClassRoom.Id,
            Role = UserRoleInClassRoom.Teacher
        };
        await _userClassroomsRepository.AddAsync(userClassRoom);
    }
}
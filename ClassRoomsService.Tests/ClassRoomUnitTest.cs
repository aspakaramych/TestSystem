using Moq;
using TestSystem.Controllers;
using TestSystem.Core.Interfaces;
using Xunit;

namespace ClassRoomsService.Tests;

public class ClassRoomUnitTest
{
    private readonly Mock<IClassRoomService> _mockClassRoomService;
    private readonly TestSystemController _testSystemController;

    public ClassRoomUnitTest(Mock<IClassRoomService> mockClassRoomService, TestSystemController testSystemController)
    {
        _mockClassRoomService = mockClassRoomService;
        _testSystemController = testSystemController;
    }
}
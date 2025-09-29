using System;
using System.Linq;
using UserManagement.Models;
using UserManagement.Services.Domain.Implementations;

namespace UserManagement.Data.Tests;

public class UserServiceTests
{
    [Fact]
    public void GetAll_WhenContextReturnsEntities_MustReturnSameEntities()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var service = CreateService();
        var users = SetupUsers();

        // Act: Invokes the method under test with the arranged parameters.
        var result = service.GetAll();

        // Assert: Verifies that the action of the method under test behaves as expected.
        result.Should().BeSameAs(users);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void FilterByActive_WhenFilteringByActiveState_MustReturnOnlyUsersMatchingActiveState(bool isActive)
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var service = CreateService();
        var users = SetupUsers();

        // Act: Invokes the method under test with the arranged parameters.
        var result = service.FilterByActive(isActive);

        // Assert: Verifies that the action of the method under test behaves as expected.
        result.Should().HaveCount(1).And.OnlyContain(u => u.IsActive == isActive);
    }

    [Fact]
    public void Create_WhenCreatingUser_MustCallDataContextCreate()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var service = CreateService();
        var user = new User
        {
            Forename = "Test",
            Surname = "User",
            Email = "test@example.com",
            IsActive = true,
            DateOfBirth = new DateTime(1990, 1, 1)
        };

        // Act: Invokes the method under test with the arranged parameters.
        service.Create(user);

        // Assert: Verifies that the action of the method under test behaves as expected.
        _dataContext.Verify(dc => dc.Create(user), Times.Once);
    }

    [Fact]
    public void GetByIdAsync_ExistingUser_ReturnsUser()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var service = CreateService();
        var user = SetupUsers().First();
        _dataContext.Setup(s => s.GetById<User>(1L)).Returns(user);

        // Act: Invokes the method under test with the arranged parameters.
        var result = service.GetById(1);

        // Assert: Verifies that the action of the method under test behaves as expected.
        result.Should().NotBeNull().And.BeEquivalentTo(user);
        _dataContext.Verify(s => s.GetById<User>(1L), Times.Once);
    }

    [Fact]
    public void GetByIdAsync_NonExistentUser_ReturnsNull()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var service = CreateService();
        _dataContext.Setup(s => s.GetById<User>(999L)).Returns((User?)null);

        // Act: Invokes the method under test with the arranged parameters.
        var result = service.GetById(999);

        // Assert: Verifies that the action of the method under test behaves as expected.
        result.Should().BeNull();
        _dataContext.Verify(s => s.GetById<User>(999L), Times.Once);
    }

    private IQueryable<User> SetupUsers(string forename = "Johnny", string surname = "User", string email = "juser@example.com")
    {
        var users = new[]
        {
            new User
            {
                Forename = forename,
                Surname = surname,
                Email = email,
                IsActive = true,
                DateOfBirth = new DateTime(1990, 1, 15)
            },
            new User
            {
                Forename = "Inactive",
                Surname = "User",
                Email = "inactive@example.com",
                IsActive = false,
                DateOfBirth = new DateTime(1985, 5, 20)
            }
        }.AsQueryable();

        _dataContext
            .Setup(s => s.GetAll<User>())
            .Returns(users);

        _dataContext
            .Setup(s => s.GetById<User>(1L))
            .Returns(users.First());

        return users;
    }

    private readonly Mock<IDataContext> _dataContext = new();
    private UserService CreateService() => new(_dataContext.Object);
}

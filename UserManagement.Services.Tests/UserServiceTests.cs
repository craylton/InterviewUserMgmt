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
        result.Should().HaveCount(1);
        result.Should().OnlyContain(u => u.IsActive == isActive);
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

        return users;
    }

    private readonly Mock<IDataContext> _dataContext = new();
    private UserService CreateService() => new(_dataContext.Object);
}

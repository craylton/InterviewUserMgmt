using System;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Data;
using UserManagement.Data.Entities;
using UserManagement.Services.Implementations;
using UserManagement.Services.Interfaces;

namespace UserManagement.Services.Tests;

public sealed class UserServiceTests
{
    [Fact]
    public void GetAll_WhenContextReturnsEntities_ShouldReturnSameEntities()
    {
        // Arrange
        var service = CreateService();
        var users = SetupUsers();

        // Act
        var result = service.GetAll();

        // Assert
        result.Should().BeSameAs(users);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void FilterByActive_WhenFilteringByActiveState_ShouldReturnOnlyUsersMatchingActiveState(bool isActive)
    {
        // Arrange
        var service = CreateService();
        var users = SetupUsers();

        // Act
        var result = service.FilterByActive(isActive);

        // Assert
        result.Should().ContainSingle(u => u.IsActive == isActive);
    }

    [Fact]
    public async Task Create_WhenCreatingUser_ShouldCallDataContextCreate()
    {
        // Arrange
        var service = CreateService();
        var user = new User
        {
            Forename = "Test",
            Surname = "User",
            Email = "test@example.com",
            IsActive = true,
            DateOfBirth = new DateTime(1990, 1, 1)
        };

        // Act
        await service.CreateAsync(user);

        // Assert
        _dataContext.Verify(s => s.CreateAsync(user), Times.Once);
        _changeLogService.Verify(s => s.LogAddAsync(user), Times.Once);
    }

    [Fact]
    public async Task GetById_ExistingUser_ReturnsUser()
    {
        // Arrange
        var service = CreateService();
        var user = SetupUsers().First();
        _dataContext.Setup(s => s.GetByIdAsync<User>(1L)).ReturnsAsync(user);

        // Act
        var result = await service.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull().And.BeEquivalentTo(user);
        _dataContext.Verify(s => s.GetByIdAsync<User>(1L), Times.Once);
    }

    [Fact]
    public async Task GetById_NonExistentUser_ReturnsNull()
    {
        // Arrange
        var service = CreateService();
        _dataContext.Setup(s => s.GetByIdAsync<User>(999L)).ReturnsAsync((User?)null);

        // Act
        var result = await service.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
        _dataContext.Verify(s => s.GetByIdAsync<User>(999L), Times.Once);
    }

    [Fact]
    public async Task Update_WhenUpdatingUser_ShouldCallDataContextUpdate()
    {
        // Arrange
        var service = CreateService();
        var existingUser = new User
        {
            Id = 1,
            Forename = "Original",
            Surname = "User",
            Email = "original@example.com",
            IsActive = true,
            DateOfBirth = new DateTime(1990, 1, 1)
        };
        var updatedUser = new User
        {
            Id = 1,
            Forename = "Updated",
            Surname = "User",
            Email = "updated@example.com",
            IsActive = false,
            DateOfBirth = new DateTime(1985, 5, 15)
        };

        // Setup GetByIdNoTracking to return the existing user for comparison
        _dataContext.Setup(s => s.GetByIdNoTrackingAsync<User>(1L)).ReturnsAsync(existingUser);

        // Act
        await service.UpdateAsync(updatedUser);

        // Assert
        _dataContext.Verify(s => s.UpdateAndSaveAsync(updatedUser), Times.Once);
        _changeLogService.Verify(s => s.LogUpdateAsync(existingUser, updatedUser), Times.Once);
    }

    [Fact]
    public async Task Delete_WhenDeletingUser_ShouldCallDataContextDelete()
    {
        // Arrange
        var service = CreateService();
        var user = SetupUsers().First();

        // Act
        await service.DeleteAsync(user);

        // Assert
        _changeLogService.Verify(s => s.LogDeleteAsync(user), Times.Once);
        _dataContext.Verify(s => s.DeleteAsync(user), Times.Once);
    }

    private IQueryable<User> SetupUsers()
    {
        var users = new[]
        {
            new User
            {
                Forename = "Johnny",
                Surname = "User",
                Email = "juser@example.com",
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
            .Setup(s => s.GetByIdAsync<User>(1L))
            .ReturnsAsync(users.First());

        return users;
    }

    private readonly Mock<IDataContext> _dataContext = new();
    private readonly Mock<IChangeLogService> _changeLogService = new();
    private UserService CreateService() => new(_dataContext.Object, _changeLogService.Object);
}

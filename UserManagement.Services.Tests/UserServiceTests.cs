using System;
using System.Linq;
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
    public void Create_WhenCreatingUser_ShouldCallDataContextCreate()
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
        service.Create(user);

        // Assert
        _dataContext.Verify(s => s.Create(user), Times.Once);
        _changeLogService.Verify(s => s.LogAdd(user), Times.Once);
    }

    [Fact]
    public void GetById_ExistingUser_ReturnsUser()
    {
        // Arrange
        var service = CreateService();
        var user = SetupUsers().First();
        _dataContext.Setup(s => s.GetById<User>(1L)).Returns(user);

        // Act
        var result = service.GetById(1);

        // Assert
        result.Should().NotBeNull().And.BeEquivalentTo(user);
        _dataContext.Verify(s => s.GetById<User>(1L), Times.Once);
    }

    [Fact]
    public void GetById_NonExistentUser_ReturnsNull()
    {
        // Arrange
        var service = CreateService();
        _dataContext.Setup(s => s.GetById<User>(999L)).Returns((User?)null);

        // Act
        var result = service.GetById(999);

        // Assert
        result.Should().BeNull();
        _dataContext.Verify(s => s.GetById<User>(999L), Times.Once);
    }

    [Fact]
    public void Update_WhenUpdatingUser_ShouldCallDataContextUpdate()
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

        // Setup GetAll to return the existing user for comparison
        var users = new[] { existingUser }.AsQueryable();
        _dataContext.Setup(s => s.GetAll<User>()).Returns(users);

        // Act
        service.Update(updatedUser);

        // Assert
        _dataContext.Verify(s => s.Update(updatedUser), Times.Once);
        _changeLogService.Verify(s => s.LogUpdate(existingUser, updatedUser), Times.Once);
    }

    [Fact]
    public void Delete_WhenDeletingUser_ShouldCallDataContextDelete()
    {
        // Arrange
        var service = CreateService();
        var user = SetupUsers().First();

        // Act
        service.Delete(user);

        // Assert
        _changeLogService.Verify(s => s.LogDelete(user), Times.Once);
        _dataContext.Verify(s => s.Delete(user), Times.Once);
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
            .Setup(s => s.GetById<User>(1L))
            .Returns(users.First());

        return users;
    }

    private readonly Mock<IDataContext> _dataContext = new();
    private readonly Mock<IChangeLogService> _changeLogService = new();
    private UserService CreateService() => new(_dataContext.Object, _changeLogService.Object);
}

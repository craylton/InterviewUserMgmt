using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UserManagement.Data;
using UserManagement.Data.Entities;
using UserManagement.Services.Implementations;

namespace UserManagement.Services.Tests;

public sealed class ChangeLogServiceTests
{
    [Fact]
    public async Task LogAdd_WhenLoggingUserAdd_MustCallDataContextCreate()
    {
        // Arrange
        var service = CreateService();
        var user = new User
        {
            Id = 1,
            Forename = "Test",
            Surname = "User",
            Email = "test@example.com",
            IsActive = true,
            DateOfBirth = new DateTime(1990, 1, 1)
        };

        // Act
        await service.LogAddAsync(user);

        // Assert
        _dataContext.Verify(s => s.CreateAsync(It.Is<ChangeLogEntry>(entry =>
            entry.UserId == user.Id &&
            entry.Action == ChangeActionType.Add &&
            entry.Description == null &&
            entry.Timestamp > DateTime.UtcNow.AddMinutes(-1))), Times.Once);
    }

    [Fact]
    public async Task LogDelete_WhenLoggingUserDelete_MustCallDataContextCreate()
    {
        // Arrange
        var service = CreateService();
        var user = new User
        {
            Id = 2,
            Forename = "Delete",
            Surname = "User",
            Email = "delete@example.com",
            IsActive = false,
            DateOfBirth = new DateTime(1985, 5, 15)
        };

        // Act
        await service.LogDeleteAsync(user);

        // Assert
        _dataContext.Verify(s => s.CreateAsync(It.Is<ChangeLogEntry>(entry =>
            entry.UserId == user.Id &&
            entry.Action == ChangeActionType.Delete &&
            entry.Description == null &&
            entry.Timestamp > DateTime.UtcNow.AddMinutes(-1))), Times.Once);
    }

    [Fact]
    public async Task LogUpdate_WhenLoggingUserUpdate_MustCallDataContextCreateForEachChange()
    {
        // Arrange
        var service = CreateService();
        var beforeUser = new User
        {
            Id = 1,
            Forename = "Original",
            Surname = "User",
            Email = "original@example.com",
            IsActive = true,
            DateOfBirth = new DateTime(1990, 1, 1)
        };
        var afterUser = new User
        {
            Id = 1,
            Forename = "Updated",
            Surname = "NewSurname",
            Email = "updated@example.com",
            IsActive = false,
            DateOfBirth = new DateTime(1985, 5, 15)
        };

        // Act
        await service.LogUpdateAsync(beforeUser, afterUser);

        // Assert
        _dataContext.Verify(s => s.CreateAsync(It.IsAny<ChangeLogEntry>()), Times.Exactly(5));

        _dataContext.Verify(s => s.CreateAsync(It.Is<ChangeLogEntry>(entry =>
            entry.UserId == afterUser.Id &&
            entry.Action == ChangeActionType.Update &&
            entry.Description != null &&
            entry.Description.Contains("Forename changed from Original to Updated"))), Times.Once);

        _dataContext.Verify(s => s.CreateAsync(It.Is<ChangeLogEntry>(entry =>
            entry.UserId == afterUser.Id &&
            entry.Action == ChangeActionType.Update &&
            entry.Description != null &&
            entry.Description.Contains("Surname changed from User to NewSurname"))), Times.Once);
    }

    [Fact]
    public async Task LogUpdate_WhenNoChanges_MustNotCallDataContextCreate()
    {
        // Arrange
        var service = CreateService();
        var user = new User
        {
            Id = 1,
            Forename = "Same",
            Surname = "User",
            Email = "same@example.com",
            IsActive = true,
            DateOfBirth = new DateTime(1990, 1, 1)
        };

        // Act
        await service.LogUpdateAsync(user, user);

        // Assert
        _dataContext.Verify(s => s.CreateAsync(It.IsAny<ChangeLogEntry>()), Times.Never);
    }

    [Fact]
    public void GetAll_WhenRetrievingLogs_MustReturnPagedResultsOrderedByTimestamp()
    {
        // Arrange
        var service = CreateService();
        SetupChangeLogEntries();

        // Act
        var result = service.GetAll(1, 2, out var totalCount);

        // Assert
        totalCount.Should().Be(3);
        result.Should().HaveCount(2).And.BeInDescendingOrder(log => log.Timestamp);
    }

    [Fact]
    public void GetAll_WhenRetrievingSecondPage_MustReturnCorrectPagedResults()
    {
        // Arrange
        var service = CreateService();
        SetupChangeLogEntries();

        // Act
        var result = service.GetAll(2, 2, out var totalCount);

        // Assert
        result.Should().HaveCount(1);
        totalCount.Should().Be(3);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    public void GetAll_WithInvalidPageNumber_ShouldThrowArgumentOutOfRangeException(int pageNumber, int pageSize)
    {
        // Arrange
        var service = CreateService();

        // Act & Assert
        var action = () => service.GetAll(pageNumber, pageSize, out _);
        action.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName(nameof(pageNumber));
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    public void GetAll_WithInvalidPageSize_ShouldThrowArgumentOutOfRangeException(int pageNumber, int pageSize)
    {
        // Arrange
        var service = CreateService();

        // Act & Assert
        var action = () => service.GetAll(pageNumber, pageSize, out _);
        action.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName(nameof(pageSize));
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    public void GetByUser_WithInvalidPageNumber_ShouldThrowArgumentOutOfRangeException(int pageNumber, int pageSize)
    {
        // Arrange
        var service = CreateService();

        // Act & Assert
        var action = () => service.GetByUser(1, pageNumber, pageSize, out _);
        action.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName(nameof(pageNumber));
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    public void GetByUser_WithInvalidPageSize_ShouldThrowArgumentOutOfRangeException(int pageNumber, int pageSize)
    {
        // Arrange
        var service = CreateService();

        // Act & Assert
        var action = () => service.GetByUser(1, pageNumber, pageSize, out _);
        action.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName(nameof(pageSize));
    }

    [Fact]
    public void GetByUser_WhenRetrievingLogsForSpecificUser_MustReturnOnlyUserLogs()
    {
        // Arrange
        var service = CreateService();
        SetupChangeLogEntries();

        // Act
        var result = service.GetByUser(1, 1, 10, out var totalCount);

        // Assert
        result.Should().HaveCount(2).And.OnlyContain(log => log.UserId == 1);
        totalCount.Should().Be(2);
    }

    [Fact]
    public void GetByUser_WhenNoLogsExistForUser_MustReturnEmptyResult()
    {
        // Arrange
        var service = CreateService();
        SetupChangeLogEntries();

        // Act
        var result = service.GetByUser(999, 1, 10, out var totalCount);

        // Assert
        result.Should().BeEmpty();
        totalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetById_WhenLogExists_MustReturnLog()
    {
        // Arrange
        var service = CreateService();
        var expectedLog = new ChangeLogEntry
        {
            Id = 1,
            UserId = 1,
            Action = ChangeActionType.Add,
            Timestamp = DateTime.UtcNow,
            Description = null
        };
        _dataContext.Setup(s => s.GetByIdAsync<ChangeLogEntry>(1L)).ReturnsAsync(expectedLog);

        // Act
        var result = await service.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull().And.BeEquivalentTo(expectedLog);
        _dataContext.Verify(s => s.GetByIdAsync<ChangeLogEntry>(1L), Times.Once);
    }

    [Fact]
    public async Task GetById_WhenLogDoesNotExist_MustReturnNull()
    {
        // Arrange
        var service = CreateService();
        _dataContext.Setup(s => s.GetByIdAsync<ChangeLogEntry>(999L)).ReturnsAsync((ChangeLogEntry?)null);

        // Act
        var result = await service.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
        _dataContext.Verify(s => s.GetByIdAsync<ChangeLogEntry>(999L), Times.Once);
    }

    private IQueryable<ChangeLogEntry> SetupChangeLogEntries()
    {
        var baseTime = DateTime.UtcNow;
        var logs = new[]
        {
            new ChangeLogEntry
            {
                Id = 1,
                UserId = 1,
                Action = ChangeActionType.Add,
                Timestamp = baseTime.AddMinutes(3), // Most recent
                Description = null
            },
            new ChangeLogEntry
            {
                Id = 2,
                UserId = 2,
                Action = ChangeActionType.Update,
                Timestamp = baseTime.AddMinutes(2),
                Description = "Email changed from old@example.com to new@example.com"
            },
            new ChangeLogEntry
            {
                Id = 3,
                UserId = 1,
                Action = ChangeActionType.Delete,
                Timestamp = baseTime.AddMinutes(1), // Oldest
                Description = null
            }
        }.AsQueryable();

        _dataContext
            .Setup(s => s.GetAll<ChangeLogEntry>())
            .Returns(logs);

        return logs;
    }

    private readonly Mock<IDataContext> _dataContext = new();
    private readonly Mock<ILogger<ChangeLogService>> _logger = new();
    private ChangeLogService CreateService() => new(_dataContext.Object, _logger.Object);
}

using System;
using System.Linq;
using UserManagement.Models;
using UserManagement.Services.Domain.Implementations;

namespace UserManagement.Data.Tests;

public class ChangeLogServiceTests
{
    [Fact]
    public void LogAdd_WhenLoggingUserAdd_MustCallDataContextCreate()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
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

        // Act: Invokes the method under test with the arranged parameters.
        service.LogAdd(user);

        // Assert: Verifies that the action of the method under test behaves as expected.
        _dataContext.Verify(s => s.Create(It.Is<ChangeLogEntry>(entry =>
            entry.UserId == user.Id &&
            entry.Action == ChangeActionType.Add &&
            entry.Description == null &&
            entry.Timestamp > DateTime.UtcNow.AddMinutes(-1))), Times.Once);
    }

    [Fact]
    public void LogDelete_WhenLoggingUserDelete_MustCallDataContextCreate()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
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

        // Act: Invokes the method under test with the arranged parameters.
        service.LogDelete(user);

        // Assert: Verifies that the action of the method under test behaves as expected.
        _dataContext.Verify(s => s.Create(It.Is<ChangeLogEntry>(entry =>
            entry.UserId == user.Id &&
            entry.Action == ChangeActionType.Delete &&
            entry.Description == null &&
            entry.Timestamp > DateTime.UtcNow.AddMinutes(-1))), Times.Once);
    }

    [Fact]
    public void LogUpdate_WhenLoggingUserUpdate_MustCallDataContextCreateForEachChange()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
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

        // Act: Invokes the method under test with the arranged parameters.
        service.LogUpdate(beforeUser, afterUser);

        // Assert: Verifies that the action of the method under test behaves as expected.
        _dataContext.Verify(s => s.Create(It.IsAny<ChangeLogEntry>()), Times.Exactly(5));

        _dataContext.Verify(s => s.Create(It.Is<ChangeLogEntry>(entry =>
            entry.UserId == afterUser.Id &&
            entry.Action == ChangeActionType.Update &&
            entry.Description != null &&
            entry.Description.Contains("Forename changed from Original to Updated"))), Times.Once);

        _dataContext.Verify(s => s.Create(It.Is<ChangeLogEntry>(entry =>
            entry.UserId == afterUser.Id &&
            entry.Action == ChangeActionType.Update &&
            entry.Description != null &&
            entry.Description.Contains("Surname changed from User to NewSurname"))), Times.Once);
    }

    [Fact]
    public void LogUpdate_WhenNoChanges_MustNotCallDataContextCreate()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
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

        // Act: Invokes the method under test with the arranged parameters.
        service.LogUpdate(user, user);

        // Assert: Verifies that the action of the method under test behaves as expected.
        _dataContext.Verify(s => s.Create(It.IsAny<ChangeLogEntry>()), Times.Never);
    }

    [Fact]
    public void GetAll_WhenRetrievingLogs_MustReturnPagedResultsOrderedByTimestamp()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var service = CreateService();
        SetupChangeLogEntries();

        // Act: Invokes the method under test with the arranged parameters.
        var result = service.GetAll(1, 2, out var totalCount);

        // Assert: Verifies that the action of the method under test behaves as expected.
        result.Should().HaveCount(2);
        totalCount.Should().Be(3);
        result.First().Timestamp.Should().BeAfter(result.Last().Timestamp);
    }

    [Fact]
    public void GetAll_WhenRetrievingSecondPage_MustReturnCorrectPagedResults()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var service = CreateService();
        SetupChangeLogEntries();

        // Act: Invokes the method under test with the arranged parameters.
        var result = service.GetAll(2, 2, out var totalCount);

        // Assert: Verifies that the action of the method under test behaves as expected.
        result.Should().HaveCount(1);
        totalCount.Should().Be(3);
    }

    [Fact]
    public void GetByUser_WhenRetrievingLogsForSpecificUser_MustReturnOnlyUserLogs()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var service = CreateService();
        var logs = SetupChangeLogEntries();

        // Act: Invokes the method under test with the arranged parameters.
        var result = service.GetByUser(1, 1, 10, out var totalCount);

        // Assert: Verifies that the action of the method under test behaves as expected.
        result.Should().HaveCount(2).And.OnlyContain(log => log.UserId == 1);
        totalCount.Should().Be(2);
    }

    [Fact]
    public void GetByUser_WhenNoLogsExistForUser_MustReturnEmptyResult()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var service = CreateService();
        SetupChangeLogEntries();

        // Act: Invokes the method under test with the arranged parameters.
        var result = service.GetByUser(999, 1, 10, out var totalCount);

        // Assert: Verifies that the action of the method under test behaves as expected.
        result.Should().BeEmpty();
        totalCount.Should().Be(0);
    }

    [Fact]
    public void GetById_WhenLogExists_MustReturnLog()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var service = CreateService();
        var expectedLog = new ChangeLogEntry
        {
            Id = 1,
            UserId = 1,
            Action = ChangeActionType.Add,
            Timestamp = DateTime.UtcNow,
            Description = null
        };
        _dataContext.Setup(s => s.GetById<ChangeLogEntry>(1L)).Returns(expectedLog);

        // Act: Invokes the method under test with the arranged parameters.
        var result = service.GetById(1);

        // Assert: Verifies that the action of the method under test behaves as expected.
        result.Should().NotBeNull().And.BeEquivalentTo(expectedLog);
        _dataContext.Verify(s => s.GetById<ChangeLogEntry>(1L), Times.Once);
    }

    [Fact]
    public void GetById_WhenLogDoesNotExist_MustReturnNull()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var service = CreateService();
        _dataContext.Setup(s => s.GetById<ChangeLogEntry>(999L)).Returns((ChangeLogEntry?)null);

        // Act: Invokes the method under test with the arranged parameters.
        var result = service.GetById(999);

        // Assert: Verifies that the action of the method under test behaves as expected.
        result.Should().BeNull();
        _dataContext.Verify(s => s.GetById<ChangeLogEntry>(999L), Times.Once);
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
    private ChangeLogService CreateService() => new(_dataContext.Object);
}

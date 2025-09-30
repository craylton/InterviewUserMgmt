using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using UserManagement.Models;

namespace UserManagement.Data.Tests;

public class ChangeLogEntryTests
{
    [Fact]
    public void Create_WhenNewChangeLogEntry_ShouldAssignId()
    {
        // Arrange
        using var context = CreateContext();
        var changeLogEntry = new ChangeLogEntry
        {
            UserId = 1,
            Timestamp = DateTime.UtcNow,
            Action = ChangeActionType.Add,
            Description = null
        };

        // Act
        context.Create(changeLogEntry);

        // Assert
        changeLogEntry.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetAll_WhenChangeLogEntriesExist_ShouldReturnAllChangeLogEntries()
    {
        // Arrange
        using var context = CreateContext();
        var changeLogEntry1 = new ChangeLogEntry
        {
            UserId = 1,
            Timestamp = DateTime.UtcNow,
            Action = ChangeActionType.Add
        };
        var changeLogEntry2 = new ChangeLogEntry
        {
            UserId = 2,
            Timestamp = DateTime.UtcNow.AddMinutes(1),
            Action = ChangeActionType.Update,
            Description = "Test update"
        };
        context.Create(changeLogEntry1);
        context.Create(changeLogEntry2);

        // Act
        var result = context.GetAll<ChangeLogEntry>().ToList();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public void GetById_WhenChangeLogEntryExists_ShouldReturnChangeLogEntry()
    {
        // Arrange
        using var context = CreateContext();
        var changeLogEntry = new ChangeLogEntry
        {
            UserId = 1,
            Timestamp = DateTime.UtcNow,
            Action = ChangeActionType.Delete
        };
        context.Create(changeLogEntry);

        // Act
        var result = context.GetById<ChangeLogEntry>(changeLogEntry.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(changeLogEntry.Id);
        result.UserId.Should().Be(changeLogEntry.UserId);
        result.Action.Should().Be(ChangeActionType.Delete);
    }

    [Fact]
    public void GetById_WhenChangeLogEntryDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var result = context.GetById<ChangeLogEntry>(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Update_WhenChangeLogEntryExists_ShouldUpdateChangeLogEntry()
    {
        // Arrange
        using var context = CreateContext();
        var changeLogEntry = new ChangeLogEntry
        {
            UserId = 1,
            Timestamp = DateTime.UtcNow,
            Action = ChangeActionType.Add,
            Description = "Original description"
        };
        context.Create(changeLogEntry);

        // Act
        changeLogEntry.Description = "Updated description";
        context.Update(changeLogEntry);

        // Assert
        var result = context.GetById<ChangeLogEntry>(changeLogEntry.Id);
        result.Should().NotBeNull();
        result!.Description.Should().Be("Updated description");
    }

    [Fact]
    public void Delete_WhenChangeLogEntryExists_ShouldRemoveChangeLogEntry()
    {
        // Arrange
        using var context = CreateContext();
        var changeLogEntry = new ChangeLogEntry
        {
            UserId = 1,
            Timestamp = DateTime.UtcNow,
            Action = ChangeActionType.Add
        };
        context.Create(changeLogEntry);

        // Act
        context.Delete(changeLogEntry);

        // Assert
        var result = context.GetById<ChangeLogEntry>(changeLogEntry.Id);
        result.Should().BeNull();
    }

    private static DataContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new DataContext(options);
    }
}

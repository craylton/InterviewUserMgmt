using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data.Entities;

namespace UserManagement.Data.Tests;

public sealed class ChangeLogEntryTests
{
    [Fact]
    public async Task Create_WhenNewChangeLogEntry_ShouldAssignId()
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
        await context.CreateAsync(changeLogEntry);

        // Assert
        changeLogEntry.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetAll_WhenChangeLogEntriesExist_ShouldReturnAllChangeLogEntries()
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
        await context.CreateAsync(changeLogEntry1);
        await context.CreateAsync(changeLogEntry2);

        // Act
        var result = context.GetAll<ChangeLogEntry>();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetById_WhenChangeLogEntryExists_ShouldReturnChangeLogEntry()
    {
        // Arrange
        using var context = CreateContext();
        var changeLogEntry = new ChangeLogEntry
        {
            UserId = 1,
            Timestamp = DateTime.UtcNow,
            Action = ChangeActionType.Delete
        };
        await context.CreateAsync(changeLogEntry);

        // Act
        var result = await context.GetByIdAsync<ChangeLogEntry>(changeLogEntry.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(changeLogEntry.Id);
        result.UserId.Should().Be(changeLogEntry.UserId);
        result.Action.Should().Be(ChangeActionType.Delete);
    }

    [Fact]
    public async Task GetById_WhenChangeLogEntryDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var result = await context.GetByIdAsync<ChangeLogEntry>(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Update_WhenChangeLogEntryExists_ShouldUpdateChangeLogEntry()
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
        await context.CreateAsync(changeLogEntry);

        // Act
        changeLogEntry.Description = "Updated description";
        await context.UpdateAndSaveAsync(changeLogEntry);

        // Assert
        var result = await context.GetByIdAsync<ChangeLogEntry>(changeLogEntry.Id);
        result.Should().NotBeNull();
        result!.Description.Should().Be("Updated description");
    }

    [Fact]
    public async Task Delete_WhenChangeLogEntryExists_ShouldRemoveChangeLogEntry()
    {
        // Arrange
        using var context = CreateContext();
        var changeLogEntry = new ChangeLogEntry
        {
            UserId = 1,
            Timestamp = DateTime.UtcNow,
            Action = ChangeActionType.Add
        };
        await context.CreateAsync(changeLogEntry);

        // Act
        await context.DeleteAsync(changeLogEntry);

        // Assert
        var result = await context.GetByIdAsync<ChangeLogEntry>(changeLogEntry.Id);
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

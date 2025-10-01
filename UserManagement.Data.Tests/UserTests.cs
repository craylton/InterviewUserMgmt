using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data.Entities;

namespace UserManagement.Data.Tests;

public sealed class UserTests
{
    [Fact]
    public void GetAll_WhenNewEntityAdded_ShouldIncludeNewEntity()
    {
        // Arrange
        using var context = CreateContext();

        var entity = new User
        {
            Forename = "Brand New",
            Surname = "User",
            Email = "brandnewuser@example.com",
            IsActive = true,
            DateOfBirth = new DateTime(1990, 5, 10)
        };
        context.Create(entity);

        // Act
        var result = context.GetAll<User>();

        // Assert
        result
            .Should().Contain(s => s.Email == entity.Email)
            .Which.Should().BeEquivalentTo(entity);
    }

    [Fact]
    public void GetAll_WhenDeleted_ShouldNotIncludeDeletedEntity()
    {
        // Arrange
        using var context = CreateContext();
        var entity = context.GetAll<User>().First();
        context.Delete(entity);

        // Act
        var result = context.GetAll<User>();

        // Assert
        result.Should().NotContain(s => s.Email == entity.Email);
    }

    [Fact]
    public void GetById_WhenEntityExists_ShouldReturnEntity()
    {
        // Arrange
        using var context = CreateContext();
        var entity = context.GetAll<User>().First();

        // Act
        var result = context.GetById<User>(entity.Id);

        // Assert
        result.Should().NotBeNull().And.BeEquivalentTo(entity);
    }

    [Fact]
    public void GetById_WhenEntityDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        using var context = CreateContext();
        var nonExistentId = 999;

        // Act
        var result = context.GetById<User>(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Update_WhenEntityExists_ShouldUpdateEntity()
    {
        // Arrange
        using var context = CreateContext();
        var entity = context.GetAll<User>().First();

        var originalSurname = entity.Surname;
        var originalEmail = entity.Email;
        var originalIsActive = entity.IsActive;
        var originalDob = entity.DateOfBirth;

        entity.Forename = "Updated";

        // Act
        context.Update(entity);

        // Assert
        // Detach to ensure we re-query from the store rather than returning the tracked instance
        context.Entry(entity).State = EntityState.Detached;
        var updatedEntity = context.GetById<User>(entity.Id);

        updatedEntity.Should().NotBeNull();
        updatedEntity!.Forename.Should().Be("Updated");
        updatedEntity.Surname.Should().Be(originalSurname);
        updatedEntity.Email.Should().Be(originalEmail);
        updatedEntity.IsActive.Should().Be(originalIsActive);
        updatedEntity.DateOfBirth.Should().Be(originalDob);
    }

    private static DataContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new DataContext(options);
    }
}

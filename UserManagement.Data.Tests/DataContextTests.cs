using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data.Entities;

namespace UserManagement.Data.Tests;

public class DataContextTests
{
    [Fact]
    public void GetAll_WhenNewEntityAdded_MustIncludeNewEntity()
    {
        // Arrange
        var context = CreateContext();

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
    public void GetAll_WhenDeleted_MustNotIncludeDeletedEntity()
    {
        // Arrange
        var context = CreateContext();
        var entity = context.GetAll<User>().First();
        context.Delete(entity);

        // Act
        var result = context.GetAll<User>();

        // Assert
        result.Should().NotContain(s => s.Email == entity.Email);
    }

    [Fact]
    public void GetById_WhenEntityExists_MustReturnEntity()
    {
        // Arrange
        var context = CreateContext();
        var entity = context.GetAll<User>().First();

        // Act
        var result = context.GetById<User>(entity.Id);

        // Assert
        result.Should().NotBeNull().And.BeEquivalentTo(entity);
    }

    [Fact]
    public void GetById_WhenEntityDoesNotExist_MustReturnNull()
    {
        // Arrange
        var context = CreateContext();
        var nonExistentId = 999;

        // Act
        var result = context.GetById<User>(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Update_WhenEntityExists_MustUpdateEntity()
    {
        // Arrange
        var context = CreateContext();
        var entity = context.GetAll<User>().First();

        entity.Forename = "Updated";

        // Act
        context.Update(entity);

        // Assert
        var updatedEntity = context.GetById<User>(entity.Id);

        updatedEntity.Should().NotBeNull()
            .And.BeEquivalentTo(entity, options => options.Excluding(e => e.Forename));

        updatedEntity.Forename.Should().Be("Updated");
    }

    private static DataContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new DataContext(options);
    }
}

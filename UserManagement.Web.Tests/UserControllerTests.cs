using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Web.Models.Users;
using UserManagement.WebMS.Controllers;

namespace UserManagement.Data.Tests;

public class UserControllerTests
{
    [Fact]
    public void List_WhenServiceReturnsUsers_ModelMustContainUsers()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var controller = CreateController();
        var users = SetupUsers();

        // Act: Invokes the method under test with the arranged parameters.
        var result = controller.List();

        // Assert: Verifies that the action of the method under test behaves as expected.
        result.Model
            .Should().BeOfType<UserListViewModel>()
            .Which.Items.Should().BeEquivalentTo(users);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void List_WhenServiceFiltersUsers_ModelMustContainFilteredUsers(bool isActive)
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var controller = CreateController();
        var users = SetupUsers(isActive: isActive);

        // Act: Invokes the method under test with the arranged parameters.
        var result = controller.List(isActive);

        // Assert: Verifies that the action of the method under test behaves as expected.
        result.Model
            .Should().BeOfType<UserListViewModel>()
            .Which.Items.Should().BeEquivalentTo(users);
    }

    [Fact]
    public void Add_WhenGetRequest_ReturnsViewWithEmptyUserViewModel()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var controller = CreateController();

        // Act: Invokes the method under test with the arranged parameters.
        var result = controller.Add();

        // Assert: Verifies that the action of the method under test behaves as expected.
        result.Should().BeOfType<ViewResult>()
            .Which.Model.Should().BeOfType<UserViewModel>()
            .Which.Should().BeEquivalentTo(new UserViewModel());
    }

    [Fact]
    public void Add_WhenValidModel_CreatesUserAndRedirectsToList()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var controller = CreateController();
        var userViewModel = new UserViewModel
        {
            Id = 1,
            Forename = "John",
            Surname = "Doe",
            Email = "john.doe@example.com",
            IsActive = true,
            DateOfBirth = new DateTime(1990, 5, 15)
        };

        // Act: Invokes the method under test with the arranged parameters.
        var result = controller.Add(userViewModel);

        // Assert: Verifies that the action of the method under test behaves as expected.
        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be(nameof(UsersController.List));

        _userService.Verify(s => s.Create(It.Is<User>(u =>
            u.Id == userViewModel.Id &&
            u.Forename == userViewModel.Forename &&
            u.Surname == userViewModel.Surname &&
            u.Email == userViewModel.Email &&
            u.IsActive == userViewModel.IsActive &&
            u.DateOfBirth == userViewModel.DateOfBirth)), Times.Once);
    }

    [Fact]
    public void Add_WhenInvalidModel_ReturnsViewWithSameModel()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var controller = CreateController();
        var userViewModel = new UserViewModel
        {
            Id = 1,
            // Missing required fields to make model invalid
            Email = "invalid-email"
        };

        // Simulate invalid ModelState
        controller.ModelState.AddModelError("Email", "Invalid email address.");

        // Act: Invokes the method under test with the arranged parameters.
        var result = controller.Add(userViewModel);

        // Assert: Verifies that the action of the method under test behaves as expected.
        result.Should().BeOfType<ViewResult>()
            .Which.Model.Should().BeEquivalentTo(userViewModel);

        _userService.Verify(s => s.Create(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public void View_WhenUserNotFound_ReturnsNotFound()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var controller = CreateController();
        const long userId = 999;

        _userService
            .Setup(s => s.GetById(userId))
            .Returns((User?)null);

        // Act: Invokes the method under test with the arranged parameters.
        var result = controller.View(userId);

        // Assert: Verifies that the action of the method under test behaves as expected.
        result.Should().BeOfType<NotFoundResult>();

        _userService.Verify(s => s.GetById(userId), Times.Once);
    }

    [Fact]
    public void View_WhenUserExists_ReturnsViewWithUserViewModel()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var controller = CreateController();
        var user = SetupUsers().First();

        _userService
            .Setup(s => s.GetById(user.Id))
            .Returns(user);

        // Act: Invokes the method under test with the arranged parameters.
        var result = controller.View(user.Id);

        // Assert: Verifies that the action of the method under test behaves as expected.
        result.Should().BeOfType<ViewResult>()
            .Which.Model.Should().BeOfType<UserViewModel>()
            .Which.Should().BeEquivalentTo(new UserViewModel
            {
                Id = user.Id,
                Forename = user.Forename,
                Surname = user.Surname,
                Email = user.Email,
                IsActive = user.IsActive,
                DateOfBirth = user.DateOfBirth
            });

        _userService.Verify(s => s.GetById(user.Id), Times.Once);
    }

    private User[] SetupUsers(string forename = "Johnny", string surname = "User", string email = "juser@example.com", bool isActive = true)
    {
        var users = new[]
        {
            new User
            {
                Forename = forename,
                Surname = surname,
                Email = email,
                IsActive = isActive,
                DateOfBirth = new DateTime(1990, 1, 15)
            }
        };

        _userService
            .Setup(s => s.GetAll())
            .Returns(users);

        _userService
            .Setup(s => s.FilterByActive(isActive))
            .Returns(users);

        return users;
    }

    private readonly Mock<IUserService> _userService = new();
    private UsersController CreateController() => new(_userService.Object);
}

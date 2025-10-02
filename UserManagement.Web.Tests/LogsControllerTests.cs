using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Data.Entities;
using UserManagement.Services.Interfaces;
using UserManagement.Web.Controllers;
using UserManagement.Web.Models.Logs;

namespace UserManagement.Web.Tests;

public sealed class LogsControllerTests
{
    [Fact]
    public void List_WhenLogsExist_ShouldReturnViewWithLogListViewModel()
    {
        // Arrange
        var controller = CreateController();
        var logs = new[]
        {
            new ChangeLogEntry { Id = 1, UserId = 1, Action = ChangeActionType.Add, Timestamp = DateTime.UtcNow },
            new ChangeLogEntry { Id = 2, UserId = 2, Action = ChangeActionType.Update, Timestamp = DateTime.UtcNow.AddMinutes(1) }
        };

        _changeLogService.Setup(s => s.GetAll(1, 10, out It.Ref<int>.IsAny))
            .Returns((int page, int pageSize, out int totalCount) =>
            {
                totalCount = 2;
                return logs;
            });

        // Act
        var result = controller.List(1);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<LogListViewModel>();

        var model = (LogListViewModel)viewResult.Model!;
        model.Items.Should().HaveCount(2);
        model.PageNumber.Should().Be(1);
        model.PageSize.Should().Be(10);
        model.TotalCount.Should().Be(2);
    }

    [Fact]
    public void List_WhenNoLogsExist_ShouldReturnViewWithEmptyLogListViewModel()
    {
        // Arrange
        var controller = CreateController();

        _changeLogService.Setup(s => s.GetAll(1, 10, out It.Ref<int>.IsAny))
            .Returns((int page, int pageSize, out int totalCount) =>
            {
                totalCount = 0;
                return new List<ChangeLogEntry>();
            });

        // Act
        var result = controller.List(1);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<LogListViewModel>();

        var model = (LogListViewModel)viewResult.Model!;
        model.Items.Should().BeEmpty();
        model.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task View_WhenLogExists_ShouldReturnViewWithLogDetailViewModel()
    {
        // Arrange
        var controller = CreateController();
        var log = new ChangeLogEntry
        {
            Id = 1,
            UserId = 1,
            Action = ChangeActionType.Update,
            Timestamp = DateTime.UtcNow,
            Description = "Test description"
        };

        _changeLogService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(log);

        // Act
        var result = await controller.ViewAsync(1, "/users/1");

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<LogDetailViewModel>();

        var model = (LogDetailViewModel)viewResult.Model!;
        model.Id.Should().Be(1);
        model.UserId.Should().Be(1);
        model.Action.Should().Be(ChangeActionType.Update);
        model.Description.Should().Be("Test description");
        model.ReturnTo.Should().Be("/users/1");
    }

    [Fact]
    public async Task View_WhenLogDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var controller = CreateController();
        _changeLogService.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((ChangeLogEntry?)null);

        // Act
        var result = await controller.ViewAsync(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task View_WhenReturnToIsNull_ShouldSetReturnToAsNull()
    {
        // Arrange
        var controller = CreateController();
        var log = new ChangeLogEntry
        {
            Id = 1,
            UserId = 1,
            Action = ChangeActionType.Add,
            Timestamp = DateTime.UtcNow
        };

        _changeLogService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(log);

        // Act
        var result = await controller.ViewAsync(1);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        var model = (LogDetailViewModel)viewResult.Model!;
        model.ReturnTo.Should().BeNull();
    }

    private readonly Mock<IChangeLogService> _changeLogService = new();
    private LogsController CreateController() => new(_changeLogService.Object);
}

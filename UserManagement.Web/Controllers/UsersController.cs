using System.Linq;
using UserManagement.Web.Models.Users;
using UserManagement.Web.Models.Logs;
using UserManagement.Services.Interfaces;
using UserManagement.Data.Entities;

namespace UserManagement.Web.Controllers;

[Route("users")]
public class UsersController(IUserService userService, IChangeLogService changeLogService) : Controller
{
    private const int _defaultPageSize = 10;

    [HttpGet]
    public IActionResult List([FromQuery(Name = "isActive")] bool? isActive = null)
    {
        var filteredUsers = isActive.HasValue ? userService.FilterByActive(isActive.Value) : userService.GetAll();
        var items = filteredUsers.Select(p => new UserViewModel
        {
            Id = p.Id,
            Forename = p.Forename,
            Surname = p.Surname,
            Email = p.Email,
            IsActive = p.IsActive,
            DateOfBirth = p.DateOfBirth
        });

        var model = new UserListViewModel
        {
            Items = [.. items]
        };

        return View(model);
    }

    [HttpGet("add")]
    public IActionResult Add() => View(new UserViewModel());

    [HttpPost("add")]
    [ValidateAntiForgeryToken]
    public IActionResult Add(UserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new User
        {
            Forename = model.Forename,
            Surname = model.Surname,
            Email = model.Email,
            IsActive = model.IsActive,
            DateOfBirth = model.DateOfBirth
        };

        userService.Create(user);
        return RedirectToAction(nameof(List));
    }

    [HttpGet("{id}")]
    public IActionResult View(long id, int page = 1)
    {
        if (userService.GetById(id) is not User user)
        {
            return NotFound();
        }

        var userViewModel = new UserViewModel
        {
            Id = user.Id,
            Forename = user.Forename,
            Surname = user.Surname,
            Email = user.Email,
            IsActive = user.IsActive,
            DateOfBirth = user.DateOfBirth
        };

        var logs = changeLogService.GetByUser(id, page, _defaultPageSize, out var totalCount);
        var logItems = logs.Select(l => new LogListItemViewModel
        {
            Id = l.Id,
            UserId = l.UserId,
            Action = l.Action,
            Timestamp = l.Timestamp
        });

        var logsViewModel = new LogListViewModel
        {
            Items = logItems,
            PageNumber = page,
            PageSize = _defaultPageSize,
            TotalCount = totalCount
        };

        var model = new UserDetailsViewModel
        {
            User = userViewModel,
            Logs = logsViewModel
        };

        return View(model);
    }

    [HttpGet("edit/{id}")]
    public IActionResult Edit(long id)
    {
        if (userService.GetById(id) is not User user)
        {
            return NotFound();
        }

        var model = new UserViewModel
        {
            Id = user.Id,
            Forename = user.Forename,
            Surname = user.Surname,
            Email = user.Email,
            IsActive = user.IsActive,
            DateOfBirth = user.DateOfBirth
        };

        return View(model);
    }

    [HttpPost("edit/{id}")]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(long id, UserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new User
        {
            Id = id,
            Forename = model.Forename,
            Surname = model.Surname,
            Email = model.Email,
            IsActive = model.IsActive,
            DateOfBirth = model.DateOfBirth
        };

        userService.Update(user);
        return RedirectToAction(nameof(List));
    }

    [HttpGet("delete/{id}")]
    public IActionResult Delete(long id)
    {
        if (userService.GetById(id) is not User user)
        {
            return NotFound();
        }

        var model = new UserViewModel
        {
            Id = user.Id,
            Forename = user.Forename,
            Surname = user.Surname,
            Email = user.Email,
            IsActive = user.IsActive,
            DateOfBirth = user.DateOfBirth
        };

        return View(model);
    }

    [HttpPost("delete/{id}")]
    [ValidateAntiForgeryToken]
    public IActionResult PostDelete(long id)
    {
        if (userService.GetById(id) is not User user)
        {
            return NotFound();
        }

        userService.Delete(user);
        return RedirectToAction(nameof(List));
    }
}

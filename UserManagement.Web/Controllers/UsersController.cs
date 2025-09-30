using System.Linq;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Web.Models.Users;
using UserManagement.Web.Models.Logs;

namespace UserManagement.WebMS.Controllers;

[Route("users")]
public class UsersController : Controller
{
    private readonly IUserService _userService;
    private readonly IChangeLogService _changeLogService;

    public UsersController(IUserService userService, IChangeLogService changeLogService)
    {
        _userService = userService;
        _changeLogService = changeLogService;
    }

    [HttpGet]
    public ViewResult List([FromQuery(Name = "isActive")] bool? isActive = null)
    {
        var filteredUsers = isActive.HasValue ? _userService.FilterByActive(isActive.Value) : _userService.GetAll();
        var items = filteredUsers.Select(p => new UserListItemViewModel
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
            Items = items.ToList()
        };

        return View(model);
    }

    [HttpGet("add")]
    public IActionResult Add() => View(new UserViewModel());

    [HttpPost("add")]
    public IActionResult Add(UserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new User
        {
            Id = model.Id,
            Forename = model.Forename,
            Surname = model.Surname,
            Email = model.Email,
            IsActive = model.IsActive,
            DateOfBirth = model.DateOfBirth
        };

        _userService.Create(user);
        return RedirectToAction(nameof(List));
    }

    [HttpGet("{id}")]
    public IActionResult View(long id, int page = 1)
    {
        if (_userService.GetById(id) is not User user)
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

        var logs = _changeLogService.GetByUser(id, page, 10, out var totalCount);
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
            PageSize = 10,
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
        if (_userService.GetById(id) is not User user)
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

        _userService.Update(user);
        return RedirectToAction(nameof(List));
    }

    [HttpGet("delete/{id}")]
    public IActionResult Delete(long id)
    {
        if (_userService.GetById(id) is not User user)
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
    public IActionResult Delete(long id, UserViewModel model)
    {
        var user = new User
        {
            Id = model.Id,
            Forename = model.Forename,
            Surname = model.Surname,
            Email = model.Email,
            IsActive = model.IsActive,
            DateOfBirth = model.DateOfBirth
        };

        _userService.Delete(user);
        return RedirectToAction(nameof(List));
    }
}

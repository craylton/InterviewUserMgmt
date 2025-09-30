using System.Linq;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Web.Models.Logs;

namespace UserManagement.WebMS.Controllers;

[Route("logs")]
public class LogsController : Controller
{
    private readonly IChangeLogService _changeLogService;

    public LogsController(IChangeLogService changeLogService) => _changeLogService = changeLogService;

    [HttpGet]
    public ViewResult List(int page = 1)
    {
        var logs = _changeLogService.GetAll(page, 10, out var totalCount);
        var items = logs.Select(l => new LogListItemViewModel
        {
            Id = l.Id,
            UserId = l.UserId,
            Action = l.Action,
            Timestamp = l.Timestamp
        });

        var model = new LogListViewModel
        {
            Items = items,
            PageNumber = page,
            PageSize = 10,
            TotalCount = totalCount
        };

        return View(model);
    }

    [HttpGet("{id}")]
    public IActionResult View(long id, string? returnTo = null)
    {
        var log = _changeLogService.GetById(id);
        if (log == null)
        {
            return NotFound();
        }

        var model = new LogDetailViewModel
        {
            Id = log.Id,
            UserId = log.UserId,
            Action = log.Action,
            Timestamp = log.Timestamp,
            Description = log.Description,
            ReturnTo = returnTo
        };

        return View(model);
    }
}

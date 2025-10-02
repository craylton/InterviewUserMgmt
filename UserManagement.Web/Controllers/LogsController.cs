using System.Linq;
using System.Threading.Tasks;
using UserManagement.Services.Interfaces;
using UserManagement.Web.Models.Logs;

namespace UserManagement.Web.Controllers;

[Route("logs")]
public class LogsController(IChangeLogService changeLogService) : Controller
{
    private const int _defaultPageSize = 10;

    [HttpGet]
    public IActionResult List(int page = 1)
    {
        var logs = changeLogService.GetAll(page, _defaultPageSize, out var totalCount);

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
            PageSize = _defaultPageSize,
            TotalCount = totalCount
        };

        return View(model);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ViewAsync(long id, string? returnTo = null)
    {
        var log = await changeLogService.GetByIdAsync(id);

        if (log is null)
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

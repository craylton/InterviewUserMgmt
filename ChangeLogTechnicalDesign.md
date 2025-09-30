# Section 4 – Change Log Technical Design (TDD-first)

## Current Codebase (brief)
- Architecture: Clean layering with Data (EF Core InMemory), Services (Domain), Web (MVC controllers + Razor Views), and unit tests per layer.
- Data: `DataContext` exposes generic CRUD (`GetAll<T>`, `Create<T>`, `Update<T>`, `Delete<T>`, `GetById<T>`). Seed data for `User` entity. Immediate `SaveChanges` inside each operation.
- Services: `UserService` is a thin wrapper over `IDataContext` with a small filter method.
- Web: `UsersController` implements CRUD; strongly-typed view models and Razor views for List/Add/Edit/Delete/View. No pagination utilities yet.
- Tests: Solid AAA tests in `*.Tests` projects for data, services, and web controllers.

Note: Although the repository mentions Razor Pages, the web layer uses MVC controllers + views. The solution below preserves this paradigm and the project’s naming/coding style.

## Requirements (focused)
- Persist a `ChangeLogEntry` per user mutation:
  - Action types: Add, Update (one entry per field change), Delete
  - Fields: UserId, Timestamp, ActionType, Description (only for Update entries)
- UI:
  - On `Users/View/{id}`: show a simplified, paginated list (timestamp + action) with clickable entries
  - Dedicated `/logs` page: all logs, paginated (10/page)
  - Log detail page: full info and description(s), with a back button that respects the origin
- Pagination: 10 logs per page on both views
- Tests remain first-class citizens; new functionality comes with unit tests and updated expectations.

## Recommended Approach
Approach 2 (Service Layer Integration with Manual Logging) is the best fit for this codebase.
- Minimally invasive: works with existing `IDataContext` generics and DI.
- Explicit control and simple manual field comparisons for `User` changes.
- Predictable, testable behaviors; easy to verify log count and content.
- Aligns with existing patterns (thin services + cohesive responsibilities).

## High-level Solution
1. Data model
   - Add `ChangeLogEntry` entity (in `UserManagement.Models`) and `ChangeActionType` enum.
   - Add `DbSet<ChangeLogEntry>` on `DataContext`. No changes to `IDataContext` needed beyond existing generics.
2. Services
   - Add `IChangeLogService` and `ChangeLogService` to encapsulate:
     - Logging add/update/delete
     - Manual field comparison for `User` (description: e.g. "Surname changed from Smith to Johnson")
     - Query APIs for logs with server-side pagination (Skip/Take ordered by Timestamp desc)
   - Integrate into `UserService`:
     - Create: after `_dataContext.Create(user)` call, write one Add log
     - Update: obtain current persisted `User` (if available), compute per-field changes; call `_dataContext.Update(user)`; write one Update log per changed field
     - Delete: log one Delete entry (before or after delete)
     - Logging must be best-effort: exceptions are caught and do not block user operations
3. Web layer
   - New `LogsController` with routes:
     - `GET /logs?page=1` – list all logs (10/page)
     - `GET /logs/{id}?returnTo=...` – details for a single log entry; render a back button honoring `returnTo`
   - Update `UsersController.View(long id)` to include a paginated list of that user’s logs (10/page)
   - View models for logs and shared pagination model
   - Razor views: list and detail for logs, and add logs section to `Users/View.cshtml`
4. Pagination
   - Server-side only; page size = 10
   - Simple view model fields: `PageNumber`, `PageSize`, `TotalCount`, `TotalPages`, plus `Items`

## Detailed Changes

### Data Layer
- Create entity `ChangeLogEntry` and enum `ChangeActionType` in `UserManagement.Models` (follow `User` entity conventions):
  - Properties: `Id (long, identity)`, `UserId (long)`, `Timestamp (DateTime)`, `Action (ChangeActionType)`, `Description (string?, nullable)`, optional `User` nav prop
  - Enum: `Add = 0`, `Update = 1`, `Delete = 2`
- `DataContext`
  - Add `DbSet<ChangeLogEntry>? ChangeLogs { get; set; }`
  - No change to existing generic methods; they already support `ChangeLogEntry`
- Tests
  - New tests in `UserManagement.Data.Tests` to verify CRUD for `ChangeLogEntry`

Example entity snippet:
```csharp
namespace UserManagement.Models
{
    public enum ChangeActionType { Add, Update, Delete }

    public class ChangeLogEntry
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public long UserId { get; set; }
        public DateTime Timestamp { get; set; }
        public ChangeActionType Action { get; set; }
        public string? Description { get; set; }

        public User? User { get; set; }
    }
}
```

### Services Layer
- Interfaces
  - New `IChangeLogService` with:
    - `void LogAdd(User user)`
    - `void LogDelete(User user)`
    - `void LogUpdate(User before, User after)` – emits one entry per changed field
    - `IEnumerable<ChangeLogEntry> GetAll(int pageNumber, int pageSize, out int totalCount)`
    - `IEnumerable<ChangeLogEntry> GetByUser(long userId, int pageNumber, int pageSize, out int totalCount)`
    - `ChangeLogEntry? GetById(long id)`
- Implementation
  - `ChangeLogService` uses `IDataContext` generics for persistence and queries; orders by `Timestamp` desc; `Skip/Take` for pagination; wraps log writes in `try/catch`
  - Manual comparison for fields `Forename`, `Surname`, `Email`, `IsActive`, `DateOfBirth`
- Integrate into `UserService`
  - Inject `IChangeLogService` via constructor
  - `Create(User user)`:
    - `_dataContext.Create(user)`
    - `try { _changeLog.LogAdd(user); } catch { /* swallow */ }`
  - `Update(User user)`:
    - `var existing = _dataContext.GetById<User>(user.Id);`
    - compute differences if `existing != null`
    - `_dataContext.Update(user)`
    - `try { if (existing != null) _changeLog.LogUpdate(existing, user); } catch { }`
  - `Delete(User user)`:
    - Option A: log-before-delete; Option B: log-after-delete (either fine since we only need Id)
    - `_dataContext.Delete(user)`
    - `try { _changeLog.LogDelete(user); } catch { }`
- DI registration
  - Extend `AddDomainServices()` to register `IChangeLogService`
- Tests
  - New `ChangeLogServiceTests` verifying:
    - Add/Delete produce one entry with correct `Action`, `UserId`, and `Timestamp`
    - Update produces N entries for N changed fields; check `Description` text
    - Pagination returns 10 items and correct `totalCount`
  - Update `UserServiceTests` to construct `UserService` with a mock `IChangeLogService` and assert `_dataContext.Update/Create/Delete` still called once. Optionally verify logging calls (not strictly required, but recommended).

Example comparison snippet (conceptual):
```csharp
var diffs = new List<string>();
if (before.Surname != after.Surname)
    diffs.Add($"Surname changed from {before.Surname} to {after.Surname}");
// repeat for Forename, Email, IsActive, DateOfBirth
foreach (var d in diffs)
{
    _dataContext.Create(new ChangeLogEntry
    {
        UserId = after.Id,
        Timestamp = DateTime.UtcNow,
        Action = ChangeActionType.Update,
        Description = d
    });
}
```

### Web Layer
- Controllers
  - New `LogsController` (namespace consistent with `UsersController`) with actions:
    - `List(int page = 1)` returns `LogListViewModel` for all logs (10/page)
    - `View(long id, string? returnTo = null)` returns `LogDetailViewModel` and stores `returnTo` for the back button
  - Update `UsersController.View(long id)` to also query `IChangeLogService.GetByUser(id, page, 10, out total)` and pass logs via a new combined view model
- View Models (new)
  - `LogListItemViewModel` { `Id`, `UserId`, `Action`, `Timestamp` }
  - `LogDetailViewModel` { `Id`, `UserId`, `Action`, `Timestamp`, `Description` }
  - `LogListViewModel` : paging + `List<LogListItemViewModel>`
  - `UserDetailsViewModel` (for `Users/View`) combining `UserViewModel` + paged `List<LogListItemViewModel>`
- Views
  - `Views/Logs/List.cshtml` – table with 10 rows/page, pager controls
  - `Views/Logs/View.cshtml` – detail with back button
  - Update `Views/Users/View.cshtml` – add logs section (timestamp + action, links to details). Include pagination controls for the user logs
  - Optional: `Views/Shared/_Pagination.cshtml` partial reused by both pages
- Routing
  - Keep attribute routing style consistent with `UsersController`
- Tests
  - New `LogsControllerTests` covering list, pagination, and details
  - Update `UserControllerTests` for `View` to expect the new combined model with logs (and verify page size 10)

Example controller snippet (conceptual):
```csharp
[HttpGet("{id}")]
public IActionResult View(long id, int page = 1)
{
    var user = _userService.GetById(id);
    if (user is null) return NotFound();

    var items = _changeLogService.GetByUser(id, page, 10, out var total);
    var model = new UserDetailsViewModel
    {
        User = Map(user),
        Logs = items.Select(Map),
        PageNumber = page,
        PageSize = 10,
        TotalCount = total
    };
    return View(model);
}
```

### Naming and Style
- Keep namespaces as in the repo:
  - Entities and enums in `UserManagement.Models`
  - Services in `UserManagement.Services.Domain.*`
  - Controllers in `UserManagement.WebMS.Controllers`
- Follow existing controller attribute routing and method naming
- Validation and display attributes on view models consistent with current code
- Minimal new abstractions; reuse existing patterns and helpers

## Files to Add/Change
- Data (add)
  - `UserManagement.Data/Entities/ChangeLogEntry.cs`
  - `UserManagement.Data/Entities/ChangeActionType.cs` (or embed enum in the same file)
  - `UserManagement.Data/DataContext.cs` (update: `DbSet<ChangeLogEntry>`)
  - `UserManagement.Data.Tests/ChangeLogEntryTests.cs` (CRUD + queries)
- Services (add/change)
  - `UserManagement.Services/Interfaces/IChangeLogService.cs`
  - `UserManagement.Services/Implementations/ChangeLogService.cs`
  - `UserManagement.Services/Implementations/UserService.cs` (update: inject and call `IChangeLogService`)
  - `UserManagement.Services/Extensions/ServiceCollectionExtensions.cs` (update: register `IChangeLogService`)
  - `UserManagement.Services.Tests/ChangeLogServiceTests.cs`
  - `UserManagement.Services.Tests/UserServiceTests.cs` (update constructor + optional logging verifications)
- Web (add/change)
  - `UserManagement.Web/Controllers/LogsController.cs`
  - `UserManagement.Web/Models/Logs/LogListViewModel.cs`
  - `UserManagement.Web/Models/Logs/LogDetailViewModel.cs`
  - `UserManagement.Web/Models/Users/UserDetailsViewModel.cs` (wrapper for user + logs)
  - `UserManagement.Web/Views/Logs/List.cshtml`
  - `UserManagement.Web/Views/Logs/View.cshtml`
  - `UserManagement.Web/Views/Shared/_Pagination.cshtml` (optional)
  - `UserManagement.Web/Views/Users/View.cshtml` (update: logs section + paging)
  - `UserManagement.Web.Tests/LogsControllerTests.cs`
  - `UserManagement.Web.Tests/UserControllerTests.cs` (update expectations for `View`)

## Test Strategy (high level)
- Data
  - Verify `Create`/`GetAll`/`GetById` for `ChangeLogEntry`
- Services
  - `ChangeLogService` add/delete/update behaviors; per-field update entries and descriptions; pagination shape
  - `UserService` still delegates to `IDataContext` correctly and calls logging (best-effort)
- Web
  - `LogsController` list paginates to 10; detail returns correct model; back button URL present via view model
  - `UsersController.View` returns combined model with 10 logs/page

## Notes and Trade-offs
- Transactions: `DataContext` saves on every call; logging calls are independent by design per requirement (“log failures should not impact user operations”). We’ll swallow and optionally log exceptions internally.
- In-memory provider: acceptable for the task; logs are transient across runs.
- Performance: Manual comparisons are O(1) for the fixed fields; pagination queries keep lists small.
- UX: No mobile-specific requirements; pagination kept simple. Back button honors a `returnTo` query (e.g. `/users/{id}` or `/logs?page=n`).

---
This plan preserves the project’s architecture and naming conventions, adds the minimum necessary abstractions, and is designed to be implemented test-first with small, verifiable increments.

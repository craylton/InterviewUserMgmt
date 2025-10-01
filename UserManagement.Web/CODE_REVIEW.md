# Code quality observations – UserManagement.Web

Goal: keep functionality as-is; highlight actual problems and high-impact consistency issues.

Definite issues / risks
- Missing anti-forgery on POST actions
  - `UsersController` and `LogsController` POST endpoints don’t use `[ValidateAntiForgeryToken]`. This is a real security risk for CSRF. Add the attribute and ensure forms emit the token.

- Delete POST trusts model Id over route Id
  - `UsersController.Delete(long id, UserViewModel model)` ignores the route `id` and uses `model.Id`. This can be tampered with. Prefer using the route `id` and, optionally, assert it matches the posted model.

- Create POST allows client-supplied Id
  - `UsersController.Add` sets `Id = model.Id` when creating. If persistence uses identity keys, this is at best unnecessary and at worst dangerous. The server should ignore Id on create.

- Inconsistent validation on POST
  - `Delete` POST does not check `ModelState.IsValid` or otherwise validate inputs. Standardize validation and/or simplify the action to only require the route Id.

High-impact consistency fixes (non-functional but valuable)
- Routing redundancy
  - The app uses attribute routing on controllers and also `app.MapDefaultControllerRoute()`. Pick one approach to avoid duplicate/ambiguous routes. If keeping attributes, use `app.MapControllers()`.

- Inconsistent action return types
  - Some actions return `ViewResult`, others `IActionResult`. Standardize on `IActionResult` (common in MVC) for readability.

- Repeated page size literal
  - The page size `10` is duplicated. Centralize as a constant (e.g., `const int DefaultPageSize = 10;`) to reduce drift.

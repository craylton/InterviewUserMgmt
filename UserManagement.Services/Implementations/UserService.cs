using System.Linq;
using UserManagement.Data;
using UserManagement.Data.Entities;
using UserManagement.Services.Interfaces;

namespace UserManagement.Services.Implementations;

public class UserService(IDataContext dataAccess, IChangeLogService changeLogService) : IUserService
{
    public IQueryable<User> FilterByActive(bool isActive) =>
        dataAccess.GetAll<User>().Where(u => u.IsActive == isActive);

    public IQueryable<User> GetAll() => dataAccess.GetAll<User>();

    public void Create(User user)
    {
        dataAccess.Create(user);
        changeLogService.LogAdd(user);
    }

    public User? GetById(long id) => dataAccess.GetById<User>(id);

    public void Update(User user)
    {
        // Get existing user for comparison without tracking to avoid conflicts
        var existing = dataAccess.GetByIdNoTracking<User>(user.Id);

        dataAccess.UpdateAndSave(user);

        if (existing is not null)
        {
            changeLogService.LogUpdate(existing, user);
        }
    }

    public void Delete(User user)
    {
        changeLogService.LogDelete(user);
        dataAccess.Delete(user);
    }
}

using System.Linq;
using System.Threading.Tasks;
using UserManagement.Data;
using UserManagement.Data.Entities;
using UserManagement.Services.Interfaces;

namespace UserManagement.Services.Implementations;

public class UserService(IDataContext dataAccess, IChangeLogService changeLogService) : IUserService
{
    public IQueryable<User> FilterByActive(bool isActive) =>
        dataAccess.GetAll<User>().Where(u => u.IsActive == isActive);

    public IQueryable<User> GetAll() => dataAccess.GetAll<User>();

    public async Task CreateAsync(User user)
    {
        await dataAccess.CreateAsync(user);
        await changeLogService.LogAddAsync(user);
    }

    public async Task<User?> GetByIdAsync(long id) => await dataAccess.GetByIdAsync<User>(id);

    public async Task UpdateAsync(User user)
    {
        // Get existing user for comparison without tracking to avoid conflicts
        var existing = await dataAccess.GetByIdNoTrackingAsync<User>(user.Id);

        await dataAccess.UpdateAndSaveAsync(user);

        if (existing is not null)
        {
            await changeLogService.LogUpdateAsync(existing, user);
        }
    }

    public async Task DeleteAsync(User user)
    {
        await changeLogService.LogDeleteAsync(user);
        await dataAccess.DeleteAsync(user);
    }
}

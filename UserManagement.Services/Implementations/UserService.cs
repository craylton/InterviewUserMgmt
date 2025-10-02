using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UserManagement.Data;
using UserManagement.Data.Entities;
using UserManagement.Services.Interfaces;

namespace UserManagement.Services.Implementations;

public class UserService(IDataContext dataAccess, IChangeLogService changeLogService, ILogger<UserService> logger) : IUserService
{
    public IQueryable<User> FilterByActive(bool isActive) =>
        dataAccess.GetAll<User>().Where(u => u.IsActive == isActive);

    public IQueryable<User> GetAll() => dataAccess.GetAll<User>();

    public async Task CreateAsync(User user)
    {
        try
        {
            await dataAccess.CreateAsync(user);
            await changeLogService.LogAddAsync(user);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating a user");
            throw;
        }
    }

    public async Task<User?> GetByIdAsync(long id)
    {
        try
        {
            return await dataAccess.GetByIdAsync<User>(id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while retrieving user with ID {UserId}", id);
            throw;
        }
    }

    public async Task UpdateAsync(User user)
    {
        try
        {
            // Get existing user for comparison without tracking to avoid conflicts
            var existing = await dataAccess.GetByIdNoTrackingAsync<User>(user.Id);

            await dataAccess.UpdateAndSaveAsync(user);

            if (existing is not null)
            {
                await changeLogService.LogUpdateAsync(existing, user);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while updating user {UserId}", user.Id);
            throw;
        }
    }

    public async Task DeleteAsync(User user)
    {
        try
        {
            await changeLogService.LogDeleteAsync(user);
            await dataAccess.DeleteAsync(user);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while deleting user {UserId}", user.Id);
            throw;
        }
    }
}

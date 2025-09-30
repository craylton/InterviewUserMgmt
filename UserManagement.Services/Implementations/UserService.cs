using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;

namespace UserManagement.Services.Domain.Implementations;

public class UserService : IUserService
{
    private readonly IDataContext _dataAccess;
    private readonly IChangeLogService _changeLogService;

    public UserService(IDataContext dataAccess, IChangeLogService changeLogService)
    {
        _dataAccess = dataAccess;
        _changeLogService = changeLogService;
    }

    /// <summary>
    /// Return users by active state
    /// </summary>
    /// <param name="isActive"></param>
    /// <returns></returns>
    public IEnumerable<User> FilterByActive(bool isActive) =>
        _dataAccess.GetAll<User>().Where(u => u.IsActive == isActive);

    public IEnumerable<User> GetAll() => _dataAccess.GetAll<User>();

    public void Create(User user)
    {
        _dataAccess.Create(user);
        _changeLogService.LogAdd(user);
    }

    public User? GetById(long id) => _dataAccess.GetById<User>(id);

    public void Update(User user)
    {
        // Get existing user for comparison using AsNoTracking to avoid tracking conflicts
        var existing = _dataAccess.GetAll<User>()
            .AsNoTracking()
            .FirstOrDefault(u => u.Id == user.Id);

        _dataAccess.Update(user);

        if (existing is not null)
        {
            _changeLogService.LogUpdate(existing, user);
        }
    }

    public void Delete(User user)
    {
        _changeLogService.LogDelete(user);
        _dataAccess.Delete(user);
    }
}

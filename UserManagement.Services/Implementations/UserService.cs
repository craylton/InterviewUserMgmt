using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data;
using UserManagement.Data.Entities;
using UserManagement.Services.Interfaces;

namespace UserManagement.Services.Implementations;

public class UserService(IDataContext dataAccess, IChangeLogService changeLogService) : IUserService
{
    private readonly IDataContext _dataAccess = dataAccess;
    private readonly IChangeLogService _changeLogService = changeLogService;

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

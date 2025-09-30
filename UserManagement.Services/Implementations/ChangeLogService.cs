using System;
using System.Collections.Generic;
using System.Linq;
using UserManagement.Data;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;

namespace UserManagement.Services.Domain.Implementations;

public class ChangeLogService : IChangeLogService
{
    private readonly IDataContext _dataContext;

    public ChangeLogService(IDataContext dataContext) => _dataContext = dataContext;

    public void LogAdd(User user)
    {
        try
        {
            var logEntry = new ChangeLogEntry
            {
                UserId = user.Id,
                Timestamp = DateTime.UtcNow,
                Action = ChangeActionType.Add,
                Description = null
            };
            _dataContext.Create(logEntry);
        }
        catch
        {
            // Swallow logging exceptions to not impact user operations
        }
    }

    public void LogDelete(User user)
    {
        try
        {
            var logEntry = new ChangeLogEntry
            {
                UserId = user.Id,
                Timestamp = DateTime.UtcNow,
                Action = ChangeActionType.Delete,
                Description = null
            };
            _dataContext.Create(logEntry);
        }
        catch
        {
            // Swallow logging exceptions to not impact user operations
        }
    }

    public void LogUpdate(User before, User after)
    {
        try
        {
            var changes = GetChanges(before, after);
            foreach (var change in changes)
            {
                var logEntry = new ChangeLogEntry
                {
                    UserId = after.Id,
                    Timestamp = DateTime.UtcNow,
                    Action = ChangeActionType.Update,
                    Description = change
                };
                _dataContext.Create(logEntry);
            }
        }
        catch
        {
            // Swallow logging exceptions to not impact user operations
        }
    }

    public IEnumerable<ChangeLogEntry> GetAll(int pageNumber, int pageSize, out int totalCount)
    {
        var query = _dataContext.GetAll<ChangeLogEntry>().OrderByDescending(x => x.Timestamp);
        totalCount = query.Count();
        
        return query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public IEnumerable<ChangeLogEntry> GetByUser(long userId, int pageNumber, int pageSize, out int totalCount)
    {
        var query = _dataContext.GetAll<ChangeLogEntry>()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.Timestamp);
        
        totalCount = query.Count();
        
        return query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public ChangeLogEntry? GetById(long id) => _dataContext.GetById<ChangeLogEntry>(id);

    private static List<string> GetChanges(User before, User after)
    {
        var changes = new List<string>();

        if (before.Forename != after.Forename)
            changes.Add($"Forename changed from {before.Forename} to {after.Forename}");

        if (before.Surname != after.Surname)
            changes.Add($"Surname changed from {before.Surname} to {after.Surname}");

        if (before.Email != after.Email)
            changes.Add($"Email changed from {before.Email} to {after.Email}");

        if (before.IsActive != after.IsActive)
            changes.Add($"IsActive changed from {before.IsActive} to {after.IsActive}");

        if (before.DateOfBirth != after.DateOfBirth)
            changes.Add($"DateOfBirth changed from {before.DateOfBirth:yyyy-MM-dd} to {after.DateOfBirth:yyyy-MM-dd}");

        return changes;
    }
}

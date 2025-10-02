using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UserManagement.Data;
using UserManagement.Data.Entities;
using UserManagement.Services.Interfaces;

namespace UserManagement.Services.Implementations;

public class ChangeLogService(IDataContext dataContext, ILogger<ChangeLogService> logger) : IChangeLogService
{
    public async Task LogAddAsync(User user)
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

            await dataContext.CreateAsync(logEntry);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to log add operation for user {UserId}", user.Id);
        }
    }

    public async Task LogDeleteAsync(User user)
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

            await dataContext.CreateAsync(logEntry);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to log delete operation for user {UserId}", user.Id);
        }
    }

    public async Task LogUpdateAsync(User before, User after)
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

                await dataContext.CreateAsync(logEntry);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to log update operation for user {UserId}", after.Id);
        }
    }

    public IEnumerable<ChangeLogEntry> GetAll(int pageNumber, int pageSize, out int totalCount)
    {
        ValidatePagingParameters(pageNumber, pageSize);

        var query = dataContext.GetAll<ChangeLogEntry>().OrderByDescending(x => x.Timestamp);
        totalCount = query.Count();

        return ApplyPaging(query, pageNumber, pageSize);
    }

    public IEnumerable<ChangeLogEntry> GetByUser(long userId, int pageNumber, int pageSize, out int totalCount)
    {
        ValidatePagingParameters(pageNumber, pageSize);

        var query = dataContext.GetAll<ChangeLogEntry>()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.Timestamp);

        totalCount = query.Count();

        return ApplyPaging(query, pageNumber, pageSize);
    }

    public async Task<ChangeLogEntry?> GetByIdAsync(long id) => await dataContext.GetByIdAsync<ChangeLogEntry>(id);

    private static void ValidatePagingParameters(int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(pageNumber), pageNumber, "Page number must be greater than 0");

        if (pageSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "Page size must be greater than 0");
    }

    private static IEnumerable<TEntity> ApplyPaging<TEntity>(IQueryable<TEntity> query, int pageNumber, int pageSize)
        => query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

    private static IEnumerable<string> GetChanges(User before, User after)
    {
        if (before.Forename != after.Forename)
            yield return $"Forename changed from {before.Forename} to {after.Forename}";

        if (before.Surname != after.Surname)
            yield return $"Surname changed from {before.Surname} to {after.Surname}";

        if (before.Email != after.Email)
            yield return $"Email changed from {before.Email} to {after.Email}";

        if (before.IsActive != after.IsActive)
            yield return $"IsActive changed from {before.IsActive} to {after.IsActive}";

        if (before.DateOfBirth != after.DateOfBirth)
            yield return $"DateOfBirth changed from {before.DateOfBirth:yyyy-MM-dd} to {after.DateOfBirth:yyyy-MM-dd}";
    }
}

using System.Collections.Generic;
using UserManagement.Data.Entities;

namespace UserManagement.Services.Interfaces;

public interface IChangeLogService
{
    void LogAdd(User user);
    void LogDelete(User user);
    void LogUpdate(User before, User after);
    IEnumerable<ChangeLogEntry> GetAll(int pageNumber, int pageSize, out int totalCount);
    IEnumerable<ChangeLogEntry> GetByUser(long userId, int pageNumber, int pageSize, out int totalCount);
    ChangeLogEntry? GetById(long id);
}

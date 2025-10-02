using System.Collections.Generic;
using System.Threading.Tasks;
using UserManagement.Data.Entities;

namespace UserManagement.Services.Interfaces;

public interface IChangeLogService
{
    Task LogAddAsync(User user);
    Task LogDeleteAsync(User user);
    Task LogUpdateAsync(User before, User after);
    IEnumerable<ChangeLogEntry> GetAll(int pageNumber, int pageSize, out int totalCount);
    IEnumerable<ChangeLogEntry> GetByUser(long userId, int pageNumber, int pageSize, out int totalCount);
    Task<ChangeLogEntry?> GetByIdAsync(long id);
}

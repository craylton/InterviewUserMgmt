using System.Linq;
using System.Threading.Tasks;
using UserManagement.Data.Entities;

namespace UserManagement.Services.Interfaces;

public interface IUserService
{
    IQueryable<User> FilterByActive(bool isActive);
    IQueryable<User> GetAll();
    Task CreateAsync(User user);
    Task<User?> GetByIdAsync(long id);
    Task UpdateAsync(User user);
    Task DeleteAsync(User user);
}

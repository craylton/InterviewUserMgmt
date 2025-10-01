using System.Linq;
using UserManagement.Data.Entities;

namespace UserManagement.Services.Interfaces;

public interface IUserService
{
    IQueryable<User> FilterByActive(bool isActive);
    IQueryable<User> GetAll();
    void Create(User user);
    User? GetById(long id);
    void Update(User user);
    void Delete(User user);
}

using System.Collections.Generic;
using UserManagement.Data.Entities;

namespace UserManagement.Services.Interfaces;

public interface IUserService
{
    IEnumerable<User> FilterByActive(bool isActive);
    IEnumerable<User> GetAll();
    void Create(User user);
    User? GetById(long id);
    void Update(User user);
    void Delete(User user);
}

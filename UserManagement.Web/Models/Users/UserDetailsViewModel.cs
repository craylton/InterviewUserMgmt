using UserManagement.Web.Models.Users;

namespace UserManagement.Web.Models.Users;

public class UserDetailsViewModel
{
    public UserViewModel User { get; set; } = new();
    public Logs.LogListViewModel Logs { get; set; } = new();
}

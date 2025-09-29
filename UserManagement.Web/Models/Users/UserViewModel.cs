using System;
using System.ComponentModel.DataAnnotations;

namespace UserManagement.Web.Models.Users;

public class UserViewModel
{
    public long Id { get; set; }

    [Required(ErrorMessage = "Forename is required.")]
    [StringLength(50, ErrorMessage = "Forename cannot exceed 50 characters.")]
    public string Forename { get; set; } = string.Empty;

    [Required(ErrorMessage = "Surname is required.")]
    [StringLength(50, ErrorMessage = "Surname cannot exceed 50 characters.")]
    public string Surname { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Date Of Birth")]
    public DateTime DateOfBirth { get; set; }

    [Required(ErrorMessage = "Active status is required")]
    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }
}

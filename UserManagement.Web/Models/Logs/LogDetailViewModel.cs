using System;
using System.ComponentModel.DataAnnotations;
using UserManagement.Data.Entities;

namespace UserManagement.Web.Models.Logs;

public class LogDetailViewModel
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public ChangeActionType Action { get; set; }

    [Display(Name = "Timestamp")]
    public DateTime Timestamp { get; set; }

    public string? Description { get; set; }
    public string? ReturnTo { get; set; }
}

using System;
using System.ComponentModel.DataAnnotations;
using UserManagement.Data.Entities;

namespace UserManagement.Web.Models.Logs;

public class LogListItemViewModel
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public ChangeActionType Action { get; set; }

    [Display(Name = "Timestamp")]
    public DateTime Timestamp { get; set; }
}

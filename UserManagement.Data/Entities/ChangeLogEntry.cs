using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserManagement.Data.Entities;

public enum ChangeActionType
{
    Add = 0,
    Update = 1,
    Delete = 2
}

public class ChangeLogEntry
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    public long UserId { get; set; }
    public DateTime Timestamp { get; set; }
    public ChangeActionType Action { get; set; }
    public string? Description { get; set; }

    public User? User { get; set; }
}

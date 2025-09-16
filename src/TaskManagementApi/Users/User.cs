using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementApi.Models;

[Table("users")]
public partial class User
{
    [Column("id")]
    public string Id { get; set; } = null!;

    [Column("email")]
    public string Email { get; set; } = null!;

    [Column("password")]
    public string Password { get; set; } = null!;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}

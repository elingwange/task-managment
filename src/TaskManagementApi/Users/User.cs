using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace TaskManagementApi.Models;

public class User : IdentityUser
{
    // 自定义的额外属性
    //    public DateTime CreatedAt { get; set; }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementApi.Models;

[Table("issues")]
public partial class Issue
{
    // 使用 [Column] 特性显式映射属性名和数据库列名
    [Column("id")]
    public int Id { get; set; }

    [Column("title")]
    public string Title { get; set; } = null!;

    [Column("description")]
    public string? Description { get; set; }

    // 映射数据库中的 enum 字段
    [Column("status")]
    public IssueStatus Status { get; set; }

    [Column("priority")]
    public IssuePriority Priority { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [Column("user_id")]
    public string UserId { get; set; } = null!;
}


// C# 枚举，直接映射到 PostgreSQL 数据库中的 public.status ENUM 类型
public enum IssueStatus
{
    // 对应数据库中的 'open'
    open,
    // 对应数据库中的 'in_progress'
    in_progress,
    // 对应数据库中的 'completed'
    completed
}

// C# 枚举，直接映射到 PostgreSQL 数据库中的 public.priority ENUM 类型
public enum IssuePriority
{
    // 对应数据库中的 'low'
    low,
    // 对应数据库中的 'medium'
    medium,
    // 对应数据库中的 'high'
    high
}

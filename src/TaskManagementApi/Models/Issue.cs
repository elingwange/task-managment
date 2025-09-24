using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementApi.Models;

[Table("issues")]
public partial class Issue
{
    // Id 属性会自动映射到 id 列
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    // Status 和 Priority 属性会自动映射到 status 和 priority 列
    public IssueStatus Status { get; set; }

    public IssuePriority Priority { get; set; }

    // CreatedAt 和 UpdatedAt 属性会自动映射到 created_at 和 updated_at 列
    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // UserId 属性会自动映射到 user_id 列
    public string UserId { get; set; } = null!;
}

// C# 枚举，直接映射到 PostgreSQL 数据库中的 public.status ENUM 类型
// [backlog, todo, in_progress, done]
public enum IssueStatus
{
    backlog,
    todo,
    in_progress,
    done
}

// C# 枚举，直接映射到 PostgreSQL 数据库中的 public.priority ENUM 类型
public enum IssuePriority
{
    low,
    medium,
    high
}


public class DashboardData
{
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public double CompletionRate { get; set; }

    public List<TaskTrend> TaskCompletionTrend { get; set; }

    public List<TaskStatusDistribution> TaskStatusDistribution { get; set; }
}

public class TaskTrend
{
    public string Label { get; set; }
    public int Data { get; set; }
}

public class TaskStatusDistribution
{
    public string Status { get; set; }
    public int Count { get; set; }
}
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Data;
using TaskManagementApi.Models;

namespace IssueApi;

internal static class IssueApi
{
    public static RouteGroupBuilder MapIssues(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/issues");

        group.WithTags("Issues");

        group.MapGet("/list", (HttpContext httpContext, AppDbContext dbContext) =>
        {
            // 从 HttpContext.User 中获取当前用户的 ID
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // 根据用户 ID 查询该用户的所有任务
            var userTasks = dbContext.Issues.Where(i => i.UserId == userId).ToList();

            return Results.Ok(userTasks);
        });

        group.MapGet("/{id:int}", (HttpContext httpContext, AppDbContext dbContext, int id) =>
        {
            // 从 HttpContext.User 中获取当前用户的 ID
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // 根据用户 ID 和 Issue ID 查询单个任务
            var issue = dbContext.Issues
                                .Where(i => i.UserId == userId && i.Id == id)
                                .FirstOrDefault(); // 使用 FirstOrDefault 避免抛出异常

            // 如果未找到任务，返回 404 Not Found
            if (issue == null)
            {
                return Results.NotFound($"Issue with ID {id} not found.");
            }

            // 如果找到任务，返回 200 OK 和任务数据
            return Results.Ok(issue);
        });

        // 任务批量添加接口
        group.MapPost("/bulk-add", async (HttpContext httpContext, AppDbContext dbContext, List<Issue> issues) =>
        {
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Results.Unauthorized();
            }

            // 为每个 Issue 设置 UserId 和创建时间
            foreach (var issue in issues)
            {
                issue.UserId = userId;
                issue.CreatedAt = DateTime.UtcNow;
                issue.UpdatedAt = DateTime.UtcNow;
            }

            dbContext.Issues.AddRange(issues);
            await dbContext.SaveChangesAsync();

            return Results.Created();
        });

        // 任务修改接口
        group.MapPut("/{id}", async (HttpContext httpContext, int id, AppDbContext dbContext, Issue updatedIssue) =>
        {
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Results.Unauthorized();
            }

            // 找到数据库中要修改的任务
            var issue = await dbContext.Issues.FirstOrDefaultAsync(i => i.Id == id);

            // 检查任务是否存在
            if (issue == null)
            {
                return Results.NotFound();
            }

            // 安全检查：确保用户只能修改自己的任务
            if (issue.UserId != userId)
            {
                return Results.Forbid(); // 返回 403 Forbidden
            }

            // 更新任务属性
            issue.Title = updatedIssue.Title;
            issue.Description = updatedIssue.Description;
            issue.Status = updatedIssue.Status;
            issue.Priority = updatedIssue.Priority;
            issue.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync();

            return Results.Ok(issue);
        });

        // 任务删除接口
        group.MapDelete("/{id}", async (HttpContext httpContext, int id, AppDbContext dbContext) =>
        {
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Results.Unauthorized();
            }

            // 找到数据库中要删除的任务
            var issue = await dbContext.Issues.FirstOrDefaultAsync(i => i.Id == id);

            if (issue == null)
            {
                return Results.NotFound();
            }

            // 安全检查：确保用户只能删除自己的任务
            if (issue.UserId != userId)
            {
                return Results.Forbid(); // 返回 403 Forbidden
            }

            dbContext.Issues.Remove(issue);
            await dbContext.SaveChangesAsync();

            return Results.NoContent();
        });

        group.MapGet("/dashboard", async (HttpContext httpContext, AppDbContext dbContext) =>
                {
                    var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrEmpty(userId))
                    {
                        return Results.Unauthorized();
                    }

                    // 获取当前用户的所有任务
                    var allTasks = await dbContext.Issues
                                                  .Where(i => i.UserId == userId)
                                                  .ToListAsync();

                    if (!allTasks.Any())
                    {
                        return Results.Ok(new DashboardData
                        {
                            TotalTasks = 0,
                            CompletedTasks = 0,
                            CompletionRate = 0,
                            TaskCompletionTrend = new List<TaskTrend>(),
                            TaskStatusDistribution = new List<TaskStatusDistribution>()
                        });
                    }

                    // 1. 统计数据：总数、已完成数和完成率
                    var totalTasks = allTasks.Count;
                    // ✅ 修正：将 Completed 更改为正确的枚举值 completed
                    var completedTasks = allTasks.Count(t => t.Status == IssueStatus.done);
                    var completionRate = totalTasks > 0 ? (double)completedTasks / totalTasks : 0;

                    // 2. 任务状态分布（饼图数据）
                    var statusDistribution = allTasks.GroupBy(t => t.Status)
                                                     .Select(g => new TaskStatusDistribution
                                                     {
                                                         Status = g.Key.ToString(),
                                                         Count = g.Count()
                                                     })
                                                     .ToList();

                    // 3. 任务完成趋势（折线图数据）
                    var last7Days = Enumerable.Range(0, 7)
                                               .Select(offset => DateTime.UtcNow.Date.AddDays(-offset))
                                               .ToList();

                    var trendData = last7Days.Select(date => new TaskTrend
                    {
                        Label = date.ToString("ddd"),
                        Data = allTasks.Count(t => t.Status == IssueStatus.done && t.UpdatedAt.Date == date.Date)
                    }).ToList();

                    // 将列表反转，让最近的日期在最后
                    trendData.Reverse();

                    // 组装最终结果
                    var dashboardData = new DashboardData
                    {
                        TotalTasks = totalTasks,
                        CompletedTasks = completedTasks,
                        CompletionRate = completionRate,
                        TaskStatusDistribution = statusDistribution,
                        TaskCompletionTrend = trendData
                    };

                    return Results.Ok(dashboardData);
                });


        return group;
    }
}

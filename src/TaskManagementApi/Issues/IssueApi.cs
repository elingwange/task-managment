using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
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

            if (string.IsNullOrEmpty(userId))
            {
                return Results.Unauthorized();
            }

            // 根据用户 ID 查询该用户的所有任务
            var userTasks = dbContext.Issues.Where(i => i.UserId == userId).ToList();

            return Results.Ok(userTasks);
        });

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



        return group;
    }
}

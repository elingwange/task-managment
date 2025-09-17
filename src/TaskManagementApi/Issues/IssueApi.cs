using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Data;

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



        return group;
    }
}

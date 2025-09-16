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


        group.MapGet("/", async (AppDbContext db) =>
            await db.Issues.ToListAsync());


        return group;
    }
}

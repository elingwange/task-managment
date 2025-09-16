using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Data;

namespace TaskApi;

internal static class UsersApi
{
    public static RouteGroupBuilder MapUsers(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/users");

        group.WithTags("Users");


        group.MapGet("/", async (AppDbContext db) =>
            await db.Users.ToListAsync());


        return group;
    }
}

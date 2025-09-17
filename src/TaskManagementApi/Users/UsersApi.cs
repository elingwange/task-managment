using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Data;
using TaskManagementApi.Models;

using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace TaskApi;

internal static class UsersApi
{
    public static RouteGroupBuilder MapUsers(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/users");

        group.WithTags("Users");


        group.MapGet("/", async (AppDbContext db) => await db.Users.ToListAsync());


        group.MapPost("/register", async (RegisterRequest registerRequest, UserManager<User> userManager) =>
        {

            var user = new User
            {
                UserName = registerRequest.UserName,
                Email = registerRequest.Email,
            };

            // 将原始密码传递给 CreateAsync 方法
            var result = await userManager.CreateAsync(user, registerRequest.Password);
            if (result.Succeeded)
            {
                // 创建新用户的完整URL
                var userUri = $"/users/{user.Id}";

                // 返回201 Created，并附带成功信息
                return Results.Created(userUri, new
                {
                    message = "Registration successful",
                    userId = user.Id,
                    userName = user.UserName
                });
            }

            return Results.BadRequest(result.Errors);
        });


        group.MapPost("/login", async (LoginRequest loginRequest, UserManager<User> userManager, SignInManager<User> signInManager, IConfiguration configuration) =>
        {
            // 根据用户名查找用户
            // 1. 尝试用用户名查找用户
            var user = await userManager.FindByNameAsync(loginRequest.UserName);

            // 2. 如果用户名查找不到，并且请求中有邮箱，就尝试用邮箱查找
            if (user == null && !string.IsNullOrEmpty(loginRequest.Email))
            {
                user = await userManager.FindByEmailAsync(loginRequest.Email);
            }

            if (user == null)
            {
                // 如果用户不存在，为了安全考虑，返回一个通用的错误
                return Results.Unauthorized();
            }

            // 验证原始密码
            var result = await signInManager.CheckPasswordSignInAsync(user, loginRequest.Password, false);

            if (result.Succeeded)
            {
                // 创建 JWT 令牌
                // 1. 定义 JWT 载荷（claims），包含用户ID和用户名
                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                // 2. 从配置中获取密钥
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                // 3. 创建令牌
                var token = new JwtSecurityToken(
                    issuer: configuration["Jwt:Issuer"],
                    audience: configuration["Jwt:Audience"],
                    claims: claims,
                    // expires: DateTime.UtcNow.AddHours(1), // 令牌有效期为 1 小时
                    expires: DateTime.UtcNow.AddYears(1), // Test: 令牌有效期为 1 年
                    signingCredentials: creds
                );

                // 4. 将令牌转换为字符串并返回
                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                return Results.Ok(new { token = tokenString });
            }

            return Results.Unauthorized();
        });

        return group;
    }
}

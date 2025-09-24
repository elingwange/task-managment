using IssueApi;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using TaskApi;
using TaskManagementApi.Data;
using TaskManagementApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls(builder.Configuration["Urls"] ?? "http://localhost:5000");

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// 在配置数据库服务之前添加此行
// 启用对未映射枚举类型的支持
var dataSourceBuilder = new NpgsqlDataSourceBuilder(builder.Configuration.GetConnectionString("DefaultConnection"));
dataSourceBuilder.EnableUnmappedTypes();
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(dataSource));


// 在 services 中添加 CORS 服务
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});


// 添加 Identity 服务
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// 配置 JWT 认证
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtKey = builder.Configuration["Jwt:Key"];
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});




var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 在应用中使用 CORS 中间件
app.UseCors("AllowAllOrigins");

// 其他中间件配置
app.UseHttpsRedirection();

// 启用认证和授权中间件
app.UseAuthentication();
app.UseAuthorization();


// 保护任务管理端点
app.MapUsers();
//app.MapUsers().RequireAuthorization();
app.MapIssues();
//app.MapIssues().RequireAuthorization();

app.Run();



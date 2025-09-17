
// Data/AppDbContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Models;


namespace TaskManagementApi.Data;

public class AppDbContext : IdentityDbContext<User>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // 这里不再需要 DbSet<User>，因为 IdentityDbContext 已经包含了它
    public DbSet<Issue> Issues { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 这是一个更健壮的实现，可以处理 Identity 表名和列名
        // 它会把 'AspNetUsers' -> 'aspnetusers'，'NormalizedUserName' -> 'normalized_user_name'
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            // 转换表名
            entity.SetTableName(entity.GetTableName()!.ToLower());

            // 转换列名
            foreach (var property in entity.GetProperties())
            {
                if (property.GetColumnName() == null) continue;

                // 这里使用一个简单的正则来转换为蛇形命名
                var name = property.GetColumnName();
                var snakeCaseName = System.Text.RegularExpressions.Regex
                                    .Replace(name, "([a-z0-9])([A-Z])", "$1_$2").ToLower();
                property.SetColumnName(snakeCaseName);
            }
        }

        // 为 User 模型添加配置
        modelBuilder.Entity<User>(entity =>
        {
            // 为 CreatedAt 属性添加一个默认值，该值将在数据库端生成
            // 使用数据库函数 now() 来获取当前时间
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
        });
    }

}
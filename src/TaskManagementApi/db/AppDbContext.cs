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

    public DbSet<Issue> Issues { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 明确告诉 EF Core 如何处理 ENUM
        modelBuilder.HasPostgresEnum<IssueStatus>();
        modelBuilder.HasPostgresEnum<IssuePriority>();

        modelBuilder.Entity<Issue>(entity =>
        {
            // 为 Status 字段添加显式的类型转换，确保它能正确地从数据库的 ENUM 类型映射
            entity.Property(e => e.Status)
                  .HasConversion<string>();

            // 为 Priority 字段添加显式的类型转换
            entity.Property(e => e.Priority)
                  .HasConversion<string>();
        });

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

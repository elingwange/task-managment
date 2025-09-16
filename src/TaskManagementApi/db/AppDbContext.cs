using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // DbSet 类型改为 Issues
        public DbSet<Issues> Issues { get; set; }
    }

    [Table("issues")]
    public class Issues
    {
        public int id { get; set; }
        public string title { get; set; }
    }
}
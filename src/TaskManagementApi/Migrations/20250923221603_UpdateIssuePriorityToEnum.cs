using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagementApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIssuePriorityToEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. 创建一个新的 ENUM 类型，用于 priority
            // migrationBuilder.Sql(@"
            //     CREATE TYPE public.priority AS ENUM (
            //         'low',
            //         'medium',
            //         'high'
            //     );
            // ");

            // 2. 添加一个临时字段，类型为新的 ENUM
            migrationBuilder.AddColumn<string>(
                name: "priority_new",
                table: "issues",
                type: "text",
                nullable: true);

            // 3. 将旧的整数数据迁移到新字段
            migrationBuilder.Sql(@"
                UPDATE issues SET priority_new =
                (CASE priority
                    WHEN 0 THEN 'low'
                    WHEN 1 THEN 'medium'
                    WHEN 2 THEN 'high'
                    ELSE 'low'
                END);
            ");

            // 4. 删除旧的整数 priority 字段
            migrationBuilder.DropColumn(
                name: "priority",
                table: "issues");

            // 5. 重命名新字段为 priority
            migrationBuilder.RenameColumn(
                name: "priority_new",
                table: "issues",
                newName: "priority");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}

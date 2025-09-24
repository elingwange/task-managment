using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagementApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIssueStatusToEnum : Migration
    {
        /// <inheritdoc />
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. 创建一个新的 ENUM 类型，用于 status
            // migrationBuilder.Sql(@"
            //     CREATE TYPE public.status AS ENUM (
            //         'backlog',
            //         'todo',
            //         'in_progress',
            //         'done'
            //     );
            // ");

            // 2. 添加一个临时字段，类型为新的 ENUM
            migrationBuilder.AddColumn<string>(
                name: "status_new",
                table: "issues",
                type: "text",
                nullable: true);

            // 3. 将旧的整数数据迁移到新字段
            migrationBuilder.Sql(@"
                UPDATE issues SET status_new =
                (CASE CAST(status AS TEXT)
                    WHEN 'backlog' THEN 'backlog'
                    WHEN 'todo' THEN 'todo'
                    WHEN 'in_progress' THEN 'in_progress'
                    WHEN 'done' THEN 'done'
                    ELSE 'backlog'
                END)::status;
            ");

            // 4. 删除旧的整数 status 字段
            migrationBuilder.DropColumn(
                name: "status",
                table: "issues");

            // 5. 重命名新字段为 status
            migrationBuilder.RenameColumn(
                name: "status_new",
                table: "issues",
                newName: "status");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}

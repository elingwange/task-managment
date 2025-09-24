using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagementApi.Migrations
{
    /// <inheritdoc />
    public partial class ModelChangesForEnums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:issue_priority", "low,medium,high")
                .Annotation("Npgsql:Enum:issue_status", "backlog,todo,in_progress,done")
                .OldAnnotation("Npgsql:Enum:issue_status", "backlog,todo,in_progress,done");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:issue_status", "backlog,todo,in_progress,done")
                .OldAnnotation("Npgsql:Enum:issue_priority", "low,medium,high")
                .OldAnnotation("Npgsql:Enum:issue_status", "backlog,todo,in_progress,done");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TODOlistsystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPriorityToNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "Notes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Notes");
        }
    }
}

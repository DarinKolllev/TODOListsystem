using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TODOlistsystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserNoteForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notes_DueDate",
                table: "Notes");

            migrationBuilder.DropIndex(
                name: "IX_Notes_IsDeleted",
                table: "Notes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Notes_DueDate",
                table: "Notes",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_IsDeleted",
                table: "Notes",
                column: "IsDeleted");
        }
    }
}

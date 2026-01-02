using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mvc_CRUD.Migrations
{
    /// <inheritdoc />
    public partial class RemovedAndEditedColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CollegeProgress",
                table: "Profile");

            migrationBuilder.DropColumn(
                name: "JobProgress",
                table: "Profile");

            migrationBuilder.RenameColumn(
                name: "SchoolProgress",
                table: "Profile",
                newName: "JobPeriod");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "JobPeriod",
                table: "Profile",
                newName: "SchoolProgress");

            migrationBuilder.AddColumn<string>(
                name: "CollegeProgress",
                table: "Profile",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JobProgress",
                table: "Profile",
                type: "text",
                nullable: true);
        }
    }
}

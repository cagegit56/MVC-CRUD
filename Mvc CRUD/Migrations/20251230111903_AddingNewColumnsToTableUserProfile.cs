using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mvc_CRUD.Migrations
{
    /// <inheritdoc />
    public partial class AddingNewColumnsToTableUserProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CollegePeriod",
                table: "Profile",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CollegeProgress",
                table: "Profile",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Industry",
                table: "Profile",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JobProgress",
                table: "Profile",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JobTitle",
                table: "Profile",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Profile",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SchoolPeriod",
                table: "Profile",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SchoolProgress",
                table: "Profile",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserCoverPicUrl",
                table: "Profile",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "Profile",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CollegePeriod",
                table: "Profile");

            migrationBuilder.DropColumn(
                name: "CollegeProgress",
                table: "Profile");

            migrationBuilder.DropColumn(
                name: "Industry",
                table: "Profile");

            migrationBuilder.DropColumn(
                name: "JobProgress",
                table: "Profile");

            migrationBuilder.DropColumn(
                name: "JobTitle",
                table: "Profile");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Profile");

            migrationBuilder.DropColumn(
                name: "SchoolPeriod",
                table: "Profile");

            migrationBuilder.DropColumn(
                name: "SchoolProgress",
                table: "Profile");

            migrationBuilder.DropColumn(
                name: "UserCoverPicUrl",
                table: "Profile");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "Profile");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mvc_CRUD.Migrations
{
    /// <inheritdoc />
    public partial class TwoNewColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "FriendRequests",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProfilePicUrl",
                table: "FriendRequests",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastName",
                table: "FriendRequests");

            migrationBuilder.DropColumn(
                name: "ProfilePicUrl",
                table: "FriendRequests");
        }
    }
}

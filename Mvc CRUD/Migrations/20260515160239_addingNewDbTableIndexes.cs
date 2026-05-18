using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mvc_CRUD.Migrations
{
    /// <inheritdoc />
    public partial class addingNewDbTableIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Profile_UserId",
                table: "Profile",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Friends_UserId",
                table: "Friends",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Friends_UserId_FriendName",
                table: "Friends",
                columns: new[] { "UserId", "FriendName" });

            migrationBuilder.CreateIndex(
                name: "IX_Chats_ToUser_UserName",
                table: "Chats",
                columns: new[] { "ToUser", "UserName" });

            migrationBuilder.CreateIndex(
                name: "IX_Chats_UserName_ToUser",
                table: "Chats",
                columns: new[] { "UserName", "ToUser" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Profile_UserId",
                table: "Profile");

            migrationBuilder.DropIndex(
                name: "IX_Friends_UserId",
                table: "Friends");

            migrationBuilder.DropIndex(
                name: "IX_Friends_UserId_FriendName",
                table: "Friends");

            migrationBuilder.DropIndex(
                name: "IX_Chats_ToUser_UserName",
                table: "Chats");

            migrationBuilder.DropIndex(
                name: "IX_Chats_UserName_ToUser",
                table: "Chats");
        }
    }
}

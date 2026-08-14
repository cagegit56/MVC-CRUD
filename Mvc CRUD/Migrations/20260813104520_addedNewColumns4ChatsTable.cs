using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mvc_CRUD.Migrations
{
    /// <inheritdoc />
    public partial class addedNewColumns4ChatsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chats_ToUser_UserName",
                table: "Chats");

            migrationBuilder.DropIndex(
                name: "IX_Chats_UserName_ToUser",
                table: "Chats");

            migrationBuilder.RenameColumn(
                name: "ToUser",
                table: "Chats",
                newName: "UserId");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Chats",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Chats",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Chats",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ToLastName",
                table: "Chats",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToUserId",
                table: "Chats",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ToUserName",
                table: "Chats",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Chats_ToUserName_UserName",
                table: "Chats",
                columns: new[] { "ToUserName", "UserName" });

            migrationBuilder.CreateIndex(
                name: "IX_Chats_UserName_ToUserName",
                table: "Chats",
                columns: new[] { "UserName", "ToUserName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chats_ToUserName_UserName",
                table: "Chats");

            migrationBuilder.DropIndex(
                name: "IX_Chats_UserName_ToUserName",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "ToLastName",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "ToUserId",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "ToUserName",
                table: "Chats");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Chats",
                newName: "ToUser");

            migrationBuilder.CreateIndex(
                name: "IX_Chats_ToUser_UserName",
                table: "Chats",
                columns: new[] { "ToUser", "UserName" });

            migrationBuilder.CreateIndex(
                name: "IX_Chats_UserName_ToUser",
                table: "Chats",
                columns: new[] { "UserName", "ToUser" });
        }
    }
}

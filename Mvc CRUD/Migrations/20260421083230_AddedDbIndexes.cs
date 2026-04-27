using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mvc_CRUD.Migrations
{
    /// <inheritdoc />
    public partial class AddedDbIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comment_Post_PostId",
                table: "Comment");

            migrationBuilder.DropIndex(
                name: "IX_Comment_PostId",
                table: "Comment");

            migrationBuilder.DropColumn(
                name: "Isliked",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "likes",
                table: "Post");

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "ReplyComments",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Profile",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Post_PostScope_UserName",
                table: "Post",
                columns: new[] { "PostScope", "UserName" });

            migrationBuilder.CreateIndex(
                name: "IX_Like_PostId_IsDeleted",
                table: "Like",
                columns: new[] { "PostId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Friends_FriendName_UserName",
                table: "Friends",
                columns: new[] { "FriendName", "UserName" });

            migrationBuilder.CreateIndex(
                name: "IX_Friends_UserName_FriendName",
                table: "Friends",
                columns: new[] { "UserName", "FriendName" });

            migrationBuilder.AddForeignKey(
                name: "FK_Like_Post_PostId",
                table: "Like",
                column: "PostId",
                principalTable: "Post",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Like_Post_PostId",
                table: "Like");

            migrationBuilder.DropIndex(
                name: "IX_Post_PostScope_UserName",
                table: "Post");

            migrationBuilder.DropIndex(
                name: "IX_Like_PostId_IsDeleted",
                table: "Like");

            migrationBuilder.DropIndex(
                name: "IX_Friends_FriendName_UserName",
                table: "Friends");

            migrationBuilder.DropIndex(
                name: "IX_Friends_UserName_FriendName",
                table: "Friends");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Profile");

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "ReplyComments",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<bool>(
                name: "Isliked",
                table: "Post",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "likes",
                table: "Post",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comment_PostId",
                table: "Comment",
                column: "PostId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_Post_PostId",
                table: "Comment",
                column: "PostId",
                principalTable: "Post",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mvc_CRUD.Migrations
{
    /// <inheritdoc />
    public partial class NewColumnUserImageUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comment_ReplyComments_ReplyId",
                table: "Comment");

            migrationBuilder.DropIndex(
                name: "IX_Comment_ReplyId",
                table: "Comment");

            migrationBuilder.DropColumn(
                name: "ReplyId",
                table: "Comment");

            migrationBuilder.AddColumn<string>(
                name: "UserImageUrl",
                table: "ReplyComments",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReplyComments_CommentId",
                table: "ReplyComments",
                column: "CommentId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReplyComments_Comment_CommentId",
                table: "ReplyComments",
                column: "CommentId",
                principalTable: "Comment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReplyComments_Comment_CommentId",
                table: "ReplyComments");

            migrationBuilder.DropIndex(
                name: "IX_ReplyComments_CommentId",
                table: "ReplyComments");

            migrationBuilder.DropColumn(
                name: "UserImageUrl",
                table: "ReplyComments");

            migrationBuilder.AddColumn<int>(
                name: "ReplyId",
                table: "Comment",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comment_ReplyId",
                table: "Comment",
                column: "ReplyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_ReplyComments_ReplyId",
                table: "Comment",
                column: "ReplyId",
                principalTable: "ReplyComments",
                principalColumn: "Id");
        }
    }
}

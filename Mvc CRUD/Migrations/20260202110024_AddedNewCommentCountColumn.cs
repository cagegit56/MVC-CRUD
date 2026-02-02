using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mvc_CRUD.Migrations
{
    /// <inheritdoc />
    public partial class AddedNewCommentCountColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommentImageContentUrl",
                table: "ReplyComments");

            migrationBuilder.DropColumn(
                name: "CommentLastName",
                table: "ReplyComments");

            migrationBuilder.DropColumn(
                name: "CommentMessage",
                table: "ReplyComments");

            migrationBuilder.DropColumn(
                name: "CommentUserName",
                table: "ReplyComments");

            migrationBuilder.AddColumn<int>(
                name: "TotalComments",
                table: "Post",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalComments",
                table: "Post");

            migrationBuilder.AddColumn<string>(
                name: "CommentImageContentUrl",
                table: "ReplyComments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CommentLastName",
                table: "ReplyComments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CommentMessage",
                table: "ReplyComments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CommentUserName",
                table: "ReplyComments",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}

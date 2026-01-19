using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Mvc_CRUD.Migrations
{
    /// <inheritdoc />
    public partial class addNewCommentsReplyTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReplyId",
                table: "Comment",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ReplyComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: true),
                    ImageContentUrl = table.Column<string>(type: "text", nullable: true),
                    CommentId = table.Column<int>(type: "integer", nullable: false),
                    CommentUserName = table.Column<string>(type: "text", nullable: false),
                    CommentLastName = table.Column<string>(type: "text", nullable: false),
                    CommentMessage = table.Column<string>(type: "text", nullable: true),
                    CommentImageContentUrl = table.Column<string>(type: "text", nullable: true),
                    SentOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReplyComments", x => x.Id);
                });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comment_ReplyComments_ReplyId",
                table: "Comment");

            migrationBuilder.DropTable(
                name: "ReplyComments");

            migrationBuilder.DropIndex(
                name: "IX_Comment_ReplyId",
                table: "Comment");

            migrationBuilder.DropColumn(
                name: "ReplyId",
                table: "Comment");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mvc_CRUD.Migrations
{
    /// <inheritdoc />
    public partial class ChangedDataTypeForColumnIsLiked : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE ""Post""
                ALTER COLUMN ""Isliked"" TYPE boolean
                USING CASE
                    WHEN ""Isliked"" = 1 THEN true
                    ELSE false
                END;
            ");


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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comment_Post_PostId",
                table: "Comment");

            migrationBuilder.DropIndex(
                name: "IX_Comment_PostId",
                table: "Comment");

            migrationBuilder.Sql(@"
                ALTER TABLE ""Post""
                ALTER COLUMN ""Isliked"" TYPE boolean
                USING CASE
                    WHEN ""Isliked"" = 1 THEN true
                    ELSE false
                END;
            ");

        }
    }
}

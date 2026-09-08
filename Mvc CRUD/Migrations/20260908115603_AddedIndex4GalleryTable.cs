using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mvc_CRUD.Migrations
{
    /// <inheritdoc />
    public partial class AddedIndex4GalleryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Gallery_UserId",
                table: "Gallery",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Gallery_UserId",
                table: "Gallery");
        }
    }
}

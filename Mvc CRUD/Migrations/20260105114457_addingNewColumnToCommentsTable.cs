using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mvc_CRUD.Migrations
{
    /// <inheritdoc />
    public partial class addingNewColumnToCommentsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Comment",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Comment");
        }
    }
}

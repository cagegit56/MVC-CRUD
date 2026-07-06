using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mvc_CRUD.Migrations
{
    /// <inheritdoc />
    public partial class addingNewDbIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Friends_UserId_FriendName",
                table: "Friends");

            migrationBuilder.CreateIndex(
                name: "IX_Friends_UserId_FriendId",
                table: "Friends",
                columns: new[] { "UserId", "FriendId" });

            migrationBuilder.CreateIndex(
                name: "IX_FriendRequests_UserId_Status_isDeleted",
                table: "FriendRequests",
                columns: new[] { "UserId", "Status", "isDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_FriendRequests_UserId_ToUserId_Status_isDeleted",
                table: "FriendRequests",
                columns: new[] { "UserId", "ToUserId", "Status", "isDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Comment_PostId",
                table: "Comment",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_BlockedUser_UserId_BlockUserId",
                table: "BlockedUser",
                columns: new[] { "UserId", "BlockUserId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Friends_UserId_FriendId",
                table: "Friends");

            migrationBuilder.DropIndex(
                name: "IX_FriendRequests_UserId_Status_isDeleted",
                table: "FriendRequests");

            migrationBuilder.DropIndex(
                name: "IX_FriendRequests_UserId_ToUserId_Status_isDeleted",
                table: "FriendRequests");

            migrationBuilder.DropIndex(
                name: "IX_Comment_PostId",
                table: "Comment");

            migrationBuilder.DropIndex(
                name: "IX_BlockedUser_UserId_BlockUserId",
                table: "BlockedUser");

            migrationBuilder.CreateIndex(
                name: "IX_Friends_UserId_FriendName",
                table: "Friends",
                columns: new[] { "UserId", "FriendName" });
        }
    }
}

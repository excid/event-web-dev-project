using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace event_web_dev_project.Migrations
{
    /// <inheritdoc />
    public partial class Bugged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ActivityPosts_OwnerId",
                table: "ActivityPosts",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityPosts_AspNetUsers_OwnerId",
                table: "ActivityPosts",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivityPosts_AspNetUsers_OwnerId",
                table: "ActivityPosts");

            migrationBuilder.DropIndex(
                name: "IX_ActivityPosts_OwnerId",
                table: "ActivityPosts");
        }
    }
}

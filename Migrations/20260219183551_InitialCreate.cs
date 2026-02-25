using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace event_web_dev_project.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityPosts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MaxMembers = table.Column<int>(type: "int", nullable: false),
                    CurrentMembers = table.Column<int>(type: "int", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApplicationMode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PostedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PostedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityPosts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PostApplications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostId = table.Column<int>(type: "int", nullable: false),
                    ApplicantName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AppliedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostApplications_ActivityPosts_PostId",
                        column: x => x.PostId,
                        principalTable: "ActivityPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ActivityPosts",
                columns: new[] { "Id", "ApplicationMode", "Category", "CurrentMembers", "Description", "ExpiresAt", "Location", "MaxMembers", "PostedAt", "PostedBy", "Status", "Title" },
                values: new object[] { 1, "Overflow allowed - Owner selects", "Sports", 2, "We need 3 more players for a friendly football match this Sunday at Central Park. All skill levels welcome! We play 7v7 format.", new DateTime(2026, 2, 15, 12, 0, 0, 0, DateTimeKind.Unspecified), "Central Park, Field 3", 3, new DateTime(2026, 2, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "Alex Johnson", "Open", "Looking for Football Teammates - Sunday Match" });

            migrationBuilder.InsertData(
                table: "PostApplications",
                columns: new[] { "Id", "ApplicantName", "AppliedAt", "Message", "PostId", "Status" },
                values: new object[,]
                {
                    { 1, "Sarah Chen", new DateTime(2026, 2, 11, 14, 30, 0, 0, DateTimeKind.Unspecified), "I'd love to join! I play midfielder and have experience.", 1, "Accepted" },
                    { 2, "Mike Rodriguez", new DateTime(2026, 2, 11, 15, 0, 0, 0, DateTimeKind.Unspecified), "Count me in! I'm available on Sunday.", 1, "Accepted" },
                    { 3, "Emily Park", new DateTime(2026, 2, 11, 16, 0, 0, 0, DateTimeKind.Unspecified), "I'm interested! Can I bring a friend?", 1, "Pending" },
                    { 4, "Jessica Liu", new DateTime(2026, 2, 11, 17, 0, 0, 0, DateTimeKind.Unspecified), "Would love to join but I'm a beginner. Is that okay?", 1, "Rejected" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PostApplications_PostId",
                table: "PostApplications",
                column: "PostId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostApplications");

            migrationBuilder.DropTable(
                name: "ActivityPosts");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace event_web_dev_project.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicantIdToApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicantId",
                table: "PostApplications",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "PostApplications",
                keyColumn: "Id",
                keyValue: 1,
                column: "ApplicantId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PostApplications",
                keyColumn: "Id",
                keyValue: 2,
                column: "ApplicantId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PostApplications",
                keyColumn: "Id",
                keyValue: 3,
                column: "ApplicantId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PostApplications",
                keyColumn: "Id",
                keyValue: 4,
                column: "ApplicantId",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicantId",
                table: "PostApplications");
        }
    }
}

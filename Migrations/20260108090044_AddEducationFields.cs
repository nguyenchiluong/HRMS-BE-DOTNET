using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeApi.Migrations
{
    /// <inheritdoc />
    public partial class AddEducationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "end_year",
                schema: "dotnet",
                table: "education",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "institution",
                schema: "dotnet",
                table: "education",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "start_year",
                schema: "dotnet",
                table: "education",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "end_year",
                schema: "dotnet",
                table: "education");

            migrationBuilder.DropColumn(
                name: "institution",
                schema: "dotnet",
                table: "education");

            migrationBuilder.DropColumn(
                name: "start_year",
                schema: "dotnet",
                table: "education");
        }
    }
}

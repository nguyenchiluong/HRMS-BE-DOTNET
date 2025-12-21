using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeApi.Migrations
{
    /// <inheritdoc />
    public partial class MoveDepartmentPositionToDotnetSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "position",
                schema: "public",
                newName: "position",
                newSchema: "dotnet");

            migrationBuilder.RenameTable(
                name: "department",
                schema: "public",
                newName: "department",
                newSchema: "dotnet");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.RenameTable(
                name: "position",
                schema: "dotnet",
                newName: "position",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "department",
                schema: "dotnet",
                newName: "department",
                newSchema: "public");
        }
    }
}

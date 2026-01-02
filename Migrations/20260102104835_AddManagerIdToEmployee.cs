using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeApi.Migrations
{
    /// <inheritdoc />
    public partial class AddManagerIdToEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "kebab_case",
                schema: "dotnet",
                table: "request_type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "kebab_case",
                schema: "dotnet",
                table: "request_type",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}

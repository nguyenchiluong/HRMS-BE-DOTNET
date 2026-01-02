using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeApi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEmergencyContactField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "emergency_contact",
                schema: "dotnet",
                table: "request");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "emergency_contact",
                schema: "dotnet",
                table: "request",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}

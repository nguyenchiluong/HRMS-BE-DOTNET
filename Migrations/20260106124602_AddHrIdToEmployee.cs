using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeApi.Migrations
{
    /// <inheritdoc />
    public partial class AddHrIdToEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "hr_id",
                schema: "dotnet",
                table: "employee",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_employee_hr_id",
                schema: "dotnet",
                table: "employee",
                column: "hr_id");

            migrationBuilder.AddForeignKey(
                name: "FK_employee_employee_hr_id",
                schema: "dotnet",
                table: "employee",
                column: "hr_id",
                principalSchema: "dotnet",
                principalTable: "employee",
                principalColumn: "emp_id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_employee_employee_hr_id",
                schema: "dotnet",
                table: "employee");

            migrationBuilder.DropIndex(
                name: "IX_employee_hr_id",
                schema: "dotnet",
                table: "employee");

            migrationBuilder.DropColumn(
                name: "hr_id",
                schema: "dotnet",
                table: "employee");
        }
    }
}

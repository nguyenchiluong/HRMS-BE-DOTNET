using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeApi.Migrations
{
    /// <inheritdoc />
    public partial class AddJobLevelEmploymentTypeTimeType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "employee_type",
                schema: "dotnet",
                table: "employee");

            migrationBuilder.DropColumn(
                name: "job_level",
                schema: "dotnet",
                table: "employee");

            migrationBuilder.DropColumn(
                name: "time_type",
                schema: "dotnet",
                table: "employee");

            migrationBuilder.AddColumn<long>(
                name: "employment_type_id",
                schema: "dotnet",
                table: "employee",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "job_level_id",
                schema: "dotnet",
                table: "employee",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "time_type_id",
                schema: "dotnet",
                table: "employee",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "employment_type",
                schema: "dotnet",
                columns: table => new
                {
                    employment_type_id = table.Column<long>(type: "bigint", nullable: false),
                    employment_type_name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employment_type", x => x.employment_type_id);
                });

            migrationBuilder.CreateTable(
                name: "job_level",
                schema: "dotnet",
                columns: table => new
                {
                    job_level_id = table.Column<long>(type: "bigint", nullable: false),
                    job_level_name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job_level", x => x.job_level_id);
                });

            migrationBuilder.CreateTable(
                name: "time_type",
                schema: "dotnet",
                columns: table => new
                {
                    time_type_id = table.Column<long>(type: "bigint", nullable: false),
                    time_type_name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_time_type", x => x.time_type_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_employee_employment_type_id",
                schema: "dotnet",
                table: "employee",
                column: "employment_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_job_level_id",
                schema: "dotnet",
                table: "employee",
                column: "job_level_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_time_type_id",
                schema: "dotnet",
                table: "employee",
                column: "time_type_id");

            migrationBuilder.AddForeignKey(
                name: "FK_employee_employment_type_employment_type_id",
                schema: "dotnet",
                table: "employee",
                column: "employment_type_id",
                principalSchema: "dotnet",
                principalTable: "employment_type",
                principalColumn: "employment_type_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_employee_job_level_job_level_id",
                schema: "dotnet",
                table: "employee",
                column: "job_level_id",
                principalSchema: "dotnet",
                principalTable: "job_level",
                principalColumn: "job_level_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_employee_time_type_time_type_id",
                schema: "dotnet",
                table: "employee",
                column: "time_type_id",
                principalSchema: "dotnet",
                principalTable: "time_type",
                principalColumn: "time_type_id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_employee_employment_type_employment_type_id",
                schema: "dotnet",
                table: "employee");

            migrationBuilder.DropForeignKey(
                name: "FK_employee_job_level_job_level_id",
                schema: "dotnet",
                table: "employee");

            migrationBuilder.DropForeignKey(
                name: "FK_employee_time_type_time_type_id",
                schema: "dotnet",
                table: "employee");

            migrationBuilder.DropTable(
                name: "employment_type",
                schema: "dotnet");

            migrationBuilder.DropTable(
                name: "job_level",
                schema: "dotnet");

            migrationBuilder.DropTable(
                name: "time_type",
                schema: "dotnet");

            migrationBuilder.DropIndex(
                name: "IX_employee_employment_type_id",
                schema: "dotnet",
                table: "employee");

            migrationBuilder.DropIndex(
                name: "IX_employee_job_level_id",
                schema: "dotnet",
                table: "employee");

            migrationBuilder.DropIndex(
                name: "IX_employee_time_type_id",
                schema: "dotnet",
                table: "employee");

            migrationBuilder.DropColumn(
                name: "employment_type_id",
                schema: "dotnet",
                table: "employee");

            migrationBuilder.DropColumn(
                name: "job_level_id",
                schema: "dotnet",
                table: "employee");

            migrationBuilder.DropColumn(
                name: "time_type_id",
                schema: "dotnet",
                table: "employee");

            migrationBuilder.AddColumn<string>(
                name: "employee_type",
                schema: "dotnet",
                table: "employee",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "job_level",
                schema: "dotnet",
                table: "employee",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "time_type",
                schema: "dotnet",
                table: "employee",
                type: "text",
                nullable: true);
        }
    }
}

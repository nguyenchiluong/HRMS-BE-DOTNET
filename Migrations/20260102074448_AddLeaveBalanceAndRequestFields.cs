using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EmployeeApi.Migrations
{
    /// <inheritdoc />
    public partial class AddLeaveBalanceAndRequestFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "emergency_contact",
                schema: "dotnet",
                table: "request",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "leave_balance",
                schema: "dotnet",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    employee_id = table.Column<long>(type: "bigint", nullable: false),
                    balance_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    total = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    used = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_leave_balance", x => x.id);
                    table.ForeignKey(
                        name: "FK_leave_balance_employee_employee_id",
                        column: x => x.employee_id,
                        principalSchema: "dotnet",
                        principalTable: "employee",
                        principalColumn: "emp_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_leave_balance_employee_id",
                schema: "dotnet",
                table: "leave_balance",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_leave_balance_employee_id_balance_type_year",
                schema: "dotnet",
                table: "leave_balance",
                columns: new[] { "employee_id", "balance_type", "year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "leave_balance",
                schema: "dotnet");

            migrationBuilder.DropColumn(
                name: "emergency_contact",
                schema: "dotnet",
                table: "request");
        }
    }
}

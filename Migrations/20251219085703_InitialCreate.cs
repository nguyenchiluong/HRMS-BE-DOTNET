using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EmployeeApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dotnet");

            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "position",
                schema: "public",
                columns: table => new
                {
                    position_id = table.Column<long>(type: "bigint", nullable: false),
                    position_name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    salary = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_position", x => x.position_id);
                });

            migrationBuilder.CreateTable(
                name: "attendance_record",
                schema: "dotnet",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeId = table.Column<long>(type: "bigint", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CheckInTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CheckOutTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TotalHours = table.Column<double>(type: "double precision", nullable: true),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attendance_record", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "bank_account",
                schema: "dotnet",
                columns: table => new
                {
                    account_number = table.Column<string>(type: "text", nullable: false),
                    bank_name = table.Column<string>(type: "text", nullable: false),
                    account_name = table.Column<string>(type: "text", nullable: true),
                    emp_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bank_account", x => new { x.account_number, x.bank_name });
                });

            migrationBuilder.CreateTable(
                name: "department",
                schema: "public",
                columns: table => new
                {
                    dept_id = table.Column<long>(type: "bigint", nullable: false),
                    dept_name = table.Column<string>(type: "text", nullable: false),
                    location = table.Column<string>(type: "text", nullable: true),
                    manager_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_department", x => x.dept_id);
                });

            migrationBuilder.CreateTable(
                name: "employee",
                schema: "dotnet",
                columns: table => new
                {
                    emp_id = table.Column<long>(type: "bigint", nullable: false),
                    full_name = table.Column<string>(type: "text", nullable: false),
                    first_name = table.Column<string>(type: "text", nullable: true),
                    last_name = table.Column<string>(type: "text", nullable: true),
                    preferred_name = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: false),
                    personal_email = table.Column<string>(type: "text", nullable: true),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    phone_number_2 = table.Column<string>(type: "text", nullable: true),
                    sex = table.Column<string>(type: "text", nullable: true),
                    date_of_birth = table.Column<DateOnly>(type: "date", nullable: true),
                    marital_status = table.Column<string>(type: "text", nullable: true),
                    pronoun = table.Column<string>(type: "text", nullable: true),
                    permanent_address = table.Column<string>(type: "text", nullable: true),
                    current_address = table.Column<string>(type: "text", nullable: true),
                    national_id_country = table.Column<string>(type: "text", nullable: true),
                    national_id_number = table.Column<string>(type: "text", nullable: true),
                    national_id_issued_date = table.Column<DateOnly>(type: "date", nullable: true),
                    national_id_expiration_date = table.Column<DateOnly>(type: "date", nullable: true),
                    national_id_issued_by = table.Column<string>(type: "text", nullable: true),
                    social_insurance_number = table.Column<string>(type: "text", nullable: true),
                    tax_id = table.Column<string>(type: "text", nullable: true),
                    start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    job_level = table.Column<string>(type: "text", nullable: true),
                    employee_type = table.Column<string>(type: "text", nullable: true),
                    time_type = table.Column<string>(type: "text", nullable: true),
                    dept_id = table.Column<long>(type: "bigint", nullable: true),
                    position_id = table.Column<long>(type: "bigint", nullable: true),
                    manager_id = table.Column<long>(type: "bigint", nullable: true),
                    status = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee", x => x.emp_id);
                    table.ForeignKey(
                        name: "FK_employee_department_dept_id",
                        column: x => x.dept_id,
                        principalSchema: "public",
                        principalTable: "department",
                        principalColumn: "dept_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_employee_employee_manager_id",
                        column: x => x.manager_id,
                        principalSchema: "dotnet",
                        principalTable: "employee",
                        principalColumn: "emp_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_employee_position_position_id",
                        column: x => x.position_id,
                        principalSchema: "public",
                        principalTable: "position",
                        principalColumn: "position_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "education",
                schema: "dotnet",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    emp_id = table.Column<long>(type: "bigint", nullable: false),
                    degree = table.Column<string>(type: "text", nullable: false),
                    field_of_study = table.Column<string>(type: "text", nullable: true),
                    gpa = table.Column<double>(type: "double precision", nullable: true),
                    country = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_education", x => x.id);
                    table.ForeignKey(
                        name: "FK_education_employee_emp_id",
                        column: x => x.emp_id,
                        principalSchema: "dotnet",
                        principalTable: "employee",
                        principalColumn: "emp_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "request",
                schema: "dotnet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RequestType = table.Column<string>(type: "text", nullable: false),
                    RequesterEmployeeId = table.Column<long>(type: "bigint", nullable: false),
                    ApproverEmployeeId = table.Column<long>(type: "bigint", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EffectiveTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    Payload = table.Column<string>(type: "jsonb", nullable: true),
                    ApprovalComment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_request", x => x.Id);
                    table.ForeignKey(
                        name: "FK_request_employee_ApproverEmployeeId",
                        column: x => x.ApproverEmployeeId,
                        principalSchema: "dotnet",
                        principalTable: "employee",
                        principalColumn: "emp_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_request_employee_RequesterEmployeeId",
                        column: x => x.RequesterEmployeeId,
                        principalSchema: "dotnet",
                        principalTable: "employee",
                        principalColumn: "emp_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_attendance_record_Date",
                schema: "dotnet",
                table: "attendance_record",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_record_EmployeeId",
                schema: "dotnet",
                table: "attendance_record",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_bank_account_account_number",
                schema: "dotnet",
                table: "bank_account",
                column: "account_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_bank_account_emp_id",
                schema: "dotnet",
                table: "bank_account",
                column: "emp_id");

            migrationBuilder.CreateIndex(
                name: "IX_department_manager_id",
                schema: "public",
                table: "department",
                column: "manager_id");

            migrationBuilder.CreateIndex(
                name: "IX_education_emp_id",
                schema: "dotnet",
                table: "education",
                column: "emp_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_dept_id",
                schema: "dotnet",
                table: "employee",
                column: "dept_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_email",
                schema: "dotnet",
                table: "employee",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_employee_manager_id",
                schema: "dotnet",
                table: "employee",
                column: "manager_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_position_id",
                schema: "dotnet",
                table: "employee",
                column: "position_id");

            migrationBuilder.CreateIndex(
                name: "IX_request_ApproverEmployeeId",
                schema: "dotnet",
                table: "request",
                column: "ApproverEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_request_RequesterEmployeeId",
                schema: "dotnet",
                table: "request",
                column: "RequesterEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_request_Status",
                schema: "dotnet",
                table: "request",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_attendance_record_employee_EmployeeId",
                schema: "dotnet",
                table: "attendance_record",
                column: "EmployeeId",
                principalSchema: "dotnet",
                principalTable: "employee",
                principalColumn: "emp_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_bank_account_employee_emp_id",
                schema: "dotnet",
                table: "bank_account",
                column: "emp_id",
                principalSchema: "dotnet",
                principalTable: "employee",
                principalColumn: "emp_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_department_employee_manager_id",
                schema: "public",
                table: "department",
                column: "manager_id",
                principalSchema: "dotnet",
                principalTable: "employee",
                principalColumn: "emp_id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_department_employee_manager_id",
                schema: "public",
                table: "department");

            migrationBuilder.DropTable(
                name: "attendance_record",
                schema: "dotnet");

            migrationBuilder.DropTable(
                name: "bank_account",
                schema: "dotnet");

            migrationBuilder.DropTable(
                name: "education",
                schema: "dotnet");

            migrationBuilder.DropTable(
                name: "request",
                schema: "dotnet");

            migrationBuilder.DropTable(
                name: "employee",
                schema: "dotnet");

            migrationBuilder.DropTable(
                name: "department",
                schema: "public");

            migrationBuilder.DropTable(
                name: "position",
                schema: "public");
        }
    }
}

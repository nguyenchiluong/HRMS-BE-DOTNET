using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EmployeeApi.Migrations
{
    /// <inheritdoc />
    public partial class AddTimesheetFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_request_employee_ApproverEmployeeId",
                schema: "dotnet",
                table: "request");

            migrationBuilder.DropForeignKey(
                name: "FK_request_employee_RequesterEmployeeId",
                schema: "dotnet",
                table: "request");

            migrationBuilder.RenameColumn(
                name: "Status",
                schema: "dotnet",
                table: "request",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Reason",
                schema: "dotnet",
                table: "request",
                newName: "reason");

            migrationBuilder.RenameColumn(
                name: "Payload",
                schema: "dotnet",
                table: "request",
                newName: "payload");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "dotnet",
                table: "request",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                schema: "dotnet",
                table: "request",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "RequesterEmployeeId",
                schema: "dotnet",
                table: "request",
                newName: "requester_employee_id");

            migrationBuilder.RenameColumn(
                name: "RequestedAt",
                schema: "dotnet",
                table: "request",
                newName: "requested_at");

            migrationBuilder.RenameColumn(
                name: "RequestType",
                schema: "dotnet",
                table: "request",
                newName: "request_type");

            migrationBuilder.RenameColumn(
                name: "RejectionReason",
                schema: "dotnet",
                table: "request",
                newName: "rejection_reason");

            migrationBuilder.RenameColumn(
                name: "EffectiveTo",
                schema: "dotnet",
                table: "request",
                newName: "effective_to");

            migrationBuilder.RenameColumn(
                name: "EffectiveFrom",
                schema: "dotnet",
                table: "request",
                newName: "effective_from");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "dotnet",
                table: "request",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ApproverEmployeeId",
                schema: "dotnet",
                table: "request",
                newName: "approver_employee_id");

            migrationBuilder.RenameColumn(
                name: "ApprovalComment",
                schema: "dotnet",
                table: "request",
                newName: "approval_comment");

            migrationBuilder.RenameIndex(
                name: "IX_request_Status",
                schema: "dotnet",
                table: "request",
                newName: "IX_request_status");

            migrationBuilder.RenameIndex(
                name: "IX_request_RequesterEmployeeId",
                schema: "dotnet",
                table: "request",
                newName: "IX_request_requester_employee_id");

            migrationBuilder.RenameIndex(
                name: "IX_request_ApproverEmployeeId",
                schema: "dotnet",
                table: "request",
                newName: "IX_request_approver_employee_id");

            migrationBuilder.CreateTable(
                name: "timesheet_task",
                schema: "dotnet",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    task_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    task_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    task_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_timesheet_task", x => x.id);
                    table.CheckConstraint("chk_task_type", "task_type IN ('project', 'leave')");
                });

            migrationBuilder.CreateTable(
                name: "timesheet_entry",
                schema: "dotnet",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    request_id = table.Column<int>(type: "integer", nullable: false),
                    employee_id = table.Column<long>(type: "bigint", nullable: false),
                    task_id = table.Column<int>(type: "integer", nullable: false),
                    entry_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    week_start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    week_end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    hours = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_timesheet_entry", x => x.id);
                    table.CheckConstraint("chk_entry_type", "entry_type IN ('project', 'leave')");
                    table.CheckConstraint("chk_hours", "hours >= 0 AND hours <= 168");
                    table.ForeignKey(
                        name: "FK_timesheet_entry_employee_employee_id",
                        column: x => x.employee_id,
                        principalSchema: "dotnet",
                        principalTable: "employee",
                        principalColumn: "emp_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_timesheet_entry_request_request_id",
                        column: x => x.request_id,
                        principalSchema: "dotnet",
                        principalTable: "request",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_timesheet_entry_timesheet_task_task_id",
                        column: x => x.task_id,
                        principalSchema: "dotnet",
                        principalTable: "timesheet_task",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_timesheet_entry_employee_id",
                schema: "dotnet",
                table: "timesheet_entry",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_timesheet_entry_employee_id_task_id_week_start_date",
                schema: "dotnet",
                table: "timesheet_entry",
                columns: new[] { "employee_id", "task_id", "week_start_date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_timesheet_entry_request_id",
                schema: "dotnet",
                table: "timesheet_entry",
                column: "request_id");

            migrationBuilder.CreateIndex(
                name: "IX_timesheet_entry_task_id",
                schema: "dotnet",
                table: "timesheet_entry",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "IX_timesheet_entry_week_start_date_week_end_date",
                schema: "dotnet",
                table: "timesheet_entry",
                columns: new[] { "week_start_date", "week_end_date" });

            migrationBuilder.CreateIndex(
                name: "IX_timesheet_task_is_active",
                schema: "dotnet",
                table: "timesheet_task",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_timesheet_task_task_code",
                schema: "dotnet",
                table: "timesheet_task",
                column: "task_code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_request_employee_approver_employee_id",
                schema: "dotnet",
                table: "request",
                column: "approver_employee_id",
                principalSchema: "dotnet",
                principalTable: "employee",
                principalColumn: "emp_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_request_employee_requester_employee_id",
                schema: "dotnet",
                table: "request",
                column: "requester_employee_id",
                principalSchema: "dotnet",
                principalTable: "employee",
                principalColumn: "emp_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_request_employee_approver_employee_id",
                schema: "dotnet",
                table: "request");

            migrationBuilder.DropForeignKey(
                name: "FK_request_employee_requester_employee_id",
                schema: "dotnet",
                table: "request");

            migrationBuilder.DropTable(
                name: "timesheet_entry",
                schema: "dotnet");

            migrationBuilder.DropTable(
                name: "timesheet_task",
                schema: "dotnet");

            migrationBuilder.RenameColumn(
                name: "status",
                schema: "dotnet",
                table: "request",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "reason",
                schema: "dotnet",
                table: "request",
                newName: "Reason");

            migrationBuilder.RenameColumn(
                name: "payload",
                schema: "dotnet",
                table: "request",
                newName: "Payload");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "dotnet",
                table: "request",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                schema: "dotnet",
                table: "request",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "requester_employee_id",
                schema: "dotnet",
                table: "request",
                newName: "RequesterEmployeeId");

            migrationBuilder.RenameColumn(
                name: "requested_at",
                schema: "dotnet",
                table: "request",
                newName: "RequestedAt");

            migrationBuilder.RenameColumn(
                name: "request_type",
                schema: "dotnet",
                table: "request",
                newName: "RequestType");

            migrationBuilder.RenameColumn(
                name: "rejection_reason",
                schema: "dotnet",
                table: "request",
                newName: "RejectionReason");

            migrationBuilder.RenameColumn(
                name: "effective_to",
                schema: "dotnet",
                table: "request",
                newName: "EffectiveTo");

            migrationBuilder.RenameColumn(
                name: "effective_from",
                schema: "dotnet",
                table: "request",
                newName: "EffectiveFrom");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "dotnet",
                table: "request",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "approver_employee_id",
                schema: "dotnet",
                table: "request",
                newName: "ApproverEmployeeId");

            migrationBuilder.RenameColumn(
                name: "approval_comment",
                schema: "dotnet",
                table: "request",
                newName: "ApprovalComment");

            migrationBuilder.RenameIndex(
                name: "IX_request_status",
                schema: "dotnet",
                table: "request",
                newName: "IX_request_Status");

            migrationBuilder.RenameIndex(
                name: "IX_request_requester_employee_id",
                schema: "dotnet",
                table: "request",
                newName: "IX_request_RequesterEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_request_approver_employee_id",
                schema: "dotnet",
                table: "request",
                newName: "IX_request_ApproverEmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_request_employee_ApproverEmployeeId",
                schema: "dotnet",
                table: "request",
                column: "ApproverEmployeeId",
                principalSchema: "dotnet",
                principalTable: "employee",
                principalColumn: "emp_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_request_employee_RequesterEmployeeId",
                schema: "dotnet",
                table: "request",
                column: "RequesterEmployeeId",
                principalSchema: "dotnet",
                principalTable: "employee",
                principalColumn: "emp_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

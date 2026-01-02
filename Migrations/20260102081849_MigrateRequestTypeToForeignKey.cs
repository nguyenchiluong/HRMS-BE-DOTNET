using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeApi.Migrations
{
    /// <inheritdoc />
    public partial class MigrateRequestTypeToForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Populate request_type_id from request_type string for existing records
            migrationBuilder.Sql(@"
                UPDATE dotnet.request r
                SET request_type_id = rt.request_type_id
                FROM dotnet.request_type rt
                WHERE UPPER(REPLACE(r.request_type, '-', '_')) = rt.request_type_code
                  AND r.request_type_id IS NULL;
            ");

            // Step 2: Set a default for any remaining NULL values (shouldn't happen if data is clean)
            // Use TIMESHEET_WEEKLY as fallback (ID 6 from seed data)
            migrationBuilder.Sql(@"
                UPDATE dotnet.request
                SET request_type_id = 6
                WHERE request_type_id IS NULL;
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_request_request_type_request_type_id",
                schema: "dotnet",
                table: "request");

            migrationBuilder.DropColumn(
                name: "request_type",
                schema: "dotnet",
                table: "request");

            migrationBuilder.AlterColumn<long>(
                name: "request_type_id",
                schema: "dotnet",
                table: "request",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_request_request_type_request_type_id",
                schema: "dotnet",
                table: "request",
                column: "request_type_id",
                principalSchema: "dotnet",
                principalTable: "request_type",
                principalColumn: "request_type_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_request_request_type_request_type_id",
                schema: "dotnet",
                table: "request");

            migrationBuilder.AlterColumn<long>(
                name: "request_type_id",
                schema: "dotnet",
                table: "request",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<string>(
                name: "request_type",
                schema: "dotnet",
                table: "request",
                type: "text",
                nullable: false,
                defaultValue: "");

            // Populate request_type from request_type_id
            migrationBuilder.Sql(@"
                UPDATE dotnet.request r
                SET request_type = rt.request_type_code
                FROM dotnet.request_type rt
                WHERE r.request_type_id = rt.request_type_id;
            ");

            migrationBuilder.AddForeignKey(
                name: "FK_request_request_type_request_type_id",
                schema: "dotnet",
                table: "request",
                column: "request_type_id",
                principalSchema: "dotnet",
                principalTable: "request_type",
                principalColumn: "request_type_id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}

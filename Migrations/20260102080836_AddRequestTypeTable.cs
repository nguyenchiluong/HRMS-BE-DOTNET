using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeApi.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestTypeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "request_type_id",
                schema: "dotnet",
                table: "request",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "request_type",
                schema: "dotnet",
                columns: table => new
                {
                    request_type_id = table.Column<long>(type: "bigint", nullable: false),
                    request_type_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    request_type_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    kebab_case = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    category = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    requires_approval = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_request_type", x => x.request_type_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_request_request_type_id",
                schema: "dotnet",
                table: "request",
                column: "request_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_request_type_is_active",
                schema: "dotnet",
                table: "request_type",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_request_type_request_type_code",
                schema: "dotnet",
                table: "request_type",
                column: "request_type_code",
                unique: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_request_request_type_request_type_id",
                schema: "dotnet",
                table: "request");

            migrationBuilder.DropTable(
                name: "request_type",
                schema: "dotnet");

            migrationBuilder.DropIndex(
                name: "IX_request_request_type_id",
                schema: "dotnet",
                table: "request");

            migrationBuilder.DropColumn(
                name: "request_type_id",
                schema: "dotnet",
                table: "request");
        }
    }
}

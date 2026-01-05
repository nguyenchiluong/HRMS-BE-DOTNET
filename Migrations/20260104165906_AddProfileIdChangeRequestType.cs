using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace EmployeeApi.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileIdChangeRequestType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var now = DateTime.UtcNow;

            // Add PROFILE_ID_CHANGE request type
            migrationBuilder.InsertData(
                table: "request_type",
                schema: "dotnet",
                columns: new[] { 
                    "request_type_id", 
                    "request_type_code", 
                    "request_type_name", 
                    "category", 
                    "description", 
                    "is_active", 
                    "requires_approval", 
                    "created_at", 
                    "updated_at" 
                },
                values: new object[]
                {
                    9L, 
                    "PROFILE_ID_CHANGE", 
                    "Profile ID Change", 
                    "profile", 
                    "Request to change employee identification information (National ID, Social Insurance Number, Tax ID, Legal Name)", 
                    true, 
                    true, 
                    now, 
                    now 
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove PROFILE_ID_CHANGE request type
            migrationBuilder.DeleteData(
                table: "request_type",
                schema: "dotnet",
                keyColumn: "request_type_id",
                keyValue: 9L);
        }
    }
}

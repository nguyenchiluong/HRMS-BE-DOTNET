using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace EmployeeApi.Migrations
{
    /// <inheritdoc />
    public partial class SeedRequestTypeData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var now = DateTime.UtcNow;

            // Seed Request Types
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
                values: new object[,]
                {
                    // Time-Off types
                    { 1L, "PAID_LEAVE", "Paid Leave", "time-off", "Paid annual leave", true, true, now, now },
                    { 2L, "UNPAID_LEAVE", "Unpaid Leave", "time-off", "Unpaid annual leave", true, true, now, now },
                    { 3L, "PAID_SICK_LEAVE", "Paid Sick Leave", "time-off", "Paid sick leave", true, true, now, now },
                    { 4L, "UNPAID_SICK_LEAVE", "Unpaid Sick Leave", "time-off", "Unpaid sick leave", true, true, now, now },
                    { 5L, "WFH", "Work From Home", "time-off", "Work from home", true, true, now, now },
                    
                    // Timesheet types
                    { 6L, "TIMESHEET_WEEKLY", "Weekly Timesheet", "timesheet", "Weekly timesheet submission", true, true, now, now },
                    
                    // Profile update types (legacy)
                    { 7L, "PROFILE_UPDATE", "Profile Update", "profile", "Profile update request", true, true, now, now },
                    { 8L, "ID_UPDATE", "ID Update", "profile", "ID update request", true, true, now, now }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove seeded Request Types
            migrationBuilder.DeleteData(
                table: "request_type",
                schema: "dotnet",
                keyColumn: "request_type_id",
                keyValues: new object[] { 1L, 2L, 3L, 4L, 5L, 6L, 7L, 8L });
        }
    }
}

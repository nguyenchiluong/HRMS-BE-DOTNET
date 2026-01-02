using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeApi.Migrations
{
    /// <inheritdoc />
    public partial class SeedTimesheetTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var utcNow = DateTime.UtcNow;

            // Seed Project Tasks
            migrationBuilder.InsertData(
                table: "timesheet_task",
                schema: "dotnet",
                columns: new[] { "task_code", "task_name", "description", "task_type", "is_active", "created_at", "updated_at" },
                values: new object[,]
                {
                    { "DL001", "Daily Learning", "Time spent on daily learning activities, training, and skill development", "project", true, utcNow, utcNow },
                    { "PROJ-A", "Project Alpha", "Main project Alpha development work", "project", true, utcNow, utcNow },
                    { "PROJ-B", "Project Beta", "Main project Beta development work", "project", true, utcNow, utcNow },
                    { "PROJ-C", "Project Gamma", "Main project Gamma development work", "project", true, utcNow, utcNow },
                    { "MAINT", "Maintenance", "System maintenance and bug fixes", "project", true, utcNow, utcNow },
                    { "CODE-REV", "Code Review", "Time spent on code reviews and technical discussions", "project", true, utcNow, utcNow },
                    { "MEETING", "Meetings", "Team meetings, standups, and planning sessions", "project", true, utcNow, utcNow },
                    { "DOC", "Documentation", "Writing and maintaining technical documentation", "project", true, utcNow, utcNow },
                    { "RESEARCH", "Research & Development", "Research activities and proof of concepts", "project", true, utcNow, utcNow },
                    { "ADMIN", "Administrative Tasks", "Administrative and non-project related tasks", "project", true, utcNow, utcNow }
                });

            // Seed Leave Tasks
            migrationBuilder.InsertData(
                table: "timesheet_task",
                schema: "dotnet",
                columns: new[] { "task_code", "task_name", "description", "task_type", "is_active", "created_at", "updated_at" },
                values: new object[,]
                {
                    { "AL", "Annual Leave", "Annual vacation leave", "leave", true, utcNow, utcNow },
                    { "SL", "Sick Leave", "Medical leave for illness", "leave", true, utcNow, utcNow },
                    { "PL", "Personal Leave", "Personal time off", "leave", true, utcNow, utcNow },
                    { "ML", "Maternity Leave", "Maternity leave", "leave", true, utcNow, utcNow },
                    { "PTL", "Paternity Leave", "Paternity leave", "leave", true, utcNow, utcNow },
                    { "BL", "Bereavement Leave", "Leave for family bereavement", "leave", true, utcNow, utcNow },
                    { "UL", "Unpaid Leave", "Unpaid time off", "leave", true, utcNow, utcNow }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove seeded tasks by task_code
            migrationBuilder.Sql(@"
                DELETE FROM dotnet.timesheet_task 
                WHERE task_code IN (
                    'DL001', 'PROJ-A', 'PROJ-B', 'PROJ-C', 'MAINT', 'CODE-REV', 'MEETING', 'DOC', 'RESEARCH', 'ADMIN',
                    'AL', 'SL', 'PL', 'ML', 'PTL', 'BL', 'UL'
                );
            ");
        }
    }
}

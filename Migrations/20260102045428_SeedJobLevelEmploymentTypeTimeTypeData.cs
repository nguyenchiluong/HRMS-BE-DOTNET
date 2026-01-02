using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeApi.Migrations
{
    /// <inheritdoc />
    public partial class SeedJobLevelEmploymentTypeTimeTypeData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Seed Job Levels
            migrationBuilder.InsertData(
                table: "job_level",
                schema: "dotnet",
                columns: new[] { "job_level_id", "job_level_name", "description" },
                values: new object[,]
                {
                    { 1L, "Intern", "Entry-level position for learning and gaining experience" },
                    { 2L, "Junior", "Early career professional with basic skills" },
                    { 3L, "Mid-level", "Experienced professional with solid skills" },
                    { 4L, "Senior", "Advanced professional with extensive experience" },
                    { 5L, "Lead", "Technical leader responsible for guiding teams" },
                    { 6L, "Principal", "Expert-level professional with deep expertise" },
                    { 7L, "Manager", "Management role responsible for teams and projects" },
                    { 8L, "Director", "Senior management role with strategic responsibilities" }
                });

            // Seed Employment Types
            migrationBuilder.InsertData(
                table: "employment_type",
                schema: "dotnet",
                columns: new[] { "employment_type_id", "employment_type_name", "description" },
                values: new object[,]
                {
                    { 1L, "Full-time", "Full-time permanent employment" },
                    { 2L, "Part-time", "Part-time employment with reduced hours" },
                    { 3L, "Contract", "Contract-based employment for a specific period" },
                    { 4L, "Intern", "Internship position for students or recent graduates" },
                    { 5L, "Temporary", "Temporary employment for short-term projects" },
                    { 6L, "Consultant", "Consulting role with flexible arrangements" }
                });

            // Seed Time Types
            migrationBuilder.InsertData(
                table: "time_type",
                schema: "dotnet",
                columns: new[] { "time_type_id", "time_type_name", "description" },
                values: new object[,]
                {
                    { 1L, "On-site", "Work from office location" },
                    { 2L, "Remote", "Work from home or remote location" },
                    { 3L, "Hybrid", "Combination of on-site and remote work" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove seeded Time Types
            migrationBuilder.DeleteData(
                table: "time_type",
                schema: "dotnet",
                keyColumn: "time_type_id",
                keyValues: new object[] { 1L, 2L, 3L });

            // Remove seeded Employment Types
            migrationBuilder.DeleteData(
                table: "employment_type",
                schema: "dotnet",
                keyColumn: "employment_type_id",
                keyValues: new object[] { 1L, 2L, 3L, 4L, 5L, 6L });

            // Remove seeded Job Levels
            migrationBuilder.DeleteData(
                table: "job_level",
                schema: "dotnet",
                keyColumn: "job_level_id",
                keyValues: new object[] { 1L, 2L, 3L, 4L, 5L, 6L, 7L, 8L });
        }
    }
}

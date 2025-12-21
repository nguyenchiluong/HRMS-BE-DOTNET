using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeApi.Migrations
{
    /// <inheritdoc />
    public partial class SeedPositionAndDepartmentData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Seed Positions
            migrationBuilder.InsertData(
                table: "position",
                schema: "dotnet",
                columns: new[] { "position_id", "position_name", "description", "salary" },
                values: new object[,]
                {
                    { 1L, "Software Engineer", "Develops and maintains software applications", 1500.00m },
                    { 2L, "Senior Software Engineer", "Leads development of complex software systems", 2500.00m },
                    { 3L, "Tech Lead", "Technical leadership and architecture decisions", 3500.00m },
                    { 4L, "Engineering Manager", "Manages engineering teams and projects", 4500.00m },
                    { 5L, "Product Manager", "Defines product strategy and roadmap", 3000.00m },
                    { 6L, "QA Engineer", "Ensures software quality through testing", 1400.00m },
                    { 7L, "DevOps Engineer", "Manages CI/CD pipelines and infrastructure", 2000.00m },
                    { 8L, "Data Analyst", "Analyzes data and provides business insights", 1600.00m },
                    { 9L, "HR Specialist", "Handles recruitment and employee relations", 1200.00m },
                    { 10L, "Finance Analyst", "Manages financial planning and analysis", 1800.00m }
                });

            // Seed Departments
            migrationBuilder.InsertData(
                table: "department",
                schema: "dotnet",
                columns: new[] { "dept_id", "dept_name", "location", "manager_id" },
                values: new object[,]
                {
                    { 1L, "Engineering", "Building A, Floor 3", null },
                    { 2L, "Product", "Building A, Floor 2", null },
                    { 3L, "Quality Assurance", "Building B, Floor 1", null },
                    { 4L, "DevOps", "Building A, Floor 3", null },
                    { 5L, "Data & Analytics", "Building B, Floor 2", null },
                    { 6L, "Human Resources", "Building C, Floor 1", null },
                    { 7L, "Finance", "Building C, Floor 2", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove seeded Departments
            migrationBuilder.DeleteData(
                table: "department",
                schema: "dotnet",
                keyColumn: "dept_id",
                keyValues: new object[] { 1L, 2L, 3L, 4L, 5L, 6L, 7L });

            // Remove seeded Positions
            migrationBuilder.DeleteData(
                table: "position",
                schema: "dotnet",
                keyColumn: "position_id",
                keyValues: new object[] { 1L, 2L, 3L, 4L, 5L, 6L, 7L, 8L, 9L, 10L });
        }
    }
}

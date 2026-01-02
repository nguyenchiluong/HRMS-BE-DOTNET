using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeApi.Migrations
{
    /// <inheritdoc />
    public partial class SeedLeaveBalances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Leave balances are initialized programmatically when needed
            // Default values are set in TimeOffService.GetLeaveBalancesAsync():
            // - Annual Leave: 15 days
            // - Sick Leave: 10 days
            // - Parental Leave: 14 days
            // - Other Leave: 5 days
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}

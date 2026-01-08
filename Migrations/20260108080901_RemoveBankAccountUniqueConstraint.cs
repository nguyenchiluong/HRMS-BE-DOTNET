using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeApi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBankAccountUniqueConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_bank_account_account_number_bank_name",
                schema: "dotnet",
                table: "bank_account");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_bank_account_account_number_bank_name",
                schema: "dotnet",
                table: "bank_account",
                columns: new[] { "account_number", "bank_name" },
                unique: true);
        }
    }
}

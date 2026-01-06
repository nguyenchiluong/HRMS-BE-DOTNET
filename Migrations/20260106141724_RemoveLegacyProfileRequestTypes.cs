using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeApi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLegacyProfileRequestTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Update any existing requests using legacy request types 7 or 8 to use type 9 (PROFILE_ID_CHANGE)
            // This is necessary because the foreign key constraint is Restrict, so we can't delete types 7 and 8
            // if there are requests referencing them
            migrationBuilder.Sql(@"
                UPDATE dotnet.request
                SET request_type_id = 9
                WHERE request_type_id IN (7, 8);
            ");

            // Step 2: Delete the legacy request types 7 and 8
            // Now that all requests have been migrated to type 9, we can safely delete the old types
            migrationBuilder.DeleteData(
                table: "request_type",
                schema: "dotnet",
                keyColumn: "request_type_id",
                keyValues: new object[] { 7L, 8L });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore the legacy request types if migration is rolled back
            var now = System.DateTime.UtcNow;
            
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
                    { 7L, "PROFILE_UPDATE", "Profile Update", "profile", "Profile update request", true, true, now, now },
                    { 8L, "ID_UPDATE", "ID Update", "profile", "ID update request", true, true, now, now }
                });

            // Note: We don't restore requests back to types 7 or 8 in the rollback
            // because we can't determine which requests originally used which type
        }
    }
}

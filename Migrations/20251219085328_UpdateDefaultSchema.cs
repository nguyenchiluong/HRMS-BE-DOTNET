using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EmployeeApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDefaultSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create the 'dotnet' schema if it doesn't exist
            migrationBuilder.Sql("CREATE SCHEMA IF NOT EXISTS dotnet;");

            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceRecords_Employees_EmployeeId",
                table: "AttendanceRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_BankAccounts_Employees_EmployeeId",
                table: "BankAccounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Employees_ManagerId",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_Educations_Employees_EmployeeId",
                table: "Educations");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Departments_DepartmentId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Employees_ManagerId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Positions_PositionId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Employees_ApproverEmployeeId",
                table: "Requests");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Employees_RequesterEmployeeId",
                table: "Requests");

            migrationBuilder.DropTable(
                name: "CampaignParticipants");

            migrationBuilder.DropTable(
                name: "EmployeeActivities");

            migrationBuilder.DropTable(
                name: "RedemptionTransactions");

            migrationBuilder.DropTable(
                name: "TransferTransactions");

            migrationBuilder.DropTable(
                name: "Campaigns");

            migrationBuilder.DropTable(
                name: "BonusPointAccounts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Requests",
                table: "Requests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Positions",
                table: "Positions");

            migrationBuilder.DropIndex(
                name: "IX_Positions_Code",
                table: "Positions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Employees",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_EmployeeNumber",
                table: "Employees");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Educations",
                table: "Educations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Departments",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Departments_Code",
                table: "Departments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BankAccounts",
                table: "BankAccounts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AttendanceRecords",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "Grade",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "BirthDate",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "HireDate",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Educations");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Educations");

            migrationBuilder.DropColumn(
                name: "Institution",
                table: "Educations");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Educations");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Educations");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "BankAccounts");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "BankAccounts");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "BankAccounts");

            migrationBuilder.DropColumn(
                name: "IsPrimary",
                table: "BankAccounts");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "BankAccounts");

            migrationBuilder.EnsureSchema(
                name: "dotnet");

            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.RenameTable(
                name: "Requests",
                newName: "request",
                newSchema: "dotnet");

            migrationBuilder.RenameTable(
                name: "Positions",
                newName: "position",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Employees",
                newName: "employee",
                newSchema: "dotnet");

            migrationBuilder.RenameTable(
                name: "Educations",
                newName: "education",
                newSchema: "dotnet");

            migrationBuilder.RenameTable(
                name: "Departments",
                newName: "department",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "BankAccounts",
                newName: "bank_account",
                newSchema: "dotnet");

            migrationBuilder.RenameTable(
                name: "AttendanceRecords",
                newName: "attendance_record",
                newSchema: "dotnet");

            migrationBuilder.RenameIndex(
                name: "IX_Requests_Status",
                schema: "dotnet",
                table: "request",
                newName: "IX_request_Status");

            migrationBuilder.RenameIndex(
                name: "IX_Requests_RequesterEmployeeId",
                schema: "dotnet",
                table: "request",
                newName: "IX_request_RequesterEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_Requests_ApproverEmployeeId",
                schema: "dotnet",
                table: "request",
                newName: "IX_request_ApproverEmployeeId");

            migrationBuilder.RenameColumn(
                name: "Description",
                schema: "public",
                table: "position",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Title",
                schema: "public",
                table: "position",
                newName: "position_name");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "public",
                table: "position",
                newName: "position_id");

            migrationBuilder.RenameColumn(
                name: "Email",
                schema: "dotnet",
                table: "employee",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                schema: "dotnet",
                table: "employee",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "PositionId",
                schema: "dotnet",
                table: "employee",
                newName: "position_id");

            migrationBuilder.RenameColumn(
                name: "Phone",
                schema: "dotnet",
                table: "employee",
                newName: "phone_number");

            migrationBuilder.RenameColumn(
                name: "ManagerId",
                schema: "dotnet",
                table: "employee",
                newName: "manager_id");

            migrationBuilder.RenameColumn(
                name: "LastName",
                schema: "dotnet",
                table: "employee",
                newName: "last_name");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                schema: "dotnet",
                table: "employee",
                newName: "first_name");

            migrationBuilder.RenameColumn(
                name: "DepartmentId",
                schema: "dotnet",
                table: "employee",
                newName: "dept_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "dotnet",
                table: "employee",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "dotnet",
                table: "employee",
                newName: "emp_id");

            migrationBuilder.RenameColumn(
                name: "JobStatus",
                schema: "dotnet",
                table: "employee",
                newName: "time_type");

            migrationBuilder.RenameColumn(
                name: "EmployeeNumber",
                schema: "dotnet",
                table: "employee",
                newName: "tax_id");

            migrationBuilder.RenameIndex(
                name: "IX_Employees_PositionId",
                schema: "dotnet",
                table: "employee",
                newName: "IX_employee_position_id");

            migrationBuilder.RenameIndex(
                name: "IX_Employees_ManagerId",
                schema: "dotnet",
                table: "employee",
                newName: "IX_employee_manager_id");

            migrationBuilder.RenameIndex(
                name: "IX_Employees_Email",
                schema: "dotnet",
                table: "employee",
                newName: "IX_employee_email");

            migrationBuilder.RenameIndex(
                name: "IX_Employees_DepartmentId",
                schema: "dotnet",
                table: "employee",
                newName: "IX_employee_dept_id");

            migrationBuilder.RenameColumn(
                name: "Gpa",
                schema: "dotnet",
                table: "education",
                newName: "gpa");

            migrationBuilder.RenameColumn(
                name: "Degree",
                schema: "dotnet",
                table: "education",
                newName: "degree");

            migrationBuilder.RenameColumn(
                name: "Country",
                schema: "dotnet",
                table: "education",
                newName: "country");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "dotnet",
                table: "education",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "FieldOfStudy",
                schema: "dotnet",
                table: "education",
                newName: "field_of_study");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                schema: "dotnet",
                table: "education",
                newName: "emp_id");

            migrationBuilder.RenameIndex(
                name: "IX_Educations_EmployeeId",
                schema: "dotnet",
                table: "education",
                newName: "IX_education_emp_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "public",
                table: "department",
                newName: "dept_name");

            migrationBuilder.RenameColumn(
                name: "ManagerId",
                schema: "public",
                table: "department",
                newName: "manager_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "public",
                table: "department",
                newName: "dept_id");

            migrationBuilder.RenameColumn(
                name: "Description",
                schema: "public",
                table: "department",
                newName: "location");

            migrationBuilder.RenameIndex(
                name: "IX_Departments_ManagerId",
                schema: "public",
                table: "department",
                newName: "IX_department_manager_id");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                schema: "dotnet",
                table: "bank_account",
                newName: "emp_id");

            migrationBuilder.RenameColumn(
                name: "BankName",
                schema: "dotnet",
                table: "bank_account",
                newName: "bank_name");

            migrationBuilder.RenameColumn(
                name: "AccountNumber",
                schema: "dotnet",
                table: "bank_account",
                newName: "account_number");

            migrationBuilder.RenameColumn(
                name: "AccountName",
                schema: "dotnet",
                table: "bank_account",
                newName: "account_name");

            migrationBuilder.RenameIndex(
                name: "IX_BankAccounts_EmployeeId",
                schema: "dotnet",
                table: "bank_account",
                newName: "IX_bank_account_emp_id");

            migrationBuilder.RenameIndex(
                name: "IX_BankAccounts_AccountNumber",
                schema: "dotnet",
                table: "bank_account",
                newName: "IX_bank_account_account_number");

            migrationBuilder.RenameIndex(
                name: "IX_AttendanceRecords_EmployeeId",
                schema: "dotnet",
                table: "attendance_record",
                newName: "IX_attendance_record_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_AttendanceRecords_Date",
                schema: "dotnet",
                table: "attendance_record",
                newName: "IX_attendance_record_Date");

            migrationBuilder.AlterColumn<long>(
                name: "position_id",
                schema: "public",
                table: "position",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<decimal>(
                name: "salary",
                schema: "public",
                table: "position",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "email",
                schema: "dotnet",
                table: "employee",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                schema: "dotnet",
                table: "employee",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "last_name",
                schema: "dotnet",
                table: "employee",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "first_name",
                schema: "dotnet",
                table: "employee",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                schema: "dotnet",
                table: "employee",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<long>(
                name: "emp_id",
                schema: "dotnet",
                table: "employee",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "current_address",
                schema: "dotnet",
                table: "employee",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "date_of_birth",
                schema: "dotnet",
                table: "employee",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "employee_type",
                schema: "dotnet",
                table: "employee",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "full_name",
                schema: "dotnet",
                table: "employee",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "job_level",
                schema: "dotnet",
                table: "employee",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "marital_status",
                schema: "dotnet",
                table: "employee",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "national_id_country",
                schema: "dotnet",
                table: "employee",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "national_id_expiration_date",
                schema: "dotnet",
                table: "employee",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "national_id_issued_by",
                schema: "dotnet",
                table: "employee",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "national_id_issued_date",
                schema: "dotnet",
                table: "employee",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "national_id_number",
                schema: "dotnet",
                table: "employee",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "permanent_address",
                schema: "dotnet",
                table: "employee",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "personal_email",
                schema: "dotnet",
                table: "employee",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "phone_number_2",
                schema: "dotnet",
                table: "employee",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "preferred_name",
                schema: "dotnet",
                table: "employee",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pronoun",
                schema: "dotnet",
                table: "employee",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "sex",
                schema: "dotnet",
                table: "employee",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "social_insurance_number",
                schema: "dotnet",
                table: "employee",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "start_date",
                schema: "dotnet",
                table: "employee",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status",
                schema: "dotnet",
                table: "employee",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "gpa",
                schema: "dotnet",
                table: "education",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "degree",
                schema: "dotnet",
                table: "education",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "dept_id",
                schema: "public",
                table: "department",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_request",
                schema: "dotnet",
                table: "request",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_position",
                schema: "public",
                table: "position",
                column: "position_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_employee",
                schema: "dotnet",
                table: "employee",
                column: "emp_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_education",
                schema: "dotnet",
                table: "education",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_department",
                schema: "public",
                table: "department",
                column: "dept_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_bank_account",
                schema: "dotnet",
                table: "bank_account",
                columns: new[] { "account_number", "bank_name" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_attendance_record",
                schema: "dotnet",
                table: "attendance_record",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_attendance_record_employee_EmployeeId",
                schema: "dotnet",
                table: "attendance_record",
                column: "EmployeeId",
                principalSchema: "dotnet",
                principalTable: "employee",
                principalColumn: "emp_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_bank_account_employee_emp_id",
                schema: "dotnet",
                table: "bank_account",
                column: "emp_id",
                principalSchema: "dotnet",
                principalTable: "employee",
                principalColumn: "emp_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_department_employee_manager_id",
                schema: "public",
                table: "department",
                column: "manager_id",
                principalSchema: "dotnet",
                principalTable: "employee",
                principalColumn: "emp_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_education_employee_emp_id",
                schema: "dotnet",
                table: "education",
                column: "emp_id",
                principalSchema: "dotnet",
                principalTable: "employee",
                principalColumn: "emp_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_employee_department_dept_id",
                schema: "dotnet",
                table: "employee",
                column: "dept_id",
                principalSchema: "public",
                principalTable: "department",
                principalColumn: "dept_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_employee_employee_manager_id",
                schema: "dotnet",
                table: "employee",
                column: "manager_id",
                principalSchema: "dotnet",
                principalTable: "employee",
                principalColumn: "emp_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_employee_position_position_id",
                schema: "dotnet",
                table: "employee",
                column: "position_id",
                principalSchema: "public",
                principalTable: "position",
                principalColumn: "position_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_request_employee_ApproverEmployeeId",
                schema: "dotnet",
                table: "request",
                column: "ApproverEmployeeId",
                principalSchema: "dotnet",
                principalTable: "employee",
                principalColumn: "emp_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_request_employee_RequesterEmployeeId",
                schema: "dotnet",
                table: "request",
                column: "RequesterEmployeeId",
                principalSchema: "dotnet",
                principalTable: "employee",
                principalColumn: "emp_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_attendance_record_employee_EmployeeId",
                schema: "dotnet",
                table: "attendance_record");

            migrationBuilder.DropForeignKey(
                name: "FK_bank_account_employee_emp_id",
                schema: "dotnet",
                table: "bank_account");

            migrationBuilder.DropForeignKey(
                name: "FK_department_employee_manager_id",
                schema: "public",
                table: "department");

            migrationBuilder.DropForeignKey(
                name: "FK_education_employee_emp_id",
                schema: "dotnet",
                table: "education");

            migrationBuilder.DropForeignKey(
                name: "FK_employee_department_dept_id",
                schema: "dotnet",
                table: "employee");

            migrationBuilder.DropForeignKey(
                name: "FK_employee_employee_manager_id",
                schema: "dotnet",
                table: "employee");

            migrationBuilder.DropForeignKey(
                name: "FK_employee_position_position_id",
                schema: "dotnet",
                table: "employee");

            migrationBuilder.DropForeignKey(
                name: "FK_request_employee_ApproverEmployeeId",
                schema: "dotnet",
                table: "request");

            migrationBuilder.DropForeignKey(
                name: "FK_request_employee_RequesterEmployeeId",
                schema: "dotnet",
                table: "request");

            migrationBuilder.DropPrimaryKey(
                name: "PK_request",
                schema: "dotnet",
                table: "request");

            migrationBuilder.DropPrimaryKey(
                name: "PK_position",
                schema: "public",
                table: "position");

            migrationBuilder.DropPrimaryKey(
                name: "PK_employee",
                schema: "dotnet",
                table: "employee");

            migrationBuilder.DropPrimaryKey(
                name: "PK_education",
                schema: "dotnet",
                table: "education");

            migrationBuilder.DropPrimaryKey(
                name: "PK_department",
                schema: "public",
                table: "department");

            migrationBuilder.DropPrimaryKey(
                name: "PK_bank_account",
                schema: "dotnet",
                table: "bank_account");

            migrationBuilder.DropPrimaryKey(
                name: "PK_attendance_record",
                schema: "dotnet",
                table: "attendance_record");

            migrationBuilder.DropColumn(
                name: "salary",
                schema: "public",
                table: "position");

            migrationBuilder.DropColumn(
                name: "current_address",
                schema: "dotnet",
                table: "employee");

            migrationBuilder.DropColumn(
                name: "date_of_birth",
                schema: "dotnet",
                table: "employee");

            migrationBuilder.DropColumn(
                name: "employee_type",
                schema: "dotnet",
                table: "employee");

            migrationBuilder.DropColumn(
                name: "full_name",
                schema: "dotnet",
                table: "employee");

            migrationBuilder.DropColumn(
                name: "job_level",
                schema: "dotnet",
                table: "employee");

            migrationBuilder.DropColumn(
                name: "marital_status",
                schema: "dotnet",
                table: "employee");

            migrationBuilder.DropColumn(
                name: "national_id_country",
                schema: "dotnet",
                table: "employee");

            migrationBuilder.DropColumn(
                name: "national_id_expiration_date",
                schema: "dotnet",
                table: "employee");

            migrationBuilder.DropColumn(
                name: "national_id_issued_by",
                schema: "dotnet",
                table: "employee");

            migrationBuilder.DropColumn(
                name: "national_id_issued_date",
                schema: "dotnet",
                table: "employee");

            migrationBuilder.DropColumn(
                name: "national_id_number",
                schema: "dotnet",
                table: "employee");

            migrationBuilder.DropColumn(
                name: "permanent_address",
                schema: "dotnet",
                table: "employee");

            migrationBuilder.DropColumn(
                name: "personal_email",
                schema: "dotnet",
                table: "employee");

            migrationBuilder.DropColumn(
                name: "phone_number_2",
                schema: "dotnet",
                table: "employee");

            migrationBuilder.DropColumn(
                name: "preferred_name",
                schema: "dotnet",
                table: "employee");

            migrationBuilder.DropColumn(
                name: "pronoun",
                schema: "dotnet",
                table: "employee");

            migrationBuilder.DropColumn(
                name: "sex",
                schema: "dotnet",
                table: "employee");

            migrationBuilder.DropColumn(
                name: "social_insurance_number",
                schema: "dotnet",
                table: "employee");

            migrationBuilder.DropColumn(
                name: "start_date",
                schema: "dotnet",
                table: "employee");

            migrationBuilder.DropColumn(
                name: "status",
                schema: "dotnet",
                table: "employee");

            migrationBuilder.RenameTable(
                name: "request",
                schema: "dotnet",
                newName: "Requests");

            migrationBuilder.RenameTable(
                name: "position",
                schema: "public",
                newName: "Positions");

            migrationBuilder.RenameTable(
                name: "employee",
                schema: "dotnet",
                newName: "Employees");

            migrationBuilder.RenameTable(
                name: "education",
                schema: "dotnet",
                newName: "Educations");

            migrationBuilder.RenameTable(
                name: "department",
                schema: "public",
                newName: "Departments");

            migrationBuilder.RenameTable(
                name: "bank_account",
                schema: "dotnet",
                newName: "BankAccounts");

            migrationBuilder.RenameTable(
                name: "attendance_record",
                schema: "dotnet",
                newName: "AttendanceRecords");

            migrationBuilder.RenameIndex(
                name: "IX_request_Status",
                table: "Requests",
                newName: "IX_Requests_Status");

            migrationBuilder.RenameIndex(
                name: "IX_request_RequesterEmployeeId",
                table: "Requests",
                newName: "IX_Requests_RequesterEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_request_ApproverEmployeeId",
                table: "Requests",
                newName: "IX_Requests_ApproverEmployeeId");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Positions",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "position_name",
                table: "Positions",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "position_id",
                table: "Positions",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "Employees",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "Employees",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "position_id",
                table: "Employees",
                newName: "PositionId");

            migrationBuilder.RenameColumn(
                name: "phone_number",
                table: "Employees",
                newName: "Phone");

            migrationBuilder.RenameColumn(
                name: "manager_id",
                table: "Employees",
                newName: "ManagerId");

            migrationBuilder.RenameColumn(
                name: "last_name",
                table: "Employees",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "first_name",
                table: "Employees",
                newName: "FirstName");

            migrationBuilder.RenameColumn(
                name: "dept_id",
                table: "Employees",
                newName: "DepartmentId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Employees",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "emp_id",
                table: "Employees",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "time_type",
                table: "Employees",
                newName: "JobStatus");

            migrationBuilder.RenameColumn(
                name: "tax_id",
                table: "Employees",
                newName: "EmployeeNumber");

            migrationBuilder.RenameIndex(
                name: "IX_employee_position_id",
                table: "Employees",
                newName: "IX_Employees_PositionId");

            migrationBuilder.RenameIndex(
                name: "IX_employee_manager_id",
                table: "Employees",
                newName: "IX_Employees_ManagerId");

            migrationBuilder.RenameIndex(
                name: "IX_employee_email",
                table: "Employees",
                newName: "IX_Employees_Email");

            migrationBuilder.RenameIndex(
                name: "IX_employee_dept_id",
                table: "Employees",
                newName: "IX_Employees_DepartmentId");

            migrationBuilder.RenameColumn(
                name: "gpa",
                table: "Educations",
                newName: "Gpa");

            migrationBuilder.RenameColumn(
                name: "degree",
                table: "Educations",
                newName: "Degree");

            migrationBuilder.RenameColumn(
                name: "country",
                table: "Educations",
                newName: "Country");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Educations",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "field_of_study",
                table: "Educations",
                newName: "FieldOfStudy");

            migrationBuilder.RenameColumn(
                name: "emp_id",
                table: "Educations",
                newName: "EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_education_emp_id",
                table: "Educations",
                newName: "IX_Educations_EmployeeId");

            migrationBuilder.RenameColumn(
                name: "manager_id",
                table: "Departments",
                newName: "ManagerId");

            migrationBuilder.RenameColumn(
                name: "dept_name",
                table: "Departments",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "dept_id",
                table: "Departments",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "location",
                table: "Departments",
                newName: "Description");

            migrationBuilder.RenameIndex(
                name: "IX_department_manager_id",
                table: "Departments",
                newName: "IX_Departments_ManagerId");

            migrationBuilder.RenameColumn(
                name: "emp_id",
                table: "BankAccounts",
                newName: "EmployeeId");

            migrationBuilder.RenameColumn(
                name: "account_name",
                table: "BankAccounts",
                newName: "AccountName");

            migrationBuilder.RenameColumn(
                name: "bank_name",
                table: "BankAccounts",
                newName: "BankName");

            migrationBuilder.RenameColumn(
                name: "account_number",
                table: "BankAccounts",
                newName: "AccountNumber");

            migrationBuilder.RenameIndex(
                name: "IX_bank_account_emp_id",
                table: "BankAccounts",
                newName: "IX_BankAccounts_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_bank_account_account_number",
                table: "BankAccounts",
                newName: "IX_BankAccounts_AccountNumber");

            migrationBuilder.RenameIndex(
                name: "IX_attendance_record_EmployeeId",
                table: "AttendanceRecords",
                newName: "IX_AttendanceRecords_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_attendance_record_Date",
                table: "AttendanceRecords",
                newName: "IX_AttendanceRecords_Date");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Positions",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Positions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Positions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Grade",
                table: "Positions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Positions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Employees",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Employees",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Employees",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Employees",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Employees",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Employees",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<DateTime>(
                name: "BirthDate",
                table: "Employees",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "HireDate",
                table: "Employees",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Employees",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<decimal>(
                name: "Gpa",
                table: "Educations",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Degree",
                table: "Educations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Educations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Educations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Institution",
                table: "Educations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Educations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Educations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Departments",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Departments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Departments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Departments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<long>(
                name: "Id",
                table: "BankAccounts",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "BankAccounts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "BankAccounts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsPrimary",
                table: "BankAccounts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "BankAccounts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Requests",
                table: "Requests",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Positions",
                table: "Positions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Employees",
                table: "Employees",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Educations",
                table: "Educations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Departments",
                table: "Departments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BankAccounts",
                table: "BankAccounts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AttendanceRecords",
                table: "AttendanceRecords",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "BonusPointAccounts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeId = table.Column<long>(type: "bigint", nullable: false),
                    Balance = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BonusPointAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BonusPointAccounts_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Campaigns",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    PointsReward = table.Column<long>(type: "bigint", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campaigns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RedemptionTransactions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    RedeemedByEmployeeId = table.Column<long>(type: "bigint", nullable: true),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RewardReference = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RedemptionTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RedemptionTransactions_BonusPointAccounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "BonusPointAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RedemptionTransactions_Employees_RedeemedByEmployeeId",
                        column: x => x.RedeemedByEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TransferTransactions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FromAccountId = table.Column<long>(type: "bigint", nullable: false),
                    InitiatedByEmployeeId = table.Column<long>(type: "bigint", nullable: true),
                    ToAccountId = table.Column<long>(type: "bigint", nullable: false),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reference = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransferTransactions_BonusPointAccounts_FromAccountId",
                        column: x => x.FromAccountId,
                        principalTable: "BonusPointAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransferTransactions_BonusPointAccounts_ToAccountId",
                        column: x => x.ToAccountId,
                        principalTable: "BonusPointAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransferTransactions_Employees_InitiatedByEmployeeId",
                        column: x => x.InitiatedByEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CampaignParticipants",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CampaignId = table.Column<long>(type: "bigint", nullable: false),
                    EmployeeId = table.Column<long>(type: "bigint", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PointsEarned = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignParticipants_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignParticipants_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeActivities",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CampaignId = table.Column<long>(type: "bigint", nullable: true),
                    EmployeeId = table.Column<long>(type: "bigint", nullable: false),
                    ActivityDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActivityType = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    PointsEarned = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeActivities_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EmployeeActivities_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Positions_Code",
                table: "Positions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_EmployeeNumber",
                table: "Employees",
                column: "EmployeeNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Code",
                table: "Departments",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BonusPointAccounts_EmployeeId",
                table: "BonusPointAccounts",
                column: "EmployeeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CampaignParticipants_CampaignId_EmployeeId",
                table: "CampaignParticipants",
                columns: new[] { "CampaignId", "EmployeeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CampaignParticipants_EmployeeId",
                table: "CampaignParticipants",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_Code",
                table: "Campaigns",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeActivities_CampaignId",
                table: "EmployeeActivities",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeActivities_EmployeeId",
                table: "EmployeeActivities",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_RedemptionTransactions_AccountId",
                table: "RedemptionTransactions",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_RedemptionTransactions_RedeemedByEmployeeId",
                table: "RedemptionTransactions",
                column: "RedeemedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferTransactions_FromAccountId",
                table: "TransferTransactions",
                column: "FromAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferTransactions_InitiatedByEmployeeId",
                table: "TransferTransactions",
                column: "InitiatedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferTransactions_ToAccountId",
                table: "TransferTransactions",
                column: "ToAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceRecords_Employees_EmployeeId",
                table: "AttendanceRecords",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BankAccounts_Employees_EmployeeId",
                table: "BankAccounts",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Employees_ManagerId",
                table: "Departments",
                column: "ManagerId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Educations_Employees_EmployeeId",
                table: "Educations",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Departments_DepartmentId",
                table: "Employees",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Employees_ManagerId",
                table: "Employees",
                column: "ManagerId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Positions_PositionId",
                table: "Employees",
                column: "PositionId",
                principalTable: "Positions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Employees_ApproverEmployeeId",
                table: "Requests",
                column: "ApproverEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Employees_RequesterEmployeeId",
                table: "Requests",
                column: "RequesterEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

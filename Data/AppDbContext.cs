using Microsoft.EntityFrameworkCore;
using EmployeeApi.Models;
using EmployeeApi.Models.Enums;

namespace EmployeeApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<Education> Educations => Set<Education>();
    public DbSet<Request> Requests => Set<Request>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<TimesheetTask> TimesheetTasks => Set<TimesheetTask>();
    public DbSet<TimesheetEntry> TimesheetEntries => Set<TimesheetEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Set default schema to 'dotnet'
        modelBuilder.HasDefaultSchema("dotnet");

        // Entities in 'dotnet' schema (managed by this service)
        modelBuilder.Entity<Employee>().ToTable("employee");
        modelBuilder.Entity<BankAccount>().ToTable("bank_account");
        modelBuilder.Entity<Education>().ToTable("education");
        modelBuilder.Entity<Request>().ToTable("request");
        modelBuilder.Entity<AttendanceRecord>().ToTable("attendance_record");
        modelBuilder.Entity<Position>().ToTable("position");
        modelBuilder.Entity<Department>().ToTable("department");
        modelBuilder.Entity<TimesheetTask>().ToTable("timesheet_task");
        modelBuilder.Entity<TimesheetEntry>().ToTable("timesheet_entry");

        // Relationships per external schema
        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Position)
            .WithMany()
            .HasForeignKey(e => e.PositionId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Department)
            .WithMany()
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Manager)
            .WithMany()
            .HasForeignKey(e => e.ManagerId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Department>()
            .HasOne(d => d.Manager)
            .WithMany()
            .HasForeignKey(d => d.ManagerId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<BankAccount>()
            .HasOne(b => b.Employee)
            .WithMany()
            .HasForeignKey(b => b.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Education>()
            .HasOne(ed => ed.Employee)
            .WithMany()
            .HasForeignKey(ed => ed.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure enum to string conversions
        modelBuilder.Entity<Request>()
            .Property(r => r.RequestType)
            .HasConversion<string>();

        modelBuilder.Entity<Request>()
            .Property(r => r.Status)
            .HasConversion<string>();

        // Configure Request entity
        modelBuilder.Entity<Request>()
            .HasOne(r => r.Requester)
            .WithMany()
            .HasForeignKey(r => r.RequesterEmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Request>()
            .HasOne(r => r.Approver)
            .WithMany()
            .HasForeignKey(r => r.ApproverEmployeeId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure AttendanceRecord entity
        modelBuilder.Entity<AttendanceRecord>()
            .HasOne(a => a.Employee)
            .WithMany()
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Add indexes for better query performance
        modelBuilder.Entity<Request>()
            .HasIndex(r => r.Status);

        modelBuilder.Entity<Request>()
            .HasIndex(r => r.RequesterEmployeeId);

        modelBuilder.Entity<AttendanceRecord>()
            .HasIndex(a => a.EmployeeId);

        modelBuilder.Entity<AttendanceRecord>()
            .HasIndex(a => a.Date);

        // Unique indexes per external schema
        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.Email)
            .IsUnique();

        modelBuilder.Entity<BankAccount>()
            .HasIndex(b => b.AccountNumber)
            .IsUnique();

        // ========================================
        // TimesheetTask Configuration
        // ========================================
        modelBuilder.Entity<TimesheetTask>()
            .HasIndex(t => t.TaskCode)
            .IsUnique();

        modelBuilder.Entity<TimesheetTask>()
            .HasIndex(t => t.IsActive);

        // ========================================
        // TimesheetEntry Configuration
        // ========================================
        modelBuilder.Entity<TimesheetEntry>()
            .HasOne(te => te.Request)
            .WithMany(r => r.TimesheetEntries)
            .HasForeignKey(te => te.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TimesheetEntry>()
            .HasOne(te => te.Employee)
            .WithMany()
            .HasForeignKey(te => te.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TimesheetEntry>()
            .HasOne(te => te.Task)
            .WithMany(t => t.TimesheetEntries)
            .HasForeignKey(te => te.TaskId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes for better query performance
        modelBuilder.Entity<TimesheetEntry>()
            .HasIndex(te => te.RequestId);

        modelBuilder.Entity<TimesheetEntry>()
            .HasIndex(te => te.EmployeeId);

        modelBuilder.Entity<TimesheetEntry>()
            .HasIndex(te => new { te.WeekStartDate, te.WeekEndDate });

        // Unique constraint: one entry per employee/task/week
        modelBuilder.Entity<TimesheetEntry>()
            .HasIndex(te => new { te.EmployeeId, te.TaskId, te.WeekStartDate })
            .IsUnique();

        // Check constraints
        modelBuilder.Entity<TimesheetEntry>()
            .ToTable(t => t.HasCheckConstraint("chk_hours", "hours >= 0 AND hours <= 168"));

        modelBuilder.Entity<TimesheetEntry>()
            .ToTable(t => t.HasCheckConstraint("chk_entry_type", "entry_type IN ('project', 'leave')"));

        modelBuilder.Entity<TimesheetTask>()
            .ToTable(t => t.HasCheckConstraint("chk_task_type", "task_type IN ('project', 'leave')"));
    }
}

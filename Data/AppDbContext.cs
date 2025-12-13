using Microsoft.EntityFrameworkCore;
using EmployeeApi.Models;
using EmployeeApi.Models.Enums;

namespace EmployeeApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Request> Requests => Set<Request>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Request>()
            .HasOne(r => r.Approver)
            .WithMany()
            .HasForeignKey(r => r.ApproverEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

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
    }
}

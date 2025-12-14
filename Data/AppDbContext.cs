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
    public DbSet<BonusPointAccount> BonusPointAccounts => Set<BonusPointAccount>();
    public DbSet<TransferTransaction> TransferTransactions => Set<TransferTransaction>();
    public DbSet<RedemptionTransaction> RedemptionTransactions => Set<RedemptionTransaction>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<CampaignParticipant> CampaignParticipants => Set<CampaignParticipant>();
    public DbSet<EmployeeActivity> EmployeeActivities => Set<EmployeeActivity>();
    public DbSet<Request> Requests => Set<Request>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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

        modelBuilder.Entity<BonusPointAccount>()
            .HasOne(bp => bp.Employee)
            .WithMany()
            .HasForeignKey(bp => bp.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TransferTransaction>()
            .HasOne(t => t.FromAccount)
            .WithMany()
            .HasForeignKey(t => t.FromAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TransferTransaction>()
            .HasOne(t => t.ToAccount)
            .WithMany()
            .HasForeignKey(t => t.ToAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TransferTransaction>()
            .HasOne(t => t.InitiatedByEmployee)
            .WithMany()
            .HasForeignKey(t => t.InitiatedByEmployeeId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<RedemptionTransaction>()
            .HasOne(r => r.Account)
            .WithMany()
            .HasForeignKey(r => r.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RedemptionTransaction>()
            .HasOne(r => r.RedeemedByEmployee)
            .WithMany()
            .HasForeignKey(r => r.RedeemedByEmployeeId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<CampaignParticipant>()
            .HasOne(cp => cp.Campaign)
            .WithMany()
            .HasForeignKey(cp => cp.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CampaignParticipant>()
            .HasOne(cp => cp.Employee)
            .WithMany()
            .HasForeignKey(cp => cp.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EmployeeActivity>()
            .HasOne(a => a.Employee)
            .WithMany()
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EmployeeActivity>()
            .HasOne(a => a.Campaign)
            .WithMany()
            .HasForeignKey(a => a.CampaignId)
            .OnDelete(DeleteBehavior.SetNull);

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
        modelBuilder.Entity<Position>()
            .HasIndex(p => p.Code)
            .IsUnique();

        modelBuilder.Entity<Department>()
            .HasIndex(d => d.Code)
            .IsUnique();

        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.EmployeeNumber)
            .IsUnique();

        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.Email)
            .IsUnique();

        modelBuilder.Entity<BankAccount>()
            .HasIndex(b => b.AccountNumber)
            .IsUnique();

        modelBuilder.Entity<BonusPointAccount>()
            .HasIndex(bp => bp.EmployeeId)
            .IsUnique();

        modelBuilder.Entity<Campaign>()
            .HasIndex(c => c.Code)
            .IsUnique();

        modelBuilder.Entity<CampaignParticipant>()
            .HasIndex(cp => new { cp.CampaignId, cp.EmployeeId })
            .IsUnique();
    }
}

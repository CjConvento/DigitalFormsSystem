using System;
using System.Collections.Generic;
using DigitalFormsSystem.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalFormsSystem.Core.Models;

public partial class DigitalFormsSystemContext : DbContext
{
    public DigitalFormsSystemContext()
    {
    }

    public DigitalFormsSystemContext(DbContextOptions<DigitalFormsSystemContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AssetType> AssetTypes { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<DamagedReport> DamagedReports { get; set; }
    public virtual DbSet<DamagedReportFollowUp> DamagedReportFollowUps { get; set; }

    public virtual DbSet<DamagedReportImage> DamagedReportImages { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<ExistingUnitDetail> ExistingUnitDetails { get; set; }

    public virtual DbSet<FixedAssetPrintLog> FixedAssetPrintLogs { get; set; }

    public virtual DbSet<FixedAssetRequest> FixedAssetRequests { get; set; }

    public virtual DbSet<FixedAssetRequestApproval> FixedAssetRequestApprovals { get; set; }

    public virtual DbSet<FixedAssetTransferHistory> FixedAssetTransferHistories { get; set; }

    public virtual DbSet<MemorandumReceipt> MemorandumReceipts { get; set; }

    public virtual DbSet<RequestStatusHistory> RequestStatusHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ============================================================
        // PostgreSQL DateTime convention
        // Forces ALL DateTime properties to 'timestamp without time zone'
        // so that DateTime.Now (Kind = Local) works with Npgsql 8+
        // ============================================================
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                var clrType = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;
                if (clrType == typeof(DateTime))
                {
                    property.SetColumnType("timestamp without time zone");
                }
            }
        }

        // ============================================================
        // AssetTypes
        // ============================================================
        modelBuilder.Entity<AssetType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AssetTypeName, "UQ_AssetTypes_AssetTypeName").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.AssetTypeName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        // ============================================================
        // Employees
        // ============================================================
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Department, "IX_Employees_Department");
            entity.HasIndex(e => e.EmployeeNo, "IX_Employees_EmployeeNo");
            entity.HasIndex(e => e.EmployeeNo, "UQ_Employees_EmployeeNo").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Category).HasMaxLength(10);
            entity.Property(e => e.Company).HasMaxLength(10);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Department).HasMaxLength(100);
            entity.Property(e => e.EmployeeNo).HasMaxLength(20);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Location).HasMaxLength(5);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Section).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(5);
        });

        // ============================================================
        // ExistingUnitDetails
        // ============================================================
        modelBuilder.Entity<ExistingUnitDetail>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.FixedAssetRequestId).HasColumnName("FixedAssetRequestID");
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.UserName).HasMaxLength(200);

            entity.HasOne(d => d.FixedAssetRequest).WithMany(p => p.ExistingUnitDetails)
                .HasForeignKey(d => d.FixedAssetRequestId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<FixedAssetPrintLog>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.FixedAssetRequestId).HasColumnName("FixedAssetRequestID");
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(50)
                .HasColumnName("IPAddress");
            entity.Property(e => e.PrintDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.PrintFormat)
                .HasMaxLength(50)
                .HasDefaultValue("Full Form");
            entity.Property(e => e.PrintedByEmployeeId).HasColumnName("PrintedByEmployeeID");
            entity.Property(e => e.Remarks).HasMaxLength(200);
            entity.Property(e => e.UserAgent).HasMaxLength(500);

            entity.HasOne(d => d.FixedAssetRequest).WithMany(p => p.FixedAssetPrintLogs)
                .HasForeignKey(d => d.FixedAssetRequestId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(d => d.PrintedByEmployee).WithMany(p => p.FixedAssetPrintLogs)
                .HasForeignKey(d => d.PrintedByEmployeeId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // ============================================================
        // FixedAssetRequests
        // ============================================================
        modelBuilder.Entity<FixedAssetRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ControlNo, "IX_FixedAssetRequests_ControlNo");
            entity.HasIndex(e => e.DateRequested, "IX_FixedAssetRequests_DateRequested");
            entity.HasIndex(e => e.Department, "IX_FixedAssetRequests_Department");
            entity.HasIndex(e => e.RequestStatus, "IX_FixedAssetRequests_RequestStatus");
            entity.HasIndex(e => e.ControlNo, "UQ_FixedAssetRequests_ControlNo").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.AssetType).HasMaxLength(100);
            entity.Property(e => e.AssetTypeId).HasColumnName("AssetTypeID");
            entity.Property(e => e.ControlNo).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.DamagedReportNo).HasMaxLength(50);
            entity.Property(e => e.Department).HasMaxLength(100);
            entity.Property(e => e.EstimatedLifeSpan).HasMaxLength(100);
            entity.Property(e => e.EvaluatedByEmployeeId).HasColumnName("EvaluatedByEmployeeID");
            entity.Property(e => e.EvaluatedByName).HasMaxLength(200);
            entity.Property(e => e.ExistingUser).HasMaxLength(200);
            entity.Property(e => e.ProposedLocation).HasMaxLength(200);
            entity.Property(e => e.RequestStatus)
                .HasMaxLength(30)
                .HasDefaultValue("Draft");
            entity.Property(e => e.RequestType).HasMaxLength(20);
            entity.Property(e => e.RequestedByEmployeeId).HasColumnName("RequestedByEmployeeID");
            entity.Property(e => e.RequestedByName).HasMaxLength(200);
            entity.Property(e => e.Section).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.AssetTypeNavigation).WithMany(p => p.FixedAssetRequests)
                .HasForeignKey(d => d.AssetTypeId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(d => d.EvaluatedByEmployee).WithMany(p => p.FixedAssetRequestEvaluatedByEmployees)
                .HasForeignKey(d => d.EvaluatedByEmployeeId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(d => d.RequestedByEmployee).WithMany(p => p.FixedAssetRequestRequestedByEmployees)
                .HasForeignKey(d => d.RequestedByEmployeeId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // ============================================================
        // FixedAssetRequestApprovals
        // ============================================================
        modelBuilder.Entity<FixedAssetRequestApproval>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.FixedAssetRequestId, "IX_FixedAssetRequestApprovals_FixedAssetRequestID");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ExecutiveEvaluatedByEmployeeId).HasColumnName("ExecutiveEvaluatedByEmployeeID");
            entity.Property(e => e.FinanceProcessedByEmployeeId).HasColumnName("FinanceProcessedByEmployeeID");
            entity.Property(e => e.FixedAssetCode).HasMaxLength(50);
            entity.Property(e => e.FixedAssetRequestId).HasColumnName("FixedAssetRequestID");
            entity.Property(e => e.PresidentApprovedByEmployeeId).HasColumnName("PresidentApprovedByEmployeeID");
            entity.Property(e => e.Quotation1Amount).HasPrecision(18, 2);
            entity.Property(e => e.Quotation1Reference).HasMaxLength(200);
            entity.Property(e => e.Quotation2Amount).HasPrecision(18, 2);
            entity.Property(e => e.Quotation2Reference).HasMaxLength(200);
            entity.Property(e => e.ReceivedByEmployeeId).HasColumnName("ReceivedByEmployeeID");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Vpapproved).HasColumnName("VPApproved");
            entity.Property(e => e.VpapprovedAt).HasColumnName("VPApprovedAt");
            entity.Property(e => e.VpapprovedByEmployeeId).HasColumnName("VPApprovedByEmployeeID");
            entity.Property(e => e.Vpremarks).HasColumnName("VPRemarks");

            entity.HasOne(d => d.ExecutiveEvaluatedByEmployee)
                .WithMany(p => p.FixedAssetRequestApprovalExecutiveEvaluatedByEmployees)
                .HasForeignKey(d => d.ExecutiveEvaluatedByEmployeeId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(d => d.FinanceProcessedByEmployee)
                .WithMany(p => p.FixedAssetRequestApprovalFinanceProcessedByEmployees)
                .HasForeignKey(d => d.FinanceProcessedByEmployeeId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(d => d.FixedAssetRequest)
                .WithMany(p => p.FixedAssetRequestApprovals)
                .HasForeignKey(d => d.FixedAssetRequestId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(d => d.PresidentApprovedByEmployee)
                .WithMany(p => p.FixedAssetRequestApprovalPresidentApprovedByEmployees)
                .HasForeignKey(d => d.PresidentApprovedByEmployeeId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(d => d.ReceivedByEmployee)
                .WithMany(p => p.FixedAssetRequestApprovalReceivedByEmployees)
                .HasForeignKey(d => d.ReceivedByEmployeeId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(d => d.VpapprovedByEmployee)
                .WithMany(p => p.FixedAssetRequestApprovalVpapprovedByEmployees)
                .HasForeignKey(d => d.VpapprovedByEmployeeId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // ============================================================
        // FixedAssetTransferHistory
        // ============================================================
        modelBuilder.Entity<FixedAssetTransferHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("FixedAssetTransferHistory");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.FixedAssetCode).HasMaxLength(50);
            entity.Property(e => e.FromDepartment).HasMaxLength(100);
            entity.Property(e => e.FromEmployeeId).HasColumnName("FromEmployeeID");
            entity.Property(e => e.FromSection).HasMaxLength(100);
            entity.Property(e => e.ProcessedByEmployeeId).HasColumnName("ProcessedByEmployeeID");
            entity.Property(e => e.ToDepartment).HasMaxLength(100);
            entity.Property(e => e.ToEmployeeId).HasColumnName("ToEmployeeID");
            entity.Property(e => e.ToSection).HasMaxLength(100);

            entity.HasOne(d => d.FromEmployee)
                .WithMany(p => p.FixedAssetTransferHistoryFromEmployees)
                .HasForeignKey(d => d.FromEmployeeId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(d => d.ProcessedByEmployee)
                .WithMany(p => p.FixedAssetTransferHistoryProcessedByEmployees)
                .HasForeignKey(d => d.ProcessedByEmployeeId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(d => d.ToEmployee)
                .WithMany(p => p.FixedAssetTransferHistoryToEmployees)
                .HasForeignKey(d => d.ToEmployeeId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // ============================================================
        // MemorandumReceipt
        // ============================================================
        modelBuilder.Entity<MemorandumReceipt>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("MemorandumReceipt");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Brand).HasMaxLength(100);
            entity.Property(e => e.Ccfinance)
                .HasDefaultValue(false)
                .HasColumnName("CCFinance");
            entity.Property(e => e.Ccpurchasing)
                .HasDefaultValue(false)
                .HasColumnName("CCPurchasing");
            entity.Property(e => e.CcrequestingDept)
                .HasDefaultValue(false)
                .HasColumnName("CCRequestingDept");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Department).HasMaxLength(100);
            entity.Property(e => e.FixedAssetCode).HasMaxLength(50);
            entity.Property(e => e.FixedAssetRequestId).HasColumnName("FixedAssetRequestID");
            entity.Property(e => e.Manufacturer).HasMaxLength(200);
            entity.Property(e => e.ModelNumber).HasMaxLength(100);
            entity.Property(e => e.Podate).HasColumnName("PODate");
            entity.Property(e => e.Ponumber)
                .HasMaxLength(50)
                .HasColumnName("PONumber");
            entity.Property(e => e.ReceivedByEmployeeId).HasColumnName("ReceivedByEmployeeID");
            entity.Property(e => e.ReceivedByName).HasMaxLength(200);
            entity.Property(e => e.ReceivedSignature).HasMaxLength(100);
            entity.Property(e => e.ReleasedByEmployeeId).HasColumnName("ReleasedByEmployeeID");
            entity.Property(e => e.ReleasedByName).HasMaxLength(200);
            entity.Property(e => e.ReleasedSignature).HasMaxLength(100);
            entity.Property(e => e.Section).HasMaxLength(100);
            entity.Property(e => e.SerialNumber).HasMaxLength(100);
            entity.Property(e => e.TransactionType).HasMaxLength(20);

            entity.HasOne(d => d.FixedAssetRequest).WithMany(p => p.MemorandumReceipts)
                .HasForeignKey(d => d.FixedAssetRequestId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(d => d.ReceivedByEmployee).WithMany(p => p.MemorandumReceiptReceivedByEmployees)
                .HasForeignKey(d => d.ReceivedByEmployeeId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(d => d.ReleasedByEmployee).WithMany(p => p.MemorandumReceiptReleasedByEmployees)
                .HasForeignKey(d => d.ReleasedByEmployeeId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<RequestStatusHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("RequestStatusHistory");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ChangedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ChangedByEmployeeId).HasColumnName("ChangedByEmployeeID");
            entity.Property(e => e.FixedAssetRequestId).HasColumnName("FixedAssetRequestID");
            entity.Property(e => e.NewStatus).HasMaxLength(30);
            entity.Property(e => e.OldStatus).HasMaxLength(30);

            entity.HasOne(d => d.ChangedByEmployee).WithMany(p => p.RequestStatusHistories)
                .HasForeignKey(d => d.ChangedByEmployeeId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(d => d.FixedAssetRequest).WithMany(p => p.RequestStatusHistories)
                .HasForeignKey(d => d.FixedAssetRequestId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

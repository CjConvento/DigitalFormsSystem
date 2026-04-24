using System;
using System.Collections.Generic;
using DigitalFormsSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalFormsSystem.Models;

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

    public virtual DbSet<DamagedReport> DamagedReports { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<ExistingUnitDetail> ExistingUnitDetails { get; set; }

    public virtual DbSet<FixedAssetPrintLog> FixedAssetPrintLogs { get; set; }

    public virtual DbSet<FixedAssetRequest> FixedAssetRequests { get; set; }

    public virtual DbSet<FixedAssetRequestApproval> FixedAssetRequestApprovals { get; set; }

    public virtual DbSet<FixedAssetTransferHistory> FixedAssetTransferHistories { get; set; }

    public virtual DbSet<MemorandumReceipt> MemorandumReceipts { get; set; }

    public virtual DbSet<RequestStatusHistory> RequestStatusHistories { get; set; }

    public virtual DbSet<VwFixedAssetRequestCompletePrint> VwFixedAssetRequestCompletePrints { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AssetType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AssetTyp__3214EC27F8985D91");

            entity.HasIndex(e => e.AssetTypeName, "UQ__AssetTyp__6824772C5E2EF8A2").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.AssetTypeName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Employee__3214EC270C6F6526");

            entity.HasIndex(e => e.Department, "IX_Employees_Department");

            entity.HasIndex(e => e.EmployeeNo, "IX_Employees_EmployeeNo");

            entity.HasIndex(e => e.EmployeeNo, "UQ__Employee__7AD0F1B749DF82FF").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Category)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Company)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Department)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.EmployeeNo)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Location)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Section)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(5)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ExistingUnitDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Existing__3214EC27CE005FA4");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.FixedAssetRequestId).HasColumnName("FixedAssetRequestID");
            entity.Property(e => e.Location)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Remarks)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.UserName)
                .HasMaxLength(200)
                .IsUnicode(false);

            entity.HasOne(d => d.FixedAssetRequest).WithMany(p => p.ExistingUnitDetails)
                .HasForeignKey(d => d.FixedAssetRequestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ExistingU__Fixed__5535A963");
        });

        modelBuilder.Entity<FixedAssetPrintLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__FixedAss__3214EC27840E0AAA");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.FixedAssetRequestId).HasColumnName("FixedAssetRequestID");
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("IPAddress");
            entity.Property(e => e.PrintDateTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PrintFormat)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("Full Form");
            entity.Property(e => e.PrintedByEmployeeId).HasColumnName("PrintedByEmployeeID");
            entity.Property(e => e.Remarks)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.UserAgent)
                .HasMaxLength(500)
                .IsUnicode(false);

            entity.HasOne(d => d.FixedAssetRequest).WithMany(p => p.FixedAssetPrintLogs)
                .HasForeignKey(d => d.FixedAssetRequestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FixedAsse__Fixed__628FA481");

            entity.HasOne(d => d.PrintedByEmployee).WithMany(p => p.FixedAssetPrintLogs)
                .HasForeignKey(d => d.PrintedByEmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FixedAsse__Print__6383C8BA");
        });

        modelBuilder.Entity<FixedAssetRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__FixedAss__3214EC27F65086B2");

            entity.HasIndex(e => e.ControlNo, "IX_FixedAssetRequests_ControlNo");

            entity.HasIndex(e => e.DateRequested, "IX_FixedAssetRequests_DateRequested");

            entity.HasIndex(e => e.Department, "IX_FixedAssetRequests_Department");

            entity.HasIndex(e => e.RequestStatus, "IX_FixedAssetRequests_RequestStatus");

            entity.HasIndex(e => e.ControlNo, "UQ__FixedAss__091DC38FA5A637C2").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.AssetType)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.AssetTypeId).HasColumnName("AssetTypeID");
            entity.Property(e => e.ControlNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DamagedReportNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Department)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.DetailedDescription).HasColumnType("text");
            entity.Property(e => e.EstimatedLifeSpan)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.EvaluatedAt).HasColumnType("datetime");
            entity.Property(e => e.EvaluatedByEmployeeId).HasColumnName("EvaluatedByEmployeeID");
            entity.Property(e => e.EvaluatedByName)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.ExistingUser)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.ProposedLocation)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.ReasonPurpose).HasColumnType("text");
            entity.Property(e => e.RequestStatus)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("Draft");
            entity.Property(e => e.RequestType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.RequestedAt).HasColumnType("datetime");
            entity.Property(e => e.RequestedByEmployeeId).HasColumnName("RequestedByEmployeeID");
            entity.Property(e => e.RequestedByName)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Section)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.AssetTypeNavigation).WithMany(p => p.FixedAssetRequests)
                .HasForeignKey(d => d.AssetTypeId)
                .HasConstraintName("FK__FixedAsse__Asset__4316F928");

            entity.HasOne(d => d.EvaluatedByEmployee).WithMany(p => p.FixedAssetRequestEvaluatedByEmployees)
                .HasForeignKey(d => d.EvaluatedByEmployeeId)
                .HasConstraintName("FK__FixedAsse__Evalu__45F365D3");

            entity.HasOne(d => d.RequestedByEmployee).WithMany(p => p.FixedAssetRequestRequestedByEmployees)
                .HasForeignKey(d => d.RequestedByEmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FixedAsse__Reque__44FF419A");
        });

        modelBuilder.Entity<FixedAssetRequestApproval>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__FixedAss__3214EC2767ABA27B");

            entity.HasIndex(e => e.FixedAssetRequestId, "IX_FixedAssetRequestApprovals_FixedAssetRequestID");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ExecutiveEvaluatedAt).HasColumnType("datetime");
            entity.Property(e => e.ExecutiveEvaluatedByEmployeeId).HasColumnName("ExecutiveEvaluatedByEmployeeID");
            entity.Property(e => e.ExecutiveRemarks).HasColumnType("text");
            entity.Property(e => e.FinanceProcessedAt).HasColumnType("datetime");
            entity.Property(e => e.FinanceProcessedByEmployeeId).HasColumnName("FinanceProcessedByEmployeeID");
            entity.Property(e => e.FinanceRemarks).HasColumnType("text");
            entity.Property(e => e.FixedAssetCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FixedAssetRequestId).HasColumnName("FixedAssetRequestID");
            entity.Property(e => e.PresidentApprovedAt).HasColumnType("datetime");
            entity.Property(e => e.PresidentApprovedByEmployeeId).HasColumnName("PresidentApprovedByEmployeeID");
            entity.Property(e => e.PresidentRemarks).HasColumnType("text");
            entity.Property(e => e.Quotation1Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Quotation1Reference)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Quotation2Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Quotation2Reference)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.ReceivedByEmployeeId).HasColumnName("ReceivedByEmployeeID");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Vpapproved).HasColumnName("VPApproved");
            entity.Property(e => e.VpapprovedAt)
                .HasColumnType("datetime")
                .HasColumnName("VPApprovedAt");
            entity.Property(e => e.VpapprovedByEmployeeId).HasColumnName("VPApprovedByEmployeeID");
            entity.Property(e => e.Vpremarks)
                .HasColumnType("text")
                .HasColumnName("VPRemarks");

            entity.HasOne(d => d.ExecutiveEvaluatedByEmployee).WithMany(p => p.FixedAssetRequestApprovalExecutiveEvaluatedByEmployees)
                .HasForeignKey(d => d.ExecutiveEvaluatedByEmployeeId)
                .HasConstraintName("FK__FixedAsse__Execu__4D94879B");

            entity.HasOne(d => d.FinanceProcessedByEmployee).WithMany(p => p.FixedAssetRequestApprovalFinanceProcessedByEmployees)
                .HasForeignKey(d => d.FinanceProcessedByEmployeeId)
                .HasConstraintName("FK__FixedAsse__Finan__5070F446");

            entity.HasOne(d => d.FixedAssetRequest).WithMany(p => p.FixedAssetRequestApprovals)
                .HasForeignKey(d => d.FixedAssetRequestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FixedAsse__Fixed__4BAC3F29");

            entity.HasOne(d => d.PresidentApprovedByEmployee).WithMany(p => p.FixedAssetRequestApprovalPresidentApprovedByEmployees)
                .HasForeignKey(d => d.PresidentApprovedByEmployeeId)
                .HasConstraintName("FK__FixedAsse__Presi__4F7CD00D");

            entity.HasOne(d => d.ReceivedByEmployee).WithMany(p => p.FixedAssetRequestApprovalReceivedByEmployees)
                .HasForeignKey(d => d.ReceivedByEmployeeId)
                .HasConstraintName("FK__FixedAsse__Recei__4CA06362");

            entity.HasOne(d => d.VpapprovedByEmployee).WithMany(p => p.FixedAssetRequestApprovalVpapprovedByEmployees)
                .HasForeignKey(d => d.VpapprovedByEmployeeId)
                .HasConstraintName("FK__FixedAsse__VPApp__4E88ABD4");
        });

        modelBuilder.Entity<FixedAssetTransferHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__FixedAss__3214EC27B06C25E6");

            entity.ToTable("FixedAssetTransferHistory");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FixedAssetCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FromDepartment)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FromEmployeeId).HasColumnName("FromEmployeeID");
            entity.Property(e => e.FromSection)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ProcessedByEmployeeId).HasColumnName("ProcessedByEmployeeID");
            entity.Property(e => e.ToDepartment)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ToEmployeeId).HasColumnName("ToEmployeeID");
            entity.Property(e => e.ToSection)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.TransferReason).HasColumnType("text");

            entity.HasOne(d => d.FromEmployee).WithMany(p => p.FixedAssetTransferHistoryFromEmployees)
                .HasForeignKey(d => d.FromEmployeeId)
                .HasConstraintName("FK__FixedAsse__FromE__6D0D32F4");

            entity.HasOne(d => d.ProcessedByEmployee).WithMany(p => p.FixedAssetTransferHistoryProcessedByEmployees)
                .HasForeignKey(d => d.ProcessedByEmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FixedAsse__Proce__6EF57B66");

            entity.HasOne(d => d.ToEmployee).WithMany(p => p.FixedAssetTransferHistoryToEmployees)
                .HasForeignKey(d => d.ToEmployeeId)
                .HasConstraintName("FK__FixedAsse__ToEmp__6E01572D");
        });

        modelBuilder.Entity<MemorandumReceipt>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Memorand__3214EC277729DE94");

            entity.ToTable("MemorandumReceipt");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Brand)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Ccfinance)
                .HasDefaultValue(false)
                .HasColumnName("CCFinance");
            entity.Property(e => e.Ccpurchasing)
                .HasDefaultValue(false)
                .HasColumnName("CCPurchasing");
            entity.Property(e => e.CcrequestingDept)
                .HasDefaultValue(false)
                .HasColumnName("CCRequestingDept");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Department)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FixedAssetCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FixedAssetRequestId).HasColumnName("FixedAssetRequestID");
            entity.Property(e => e.ItemDescription).HasColumnType("text");
            entity.Property(e => e.Manufacturer)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.ModelNumber)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Podate).HasColumnName("PODate");
            entity.Property(e => e.Ponumber)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("PONumber");
            entity.Property(e => e.ReceivedByEmployeeId).HasColumnName("ReceivedByEmployeeID");
            entity.Property(e => e.ReceivedByName)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.ReceivedSignature)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ReleasedByEmployeeId).HasColumnName("ReleasedByEmployeeID");
            entity.Property(e => e.ReleasedByName)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.ReleasedSignature)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Section)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.SerialNumber)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.TransactionType)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.FixedAssetRequest).WithMany(p => p.MemorandumReceipts)
                .HasForeignKey(d => d.FixedAssetRequestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Memorandu__Fixed__59063A47");

            entity.HasOne(d => d.ReceivedByEmployee).WithMany(p => p.MemorandumReceiptReceivedByEmployees)
                .HasForeignKey(d => d.ReceivedByEmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Memorandu__Recei__5AEE82B9");

            entity.HasOne(d => d.ReleasedByEmployee).WithMany(p => p.MemorandumReceiptReleasedByEmployees)
                .HasForeignKey(d => d.ReleasedByEmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Memorandu__Relea__5BE2A6F2");
        });

        modelBuilder.Entity<RequestStatusHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RequestS__3214EC27B134BA8C");

            entity.ToTable("RequestStatusHistory");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ChangedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ChangedByEmployeeId).HasColumnName("ChangedByEmployeeID");
            entity.Property(e => e.FixedAssetRequestId).HasColumnName("FixedAssetRequestID");
            entity.Property(e => e.NewStatus)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.OldStatus)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.Remarks).HasColumnType("text");

            entity.HasOne(d => d.ChangedByEmployee).WithMany(p => p.RequestStatusHistories)
                .HasForeignKey(d => d.ChangedByEmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RequestSt__Chang__693CA210");

            entity.HasOne(d => d.FixedAssetRequest).WithMany(p => p.RequestStatusHistories)
                .HasForeignKey(d => d.FixedAssetRequestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RequestSt__Fixed__68487DD7");
        });

        modelBuilder.Entity<VwFixedAssetRequestCompletePrint>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_FixedAssetRequest_CompletePrint");

            entity.Property(e => e.AssetClassification)
                .HasMaxLength(7)
                .IsUnicode(false);
            entity.Property(e => e.AssetType)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Brand)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ControlNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.DamagedReportNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Department)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.DetailedDescription).HasColumnType("text");
            entity.Property(e => e.EstimatedLifeSpan)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.EvaluatedAt).HasColumnType("datetime");
            entity.Property(e => e.EvaluatedByName)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.ExecutiveEvaluatedAt).HasColumnType("datetime");
            entity.Property(e => e.ExecutiveEvaluatedByName)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.ExecutiveRecommendation)
                .HasMaxLength(19)
                .IsUnicode(false);
            entity.Property(e => e.ExecutiveRemarks).HasColumnType("text");
            entity.Property(e => e.ExistingUser)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.FinanceProcessedAt).HasColumnType("datetime");
            entity.Property(e => e.FinanceProcessedByName)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.FinanceRemarks).HasColumnType("text");
            entity.Property(e => e.FixedAssetCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Manufacturer)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.MemorandumItemDescription).HasColumnType("text");
            entity.Property(e => e.MemorandumReceivedByName)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.ModelNumber)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Podate).HasColumnName("PODate");
            entity.Property(e => e.Ponumber)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("PONumber");
            entity.Property(e => e.PresidentApprovalStatus)
                .HasMaxLength(11)
                .IsUnicode(false);
            entity.Property(e => e.PresidentApprovedAt).HasColumnType("datetime");
            entity.Property(e => e.PresidentApprovedByName)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.PresidentRemarks).HasColumnType("text");
            entity.Property(e => e.ProposedLocation)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Quotation1Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Quotation1Reference)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Quotation2Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Quotation2Reference)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.ReasonPurpose).HasColumnType("text");
            entity.Property(e => e.ReceivedByName)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.ReleasedByName)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.RequestStatus)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.RequestType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.RequestedAt).HasColumnType("datetime");
            entity.Property(e => e.RequestedByName)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Section)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.SerialNumber)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.TransactionType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.VpapprovalStatus)
                .HasMaxLength(11)
                .IsUnicode(false)
                .HasColumnName("VPApprovalStatus");
            entity.Property(e => e.VpapprovedAt)
                .HasColumnType("datetime")
                .HasColumnName("VPApprovedAt");
            entity.Property(e => e.VpapprovedByName)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("VPApprovedByName");
            entity.Property(e => e.Vpremarks)
                .HasColumnType("text")
                .HasColumnName("VPRemarks");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

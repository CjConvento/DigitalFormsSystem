using System;
using System.Collections.Generic;

namespace DigitalFormsSystem.Models;

public partial class VwFixedAssetRequestCompletePrint
{
    public int Id { get; set; }

    public string? ControlNo { get; set; }

    public DateOnly DateRequested { get; set; }

    public string Department { get; set; } = null!;

    public DateOnly TargetDateNeeded { get; set; }

    public string Section { get; set; } = null!;

    public int Quantity { get; set; }

    public string? AssetType { get; set; }

    public string DetailedDescription { get; set; } = null!;

    public string ReasonPurpose { get; set; } = null!;

    public string? ProposedLocation { get; set; }

    public string? EstimatedLifeSpan { get; set; }

    public string RequestType { get; set; } = null!;

    public int? ExistingUnitCount { get; set; }

    public string? ExistingUser { get; set; }

    public string? DamagedReportNo { get; set; }

    public string? RequestedByName { get; set; }

    public DateTime? RequestedAt { get; set; }

    public string? EvaluatedByName { get; set; }

    public DateTime? EvaluatedAt { get; set; }

    public string? ReceivedByName { get; set; }

    public DateOnly? ReceivedDate { get; set; }

    public string? Quotation1Reference { get; set; }

    public decimal? Quotation1Amount { get; set; }

    public string? Quotation2Reference { get; set; }

    public decimal? Quotation2Amount { get; set; }

    public string? ExecutiveRemarks { get; set; }

    public string ExecutiveRecommendation { get; set; } = null!;

    public string? ExecutiveEvaluatedByName { get; set; }

    public DateTime? ExecutiveEvaluatedAt { get; set; }

    public string VpapprovalStatus { get; set; } = null!;

    public string? Vpremarks { get; set; }

    public string? VpapprovedByName { get; set; }

    public DateTime? VpapprovedAt { get; set; }

    public string PresidentApprovalStatus { get; set; } = null!;

    public string? PresidentRemarks { get; set; }

    public string? PresidentApprovedByName { get; set; }

    public DateTime? PresidentApprovedAt { get; set; }

    public string? FixedAssetCode { get; set; }

    public string AssetClassification { get; set; } = null!;

    public int? AmortizationMonths { get; set; }

    public string? FinanceRemarks { get; set; }

    public string? FinanceProcessedByName { get; set; }

    public DateTime? FinanceProcessedAt { get; set; }

    public string? Ponumber { get; set; }

    public DateOnly? Podate { get; set; }

    public string? TransactionType { get; set; }

    public string? Manufacturer { get; set; }

    public string? SerialNumber { get; set; }

    public string? ModelNumber { get; set; }

    public string? Brand { get; set; }

    public string? MemorandumItemDescription { get; set; }

    public string? MemorandumReceivedByName { get; set; }

    public DateOnly? MemorandumReceivedDate { get; set; }

    public string? ReleasedByName { get; set; }

    public DateOnly? ReleasedDate { get; set; }

    public string? RequestStatus { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

using System;
using System.Collections.Generic;

namespace DigitalFormsSystem.Models;

public partial class FixedAssetRequestApproval
{
    public int Id { get; set; }

    public int FixedAssetRequestId { get; set; }

    public int? ReceivedByEmployeeId { get; set; }

    public DateOnly? ReceivedDate { get; set; }

    public string? Quotation1Reference { get; set; }

    public decimal? Quotation1Amount { get; set; }

    public string? Quotation2Reference { get; set; }

    public decimal? Quotation2Amount { get; set; }

    public string? ExecutiveRemarks { get; set; }

    public bool? ExecutiveRecommendingApproval { get; set; }

    public int? ExecutiveEvaluatedByEmployeeId { get; set; }

    public DateTime? ExecutiveEvaluatedAt { get; set; }

    public bool? Vpapproved { get; set; }

    public string? Vpremarks { get; set; }

    public int? VpapprovedByEmployeeId { get; set; }

    public DateTime? VpapprovedAt { get; set; }

    public bool? PresidentApproved { get; set; }

    public string? PresidentRemarks { get; set; }

    public int? PresidentApprovedByEmployeeId { get; set; }

    public DateTime? PresidentApprovedAt { get; set; }

    public string? FixedAssetCode { get; set; }

    public bool? IsCapitalized { get; set; }

    public int? AmortizationMonths { get; set; }

    public string? FinanceRemarks { get; set; }

    public int? FinanceProcessedByEmployeeId { get; set; }

    public DateTime? FinanceProcessedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Employee? ExecutiveEvaluatedByEmployee { get; set; }

    public virtual Employee? FinanceProcessedByEmployee { get; set; }

    public virtual FixedAssetRequest FixedAssetRequest { get; set; } = null!;

    public virtual Employee? PresidentApprovedByEmployee { get; set; }

    public virtual Employee? ReceivedByEmployee { get; set; }

    public virtual Employee? VpapprovedByEmployee { get; set; }
}

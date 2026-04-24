using System;
using System.Collections.Generic;

namespace DigitalFormsSystem.Models;

public partial class DamagedReport
{
    public int Id { get; set; }

    public string ControlNo { get; set; } = null!;

    public string Item { get; set; } = null!;

    public string? FixedAssetCode { get; set; }

    public DateOnly? DatePurchased { get; set; }

    public string? BrandSize { get; set; }

    public string? LocationUser { get; set; }

    public string? SerialNumber { get; set; }

    public string? Color { get; set; }

    public DateTime? IncidentDateTime { get; set; }

    public string? CauseOfDamage { get; set; }

    public string? ImmediateAction { get; set; }

    public int? RecommendedAction { get; set; }

    public int ReportedByEmployeeId { get; set; }

    public int? ReceivedByEmployeeId { get; set; }

    public DateTime? ReceivedDateTime { get; set; }

    public string? Findings { get; set; }

    public string? Recommendation { get; set; }

    public int? NegligenceFlag { get; set; }

    public string? NegligenceDetails { get; set; }

    public string? Remarks { get; set; }

    public bool? AdministrativeDiscipline { get; set; }

    public int? InvestigatedByEmployeeId { get; set; }

    public int? VerifiedByEmployeeId { get; set; }

    public int? NotedByEmployeeId { get; set; }

    public string? RequestStatus { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

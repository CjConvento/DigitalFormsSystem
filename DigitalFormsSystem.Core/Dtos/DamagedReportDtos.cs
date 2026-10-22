using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace DigitalFormsSystem.Core.Dtos;

public class CreateDamagedReportRequest
{
    [Required] public string Item { get; set; } = null!;
    [Required] public string CauseOfDamage { get; set; } = null!;
    public string? FixedAssetCode { get; set; }
    public DateOnly? DatePurchased { get; set; }
    public string? BrandSize { get; set; }
    public string? LocationUser { get; set; }
    public string? SerialNumber { get; set; }
    public string? Color { get; set; }
    public DateTime? IncidentDateTime { get; set; }
    public string? ImmediateAction { get; set; }
    public int? RecommendedAction { get; set; }
    public string? Remarks { get; set; }
    public int? ReceivedByEmployeeId { get; set; }
    public DateTime? ReceivedDateTime { get; set; }

    public List<IFormFile>? PartIImages { get; set; }
    public List<IFormFile>? PartIIImages { get; set; }
}

public class UpdateDamagedReportRequest
{
    [Required] public int Id { get; set; }
    [Required] public string Item { get; set; } = null!;
    [Required] public string CauseOfDamage { get; set; } = null!;
    public string? FixedAssetCode { get; set; }
    public DateOnly? DatePurchased { get; set; }
    public string? BrandSize { get; set; }
    public string? LocationUser { get; set; }
    public string? SerialNumber { get; set; }
    public string? Color { get; set; }
    public DateTime? IncidentDateTime { get; set; }
    public string? ImmediateAction { get; set; }
    public int? RecommendedAction { get; set; }
    public string? Remarks { get; set; }
    public int? ReceivedByEmployeeId { get; set; }
    public DateTime? ReceivedDateTime { get; set; }
    public bool IsGadUser { get; set; } 

    // GAD Admin Fields
    public bool IsGadUpdate { get; set; } // Controller will set this
    public string? Findings { get; set; }
    public string? Recommendation { get; set; }
    public int? NegligenceFlag { get; set; }
    public string? NegligenceDetails { get; set; }
    public bool? AdministrativeDiscipline { get; set; }
    public int? InvestigatedByEmployeeId { get; set; }
    public int? VerifiedByEmployeeId { get; set; }
    public int? NotedByEmployeeId { get; set; }

    public List<IFormFile>? PartIImages { get; set; }
    public List<IFormFile>? PartIIImages { get; set; }
    public List<int>? DeleteImageIds { get; set; }

    // To handle FollowUps in the service
    public List<DamagedReportFollowUpDto>? FollowUps { get; set; }
}

// DTO for FollowUps
public class DamagedReportFollowUpDto
{
    public DateTime FollowUpDate { get; set; }
    public string? Status { get; set; }
    public string? UpdateBy { get; set; }
    public string? NotedBy { get; set; }
}
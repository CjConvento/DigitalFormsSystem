using System.ComponentModel.DataAnnotations;

namespace DigitalFormsSystem.Core.Dtos;

public class CreateFixedAssetRequestDto
{
    public DateOnly DateRequested { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    [Required, MaxLength(100)] public string Department { get; set; } = null!;
    [Required] public DateOnly TargetDateNeeded { get; set; }
    [Required, MaxLength(100)] public string Section { get; set; } = null!;
    [Required] public int Quantity { get; set; }
    public int? AssetTypeId { get; set; }
    public string? AssetType { get; set; }
    [Required, MaxLength(1000)] public string DetailedDescription { get; set; } = null!;
    [Required, MaxLength(1000)] public string ReasonPurpose { get; set; } = null!;
    public string? ProposedLocation { get; set; }
    public string? EstimatedLifeSpan { get; set; }
    [Required, MaxLength(20)] public string RequestType { get; set; } = null!; // e.g., "Additional", "Replacement"
    public int? ExistingUnitCount { get; set; }
    public string? ExistingUser { get; set; }
    public string? DamagedReportNo { get; set; }

    public List<ExistingUnitDetailDto>? ExistingUnits { get; set; }
}

public class UpdateFixedAssetRequestDto
{
    [Required] public int Id { get; set; }
    [Required] public DateOnly DateRequested { get; set; }
    [Required, MaxLength(100)] public string Department { get; set; } = null!;
    [Required] public DateOnly TargetDateNeeded { get; set; }
    [Required, MaxLength(100)] public string Section { get; set; } = null!;
    [Required] public int Quantity { get; set; }
    public int? AssetTypeId { get; set; }
    public string? AssetType { get; set; }
    [Required, MaxLength(1000)] public string DetailedDescription { get; set; } = null!;
    [Required, MaxLength(1000)] public string ReasonPurpose { get; set; } = null!;
    public string? ProposedLocation { get; set; }
    public string? EstimatedLifeSpan { get; set; }
    [Required, MaxLength(20)] public string RequestType { get; set; } = null!;
    public int? ExistingUnitCount { get; set; }
    public string? ExistingUser { get; set; }
    public string? DamagedReportNo { get; set; }

    public List<ExistingUnitDetailDto>? ExistingUnits { get; set; }
}

public class ExistingUnitDetailDto
{
    public int ItemNo { get; set; }
    [Required, MaxLength(500)] public string Description { get; set; } = null!;
    [MaxLength(200)] public string? Location { get; set; }
    [MaxLength(200)] public string? UserName { get; set; }
    [MaxLength(500)] public string? Remarks { get; set; }
}
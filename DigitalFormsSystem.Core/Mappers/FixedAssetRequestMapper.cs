using DigitalFormsSystem.Core.Dtos;
using DigitalFormsSystem.Core.Models;

namespace DigitalFormsSystem.Core.Mappers;

public static class FixedAssetRequestMapper
{
    // 1. Form Model → Create DTO
    public static CreateFixedAssetRequestDto ToCreateDto(this FixedAssetRequest formModel, List<ExistingUnitDetailDto>? existingUnits)
    {
        return new CreateFixedAssetRequestDto
        {
            DateRequested = formModel.DateRequested,
            Department = formModel.Department,
            TargetDateNeeded = formModel.TargetDateNeeded,
            Section = formModel.Section,
            Quantity = formModel.Quantity,
            AssetTypeId = formModel.AssetTypeId,
            AssetType = formModel.AssetType,
            DetailedDescription = formModel.DetailedDescription,
            ReasonPurpose = formModel.ReasonPurpose,
            ProposedLocation = formModel.ProposedLocation,
            EstimatedLifeSpan = formModel.EstimatedLifeSpan,
            RequestType = formModel.RequestType,
            ExistingUnitCount = formModel.ExistingUnitCount,
            ExistingUser = formModel.ExistingUser,
            DamagedReportNo = formModel.DamagedReportNo,
            ExistingUnits = existingUnits
        };
    }

    // 2. Form Model → Update DTO
    public static UpdateFixedAssetRequestDto ToUpdateDto(this FixedAssetRequest formModel, List<ExistingUnitDetailDto>? existingUnits)
    {
        return new UpdateFixedAssetRequestDto
        {
            Id = formModel.Id,
            DateRequested = formModel.DateRequested,
            Department = formModel.Department,
            TargetDateNeeded = formModel.TargetDateNeeded,
            Section = formModel.Section,
            Quantity = formModel.Quantity,
            AssetTypeId = formModel.AssetTypeId,
            AssetType = formModel.AssetType,
            DetailedDescription = formModel.DetailedDescription,
            ReasonPurpose = formModel.ReasonPurpose,
            ProposedLocation = formModel.ProposedLocation,
            EstimatedLifeSpan = formModel.EstimatedLifeSpan,
            RequestType = formModel.RequestType,
            ExistingUnitCount = formModel.ExistingUnitCount,
            ExistingUser = formModel.ExistingUser,
            DamagedReportNo = formModel.DamagedReportNo,
            EvaluatedByName = formModel.EvaluatedByName, 
            RequestedByName = formModel.RequestedByName, 
            ExistingUnits = existingUnits
        };
    }

    // 3. Create DTO → Entity
    public static FixedAssetRequest ToEntity(this CreateFixedAssetRequestDto request, int employeeId, string controlNo)
    {
        return new FixedAssetRequest
        {
            ControlNo = controlNo,
            DateRequested = request.DateRequested == default ? DateOnly.FromDateTime(DateTime.Now) : request.DateRequested, 
            Department = request.Department,
            TargetDateNeeded = request.TargetDateNeeded,
            Section = request.Section,
            Quantity = request.Quantity,
            AssetTypeId = request.AssetTypeId,
            AssetType = request.AssetType,
            DetailedDescription = request.DetailedDescription,
            ReasonPurpose = request.ReasonPurpose,
            ProposedLocation = request.ProposedLocation,
            EstimatedLifeSpan = request.EstimatedLifeSpan,
            RequestType = request.RequestType,
            ExistingUnitCount = request.ExistingUnitCount,
            ExistingUser = request.ExistingUser,
            DamagedReportNo = request.DamagedReportNo,
            EvaluatedByName = request.EvaluatedByName, 
            RequestedByName = request.RequestedByName, 
            RequestedByEmployeeId = employeeId,
            RequestStatus = "Draft",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            RequestedAt = DateTime.Now
        };
    }

    // 4. Update DTO → Entity (Mutates existing)
    public static void UpdateFrom(this FixedAssetRequest existing, UpdateFixedAssetRequestDto request)
    {
        existing.DateRequested = request.DateRequested;
        existing.Department = request.Department;
        existing.TargetDateNeeded = request.TargetDateNeeded;
        existing.Section = request.Section;
        existing.Quantity = request.Quantity;
        existing.AssetTypeId = request.AssetTypeId;
        existing.AssetType = request.AssetType;
        existing.DetailedDescription = request.DetailedDescription;
        existing.ReasonPurpose = request.ReasonPurpose;
        existing.ProposedLocation = request.ProposedLocation;
        existing.EstimatedLifeSpan = request.EstimatedLifeSpan;
        existing.RequestType = request.RequestType;
        existing.ExistingUnitCount = request.ExistingUnitCount;
        existing.ExistingUser = request.ExistingUser;
        existing.DamagedReportNo = request.DamagedReportNo;
        existing.EvaluatedByName = request.EvaluatedByName; 
        existing.RequestedByName = request.RequestedByName;
        existing.UpdatedAt = DateTime.Now;
    }

    // 5. Nested DTO → Entity
    public static ExistingUnitDetail ToEntity(this ExistingUnitDetailDto dto, int requestId, int itemNo)
    {
        return new ExistingUnitDetail
        {
            FixedAssetRequestId = requestId,
            ItemNo = itemNo,
            Description = dto.Description,
            Location = dto.Location,
            UserName = dto.UserName,
            Remarks = dto.Remarks,
            CreatedAt = DateTime.Now
        };
    }
}
using DigitalFormsSystem.Core.Dtos;
using DigitalFormsSystem.Core.Interfaces; // For StoredFile
using DigitalFormsSystem.Core.Models;
using Microsoft.AspNetCore.Http;

namespace DigitalFormsSystem.Core.Mappers;

public static class DamagedReportMapper
{
    // 1. HTTP Form → Create DTO
    public static CreateDamagedReportRequest ToCreateRequest(
        this DamagedReport formModel, 
        List<IFormFile>? partIImages, 
        List<IFormFile>? partIIImages)
    {
        return new CreateDamagedReportRequest
        {
            Item = formModel.Item ?? string.Empty,
            CauseOfDamage = formModel.CauseOfDamage ?? string.Empty,
            FixedAssetCode = formModel.FixedAssetCode,
            DatePurchased = formModel.DatePurchased,
            BrandSize = formModel.BrandSize,
            LocationUser = formModel.LocationUser,
            SerialNumber = formModel.SerialNumber,
            Color = formModel.Color,
            IncidentDateTime = formModel.IncidentDateTime,
            ImmediateAction = formModel.ImmediateAction,
            RecommendedAction = formModel.RecommendedAction,
            Remarks = formModel.Remarks,
            ReceivedByEmployeeId = formModel.ReceivedByEmployeeId,
            ReceivedDateTime = formModel.ReceivedDateTime,
            PartIImages = partIImages,
            PartIIImages = partIIImages
        };
    }

    // 2. HTTP Form → Update DTO
    public static UpdateDamagedReportRequest ToUpdateRequest(
        this DamagedReport formModel,
        List<IFormFile>? partIImages,
        List<IFormFile>? partIIImages,
        List<int>? deleteImageIds,
        List<DamagedReportFollowUp>? followUps,
        bool isGadUser)
    {
        return new UpdateDamagedReportRequest
        {
            Id = formModel.Id,
            Item = formModel.Item ?? string.Empty,
            CauseOfDamage = formModel.CauseOfDamage ?? string.Empty,
            FixedAssetCode = formModel.FixedAssetCode,
            DatePurchased = formModel.DatePurchased,
            BrandSize = formModel.BrandSize,
            LocationUser = formModel.LocationUser,
            SerialNumber = formModel.SerialNumber,
            Color = formModel.Color,
            IncidentDateTime = formModel.IncidentDateTime,
            ImmediateAction = formModel.ImmediateAction,
            RecommendedAction = formModel.RecommendedAction,
            Remarks = formModel.Remarks,
            ReceivedByEmployeeId = formModel.ReceivedByEmployeeId,
            ReceivedDateTime = formModel.ReceivedDateTime,
            IsGadUser = isGadUser,
            Findings = formModel.Findings,
            Recommendation = formModel.Recommendation,
            NegligenceFlag = formModel.NegligenceFlag,
            NegligenceDetails = formModel.NegligenceDetails,
            AdministrativeDiscipline = formModel.AdministrativeDiscipline,
            InvestigatedByEmployeeId = formModel.InvestigatedByEmployeeId,
            VerifiedByEmployeeId = formModel.VerifiedByEmployeeId,
            NotedByEmployeeId = formModel.NotedByEmployeeId,
            PartIImages = partIImages,
            PartIIImages = partIIImages,
            DeleteImageIds = deleteImageIds,
            FollowUps = followUps?.Select(f => new DamagedReportFollowUpDto
            {
                FollowUpDate = f.FollowUpDate,
                Status = f.Status,
                UpdateBy = f.UpdateBy,
                NotedBy = f.NotedBy
            }).ToList()
        };
    }

    // 3. Create DTO → Entity
    public static DamagedReport ToEntity(
        this CreateDamagedReportRequest request, 
        int currentEmployeeId, 
        string controlNo)
    {
        return new DamagedReport
        {
            ControlNo = controlNo,
            Item = request.Item,
            CauseOfDamage = request.CauseOfDamage,
            FixedAssetCode = request.FixedAssetCode,
            DatePurchased = request.DatePurchased,
            BrandSize = request.BrandSize,
            LocationUser = request.LocationUser,
            SerialNumber = request.SerialNumber,
            Color = request.Color,
            IncidentDateTime = request.IncidentDateTime,
            ImmediateAction = request.ImmediateAction,
            RecommendedAction = request.RecommendedAction,
            Remarks = request.Remarks,
            ReportedByEmployeeId = currentEmployeeId,
            ReceivedByEmployeeId = request.ReceivedByEmployeeId,
            ReceivedDateTime = request.ReceivedDateTime,
            RequestStatus = "Draft",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
    }

    // 4. Update DTO → Entity (Mutates existing)
    public static void UpdateFrom(this DamagedReport existing, UpdateDamagedReportRequest request)
    {
        existing.Item = request.Item;
        existing.CauseOfDamage = request.CauseOfDamage;
        existing.FixedAssetCode = request.FixedAssetCode;
        existing.DatePurchased = request.DatePurchased;
        existing.BrandSize = request.BrandSize;
        existing.LocationUser = request.LocationUser;
        existing.SerialNumber = request.SerialNumber;
        existing.Color = request.Color;
        existing.IncidentDateTime = request.IncidentDateTime;
        existing.ImmediateAction = request.ImmediateAction;
        existing.RecommendedAction = request.RecommendedAction;
        existing.Remarks = request.Remarks;
        existing.ReceivedByEmployeeId = request.ReceivedByEmployeeId;
        existing.ReceivedDateTime = request.ReceivedDateTime;
        existing.UpdatedAt = DateTime.Now;

        if (request.IsGadUser)
        {
            existing.Findings = request.Findings;
            existing.Recommendation = request.Recommendation;
            existing.NegligenceFlag = request.NegligenceFlag;
            existing.NegligenceDetails = request.NegligenceDetails;
            existing.AdministrativeDiscipline = request.AdministrativeDiscipline;
            existing.InvestigatedByEmployeeId = request.InvestigatedByEmployeeId;
            existing.VerifiedByEmployeeId = request.VerifiedByEmployeeId;
            existing.NotedByEmployeeId = request.NotedByEmployeeId;
        }
    }

    // 5. FollowUp DTO → Entity
    public static DamagedReportFollowUp ToEntity(this DamagedReportFollowUpDto dto, int reportId)
    {
        return new DamagedReportFollowUp
        {
            DamagedReportId = reportId,
            FollowUpDate = dto.FollowUpDate,
            Status = dto.Status,
            UpdateBy = dto.UpdateBy,
            NotedBy = dto.NotedBy,
            CreatedAt = DateTime.Now
        };
    }

    // 6. IFormFile + StoredFile → Image Entity
    public static DamagedReportImage ToImageEntity(
        this IFormFile file, 
        StoredFile stored, 
        int reportId, 
        string section)
    {
        return new DamagedReportImage
        {
            DamagedReportId = reportId,
            Section = section,
            FileName = file.FileName,
            FilePath = stored.PublicUrl,
            StorageFileId = stored.FileId,
            ContentType = file.ContentType,
            UploadedAt = DateTime.Now
        };
    }
}
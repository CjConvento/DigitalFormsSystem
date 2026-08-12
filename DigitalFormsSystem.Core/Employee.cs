using System;
using System.Collections.Generic;

namespace DigitalFormsSystem.Models;

public partial class Employee
{
    public int Id { get; set; }

    public string EmployeeNo { get; set; } = null!;

    public string Name { get; set; } = null!;

    public DateOnly? DateHired { get; set; }

    public string? Company { get; set; }

    public string? Location { get; set; }

    public string? Department { get; set; }

    public string? Section { get; set; }

    public string? Category { get; set; }

    public string? Status { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<FixedAssetPrintLog> FixedAssetPrintLogs { get; set; } = new List<FixedAssetPrintLog>();

    public virtual ICollection<FixedAssetRequestApproval> FixedAssetRequestApprovalExecutiveEvaluatedByEmployees { get; set; } = new List<FixedAssetRequestApproval>();

    public virtual ICollection<FixedAssetRequestApproval> FixedAssetRequestApprovalFinanceProcessedByEmployees { get; set; } = new List<FixedAssetRequestApproval>();

    public virtual ICollection<FixedAssetRequestApproval> FixedAssetRequestApprovalPresidentApprovedByEmployees { get; set; } = new List<FixedAssetRequestApproval>();

    public virtual ICollection<FixedAssetRequestApproval> FixedAssetRequestApprovalReceivedByEmployees { get; set; } = new List<FixedAssetRequestApproval>();

    public virtual ICollection<FixedAssetRequestApproval> FixedAssetRequestApprovalVpapprovedByEmployees { get; set; } = new List<FixedAssetRequestApproval>();

    public virtual ICollection<FixedAssetRequest> FixedAssetRequestEvaluatedByEmployees { get; set; } = new List<FixedAssetRequest>();

    public virtual ICollection<FixedAssetRequest> FixedAssetRequestRequestedByEmployees { get; set; } = new List<FixedAssetRequest>();

    public virtual ICollection<FixedAssetTransferHistory> FixedAssetTransferHistoryFromEmployees { get; set; } = new List<FixedAssetTransferHistory>();

    public virtual ICollection<FixedAssetTransferHistory> FixedAssetTransferHistoryProcessedByEmployees { get; set; } = new List<FixedAssetTransferHistory>();

    public virtual ICollection<FixedAssetTransferHistory> FixedAssetTransferHistoryToEmployees { get; set; } = new List<FixedAssetTransferHistory>();

    public virtual ICollection<MemorandumReceipt> MemorandumReceiptReceivedByEmployees { get; set; } = new List<MemorandumReceipt>();

    public virtual ICollection<MemorandumReceipt> MemorandumReceiptReleasedByEmployees { get; set; } = new List<MemorandumReceipt>();

    public virtual ICollection<RequestStatusHistory> RequestStatusHistories { get; set; } = new List<RequestStatusHistory>();
}

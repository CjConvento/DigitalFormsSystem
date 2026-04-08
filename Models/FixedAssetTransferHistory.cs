using System;
using System.Collections.Generic;

namespace FixedAssetSystem.Models;

public partial class FixedAssetTransferHistory
{
    public int Id { get; set; }

    public string FixedAssetCode { get; set; } = null!;

    public string? FromDepartment { get; set; }

    public string? FromSection { get; set; }

    public int? FromEmployeeId { get; set; }

    public string? ToDepartment { get; set; }

    public string? ToSection { get; set; }

    public int? ToEmployeeId { get; set; }

    public DateOnly TransferDate { get; set; }

    public string? TransferReason { get; set; }

    public int ProcessedByEmployeeId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Employee? FromEmployee { get; set; }

    public virtual Employee ProcessedByEmployee { get; set; } = null!;

    public virtual Employee? ToEmployee { get; set; }
}

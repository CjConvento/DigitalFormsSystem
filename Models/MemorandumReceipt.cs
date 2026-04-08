using System;
using System.Collections.Generic;

namespace FixedAssetSystem.Models;

public partial class MemorandumReceipt
{
    public int Id { get; set; }

    public int FixedAssetRequestId { get; set; }

    public string FixedAssetCode { get; set; } = null!;

    public string? Department { get; set; }

    public string? Section { get; set; }

    public string? Ponumber { get; set; }

    public DateOnly? Podate { get; set; }

    public string? TransactionType { get; set; }

    public string ItemDescription { get; set; } = null!;

    public string? Manufacturer { get; set; }

    public string? SerialNumber { get; set; }

    public string? ModelNumber { get; set; }

    public string? Brand { get; set; }

    public int ReceivedByEmployeeId { get; set; }

    public string? ReceivedByName { get; set; }

    public string? ReceivedSignature { get; set; }

    public DateOnly ReceivedDate { get; set; }

    public int ReleasedByEmployeeId { get; set; }

    public string? ReleasedByName { get; set; }

    public string? ReleasedSignature { get; set; }

    public DateOnly ReleasedDate { get; set; }

    public bool? Ccpurchasing { get; set; }

    public bool? Ccfinance { get; set; }

    public bool? CcrequestingDept { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual FixedAssetRequest FixedAssetRequest { get; set; } = null!;

    public virtual Employee ReceivedByEmployee { get; set; } = null!;

    public virtual Employee ReleasedByEmployee { get; set; } = null!;
}

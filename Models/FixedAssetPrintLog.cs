using System;
using System.Collections.Generic;

namespace FixedAssetSystem.Models;

public partial class FixedAssetPrintLog
{
    public int Id { get; set; }

    public int FixedAssetRequestId { get; set; }

    public int PrintedByEmployeeId { get; set; }

    public DateTime? PrintDateTime { get; set; }

    public string? PrintFormat { get; set; }

    public string? Ipaddress { get; set; }

    public string? UserAgent { get; set; }

    public string? Remarks { get; set; }

    public virtual FixedAssetRequest FixedAssetRequest { get; set; } = null!;

    public virtual Employee PrintedByEmployee { get; set; } = null!;
}

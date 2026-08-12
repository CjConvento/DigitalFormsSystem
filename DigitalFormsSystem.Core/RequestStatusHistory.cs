using System;
using System.Collections.Generic;

namespace DigitalFormsSystem.Models;

public partial class RequestStatusHistory
{
    public int Id { get; set; }

    public int FixedAssetRequestId { get; set; }

    public string? OldStatus { get; set; }

    public string NewStatus { get; set; } = null!;

    public int ChangedByEmployeeId { get; set; }

    public string? Remarks { get; set; }

    public DateTime? ChangedAt { get; set; }

    public virtual Employee ChangedByEmployee { get; set; } = null!;

    public virtual FixedAssetRequest FixedAssetRequest { get; set; } = null!;
}

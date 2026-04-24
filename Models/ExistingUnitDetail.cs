using System;
using System.Collections.Generic;

namespace DigitalFormsSystem.Models;

public partial class ExistingUnitDetail
{
    public int Id { get; set; }

    public int FixedAssetRequestId { get; set; }

    public int ItemNo { get; set; }

    public string Description { get; set; } = null!;

    public string? Location { get; set; }

    public string? UserName { get; set; }

    public string? Remarks { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual FixedAssetRequest FixedAssetRequest { get; set; } = null!;
}

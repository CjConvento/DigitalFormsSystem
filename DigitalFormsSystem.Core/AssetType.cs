using System;
using System.Collections.Generic;

namespace DigitalFormsSystem.Models;

public partial class AssetType
{
    public int Id { get; set; }

    public string AssetTypeName { get; set; } = null!;

    public bool? IsActive { get; set; }

    public virtual ICollection<FixedAssetRequest> FixedAssetRequests { get; set; } = new List<FixedAssetRequest>();
}

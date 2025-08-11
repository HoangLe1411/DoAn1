using System;
using System.Collections.Generic;

namespace DoAn1.Areas.Admin.Models;

public partial class Report
{
    public int ReportId { get; set; }

    public int? ReporterId { get; set; }

    public int? ReportedUserId { get; set; }

    public string? Reason { get; set; }

    public bool? IsResolved { get; set; }

    public DateTime? ReportedAt { get; set; }

    public virtual User? ReportedUser { get; set; }

    public virtual User? Reporter { get; set; }
}

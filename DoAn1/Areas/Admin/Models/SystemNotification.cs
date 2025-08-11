using System;
using System.Collections.Generic;

namespace DoAn1.Areas.Admin.Models;

public partial class SystemNotification
{
    public int NotificationId { get; set; }

    public string? Title { get; set; }

    public string? Content { get; set; }

    public DateTime? CreatedAt { get; set; }
}

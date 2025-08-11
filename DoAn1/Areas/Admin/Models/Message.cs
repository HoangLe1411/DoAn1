using System;
using System.Collections.Generic;

namespace DoAn1.Areas.Admin.Models;

public partial class Message
{
    public int MessageId { get; set; }

    public int? SenderId { get; set; }

    public int? ReceiverId { get; set; }

    public string? Content { get; set; }

    public DateTime? SentAt { get; set; }

    public string? MessageType { get; set; }

    public bool? IsRead { get; set; }

    public bool? IsDeletedBySender { get; set; }

    public bool? IsDeletedByReceiver { get; set; }

    public virtual User? Receiver { get; set; }

    public virtual User? Sender { get; set; }
}

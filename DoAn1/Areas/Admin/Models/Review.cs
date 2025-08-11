using System;
using System.Collections.Generic;

namespace DoAn1.Areas.Admin.Models;

public partial class Review
{
    public int ReviewId { get; set; }

    public int? ProductId { get; set; }

    public int? ReviewerId { get; set; }

    public int? ReviewedUserId { get; set; }

    public int? Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? SellerReply { get; set; }

    public DateTime? SellerReplyAt { get; set; }

    public virtual Product? Product { get; set; }

    public virtual User? ReviewedUser { get; set; }

    public virtual User? Reviewer { get; set; }
}

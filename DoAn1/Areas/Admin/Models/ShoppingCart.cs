using System;
using System.Collections.Generic;

namespace DoAn1.Areas.Admin.Models;

public partial class ShoppingCart
{
    public int CartItemId { get; set; }

    public int UserId { get; set; }

    public int ProductId { get; set; }

    public int? Quantity { get; set; }

    public DateTime? AddedAt { get; set; }

    public bool? IsCheckedOut { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}

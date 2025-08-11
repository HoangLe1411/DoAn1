using System;
using System.Collections.Generic;

namespace DoAn1.Areas.Admin.Models;

public partial class Transaction
{
    public int TransactionId { get; set; }

    public int? BuyerId { get; set; }

    public int? SellerId { get; set; }

    public int? ProductId { get; set; }

    public int? PaymentMethodId { get; set; }

    public string? PaymentStatus { get; set; }

    public decimal? Amount { get; set; }

    public string? ExchangeType { get; set; }

    public DateTime? TransactionDate { get; set; }

    public string? ShippingStatus { get; set; }

    public virtual User? Buyer { get; set; }

    public virtual PaymentMethod? PaymentMethod { get; set; }

    public virtual Product? Product { get; set; }

    public virtual User? Seller { get; set; }
}

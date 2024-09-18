using System;
using System.Collections.Generic;

namespace HShop2024.Data;

public partial class Voucher
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public decimal DiscountAmount { get; set; }

    public bool IsPercentage { get; set; }

    public int? MaHh { get; set; }

    public DateTime? ExpirationDate { get; set; }

    public int? UsageLimit { get; set; }

    public int? TimesUsed { get; set; }

    public virtual HangHoa? MaHhNavigation { get; set; }
}

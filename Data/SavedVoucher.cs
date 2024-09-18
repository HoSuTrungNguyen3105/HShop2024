using System;
using System.Collections.Generic;

namespace HShop2024.Data;

public partial class SavedVoucher
{
    public int Id { get; set; }

    public string? CustomerId { get; set; }

    public string? MaVoucher { get; set; }

    public DateTime? NgayLuu { get; set; }
}

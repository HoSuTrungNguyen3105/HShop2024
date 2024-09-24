using System;
using System.Collections.Generic;

namespace HShop2024.Data;

public partial class LoginHistory
{
    public int Id { get; set; }

    public string MaNv { get; set; } = null!;

    public DateTime LoginTime { get; set; }

    public string? IpAddress { get; set; }

    public virtual NhanVien MaNvNavigation { get; set; } = null!;
}

using System;
using System.Collections.Generic;

namespace HShop2024.Data;

public partial class HangHoa
{
    public int MaHh { get; set; }

    public string TenHh { get; set; } = null!;

    public string? TenAlias { get; set; }

    public int MaLoai { get; set; }

    public string? MoTaDonVi { get; set; }

    public double? DonGia { get; set; }

    public string? Hinh { get; set; }

    public DateTime NgaySx { get; set; }

    public double GiamGia { get; set; }

    public int SoLanXem { get; set; }

    public string? MoTa { get; set; }

    public string MaNcc { get; set; } = null!;

    public bool IsOrganic { get; set; }

    public bool IsFantastic { get; set; }

    public int? SoLuong { get; set; }

    public virtual ICollection<ChiTietHd> ChiTietHds { get; set; } = new List<ChiTietHd>();

    public virtual ICollection<HoiDap> HoiDaps { get; set; } = new List<HoiDap>();

    public virtual Loai MaLoaiNavigation { get; set; } = null!;

    public virtual NhaCungCap MaNccNavigation { get; set; } = null!;

    public virtual ICollection<Voucher> Vouchers { get; set; } = new List<Voucher>();

    public virtual ICollection<YeuThich> YeuThiches { get; set; } = new List<YeuThich>();
}

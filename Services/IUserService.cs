using HShop2024.Data;

namespace HShop2024.Services
{
    public interface IUserService
    {
        Task<KhachHang> GetUserByIdAsync(string userId);
        Task UpdateUserAsync(KhachHang user);
    }

}

using AutoMapper;
using HShop2024.Data;
using HShop2024.ViewModels;

namespace HShop2024.Helpers
{
	public class AutoMapperProfile : Profile
	{
		public AutoMapperProfile()
		{
			CreateMap<RegisterVM, KhachHang>();
			//.ForMember(kh => kh.UserName, option => option.MapFrom(RegisterVM => RegisterVM.UserName))
			//.ReverseMap();
		}
	}
}

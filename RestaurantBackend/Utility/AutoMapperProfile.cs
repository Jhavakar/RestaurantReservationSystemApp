using AutoMapper;
using RestaurantBackend.Models;
using RestaurantBackend.ViewModels;

namespace RestaurantBackend.Utility
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Customer, CustomerVM>()
            .ForMember(dest => dest.EmailAddress, opt => opt.MapFrom(src => src.Email))
            .ReverseMap();
            CreateMap<Reservation, ReservationVM>().ReverseMap();
        }
    }
}

using AutoMapper;
using RestaurantBackend.Models;
using RestaurantBackend.ViewModels;

namespace RestaurantBackend.Utility
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Customer, CustomerVM>().ReverseMap();
        }
    }
}

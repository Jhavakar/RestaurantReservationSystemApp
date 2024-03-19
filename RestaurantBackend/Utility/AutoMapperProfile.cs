using AutoMapper;
using RestaurantBackend.Models;
using RestaurantBackend.ViewModels;

namespace RestaurantBackend.Utility
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Existing mapping
            CreateMap<CustomerVM, Customer>().ReverseMap(); // Adding ReverseMap() allows bi-directional mapping

            // Additional mappings as needed
            CreateMap<TableVM, Table>().ReverseMap();
            CreateMap<ReservationVM, Reservation>().ReverseMap();
            // Add other mappings here
        }
    }
}

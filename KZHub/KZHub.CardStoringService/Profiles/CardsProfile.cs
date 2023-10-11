using AutoMapper;
using KZHub.CardStoringService.DTOs;
using KZHub.CardStoringService.Models;

namespace KZHub.CardStoringService.Profiles
{
    public class CardsProfile : Profile
    {
        public CardsProfile()
        {
            CreateMap<CreateCardDTO, Card>();
            CreateMap<CreatePointDTO, Point>();
        }
    }
}

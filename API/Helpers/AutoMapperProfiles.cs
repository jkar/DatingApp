using System.Linq;
using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;

namespace API.Helpers
{
    public class AutoMapperProfiles : Profile
    {

        public AutoMapperProfiles()
        {
            // source -> to

            //ME TO forMember δινω τιμη στο PhotoUrl του MemberDto απο το photos Που γίνεται populate, και παίρνω το url σε όσα είναι το IsMain true.
            CreateMap<AppUser, MemberDto>()
                .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src => src.Photos.FirstOrDefault(x => x.IsMain).Url))
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.DateOfBirth.CalculateAge()));

            CreateMap<Photo, PhotoDto>();
            CreateMap<MemberUpdateDTO, AppUser>();
            CreateMap<RegisterDto, AppUser>();
        }
        
    }
}
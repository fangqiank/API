using AutoMapper;
using Routing.Api.Dto;
using Routing.Api.Entities;

namespace Routing.Api.Profiles
{
    public class CompanyProfile:Profile  //company entity mapped to company dto
    {
        public CompanyProfile()
        {
            CreateMap<Company, CompanyDto>()//参数(source,destination)
                .ForMember(
                    dest=>dest.CompanyName,
                    opt=>opt.MapFrom(
                        src=>src.Name));

            CreateMap<CompanyAddDto, Company>();

            CreateMap<Company, CompanyFullDto>();

            CreateMap<CompanyAddWithBankruptTimeDto, Company>();
        }
    }
}

﻿using AutoMapper;
using Routing.Api.Dto;
using Routing.Api.Entities;
using System;

namespace Routing.Api.Profiles
{
    public class EmployeeProfile:Profile
    {
        public EmployeeProfile()
        {
            CreateMap<Employee, EmployeeDto>()
                .ForMember(dest =>
                        dest.Name,
                    opt => 
                        opt.MapFrom(src =>
                        $"{src.FirstName} {src.LastName}"
                    )
                )
                .ForMember(dest =>
                        dest.GenderDisplay,
                    opt => 
                        opt.MapFrom(src =>
                        src.Gender.ToString())
                )
                .ForMember(dest
                        => dest.Age,
                    opt => 
                        opt.MapFrom(src =>
                        DateTime.Now.Year - src.DateOfBirth.Year)
                );

            CreateMap<EmployeeAddDto, Employee>();

            CreateMap<EmployeeUpdatedDto, Employee>();

            CreateMap<Employee,EmployeeUpdatedDto>();
        }
    }
}

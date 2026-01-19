using AutoMapper;
using Mvc_CRUD.Dto;
using Mvc_CRUD.Models;

namespace Mvc_CRUD.Services;

    public class MappingService : Profile
    {
          public MappingService(){
            CreateMap<Comments, CommentsDto>()
            .ForMember(x => x.UserName, k => k.MapFrom(s => s.UserName));
          }
    }


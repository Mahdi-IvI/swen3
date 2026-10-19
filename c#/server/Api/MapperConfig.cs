using Api.DTOs;
using AutoMapper;
using Models;

namespace Api;

public class MapperConfig : Profile
{
    public MapperConfig()
    {
        CreateMap<Document, DocumentDto>()
            .ReverseMap()
            .ForMember(d => d.Username, opt => opt.Ignore())
            .ForMember(d => d.Summery, opt => opt.Ignore());

        CreateMap<Folder, FolderDto>()
            .ReverseMap()
            .ForMember(d => d.Username, opt => opt.Ignore());

        CreateMap<User, UserProfileDto>();
    }
}
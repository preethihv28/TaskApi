using AutoMapper;
using TaskApi.Models;
using TaskApi.Models.DTOs;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TaskApi.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ToDoItem, ToDoItemDto>().ReverseMap();
            CreateMap<CreateToDoItemDto, ToDoItem>();
        }
    }
}

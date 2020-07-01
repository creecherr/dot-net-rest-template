using System;

using AutoMapper;
using TemplateAPI.Models;

namespace TemplateAPI.MappingProfiles
{
    public class TodoMappings : Profile
    {
        public TodoMappings()
        {
            CreateMap<TodoEntity, TodoItemUpsertDTO>().ReverseMap();
            CreateMap<TodoItemResponseDTO, TodoEntity>().ReverseMap();
        }
    }
}

using System;
namespace TemplateAPI.Models
{
    public class TodoItemResponseDTO
        {
            public long Id { get; set; }
            public string Name { get; set; }
            public bool IsComplete { get; set; }
        }
}

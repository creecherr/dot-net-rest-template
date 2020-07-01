using Microsoft.EntityFrameworkCore;

namespace TemplateAPI.Models
{
    public class TodoContext : DbContext
    {
        public TodoContext(DbContextOptions<TodoContext> options)
            : base(options)
        {
        }

        public DbSet<TodoEntity> TodoItems { get; set; }
    }
}
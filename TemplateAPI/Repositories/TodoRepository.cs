using System;
using System.Linq;
using TemplateAPI.Models;

namespace TemplateAPI.Repositories
{
    public interface ITodoRepository
    {
        TodoEntity GetSingle(int id);
        void Add(TodoEntity item);
        void Delete(TodoEntity item);
        TodoEntity Update(int id, TodoEntity item);
        IQueryable<TodoEntity> GetAll(QueryParameters queryParameters);
        int Count();
        bool Save();
    }

    public class TodoRepository : ITodoRepository
    {
        private readonly TodoContext _todoContext;

        public TodoRepository(TodoContext todoContext)
        {
            _todoContext = todoContext;
        }

        TodoEntity ITodoRepository.GetSingle(int id)
        {
            return _todoContext.TodoItems.FirstOrDefault(x => x.Id == id);
        }

        public void Add(TodoEntity item)
        {
            _todoContext.TodoItems.Add(item);
        }

        public TodoEntity Update(int id, TodoEntity item)
        {
            _todoContext.TodoItems.Update(item);
            return item;
        }

        public IQueryable<TodoEntity> GetAll(QueryParameters queryParameters)
        {
            IQueryable<TodoEntity> _allItems = _todoContext.TodoItems;

            //want to make sure that query parameters exist 
            if (!String.IsNullOrEmpty(queryParameters.Query))
            {
                /*This is if there are multiple query strings....
                 * _allItems = _allItems
                    .Where(x => x.Name.ToString().Contains(queryParameters.Query.ToLowerInvariant())
                    || x.Name.ToLowerInvariant().Contains(queryParameters.Query.ToLowerInvariant()));
                    */
                _allItems = _allItems
                .Where(x => x.Name.ToString().Contains(queryParameters.Query.ToLowerInvariant()));
            }

            return _allItems
                .Skip(queryParameters.PageCount * (queryParameters.Page - 1))
                .Take(queryParameters.PageCount);
        }

        public void Delete(TodoEntity item)
        {
            _todoContext.TodoItems.Remove(item);
        }

        public int Count()
        {
            return _todoContext.TodoItems.Count();
        }

        public bool Save()
        {
            return (_todoContext.SaveChanges() >= 0);
        }
    }
}

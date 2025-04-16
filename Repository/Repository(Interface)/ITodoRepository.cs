using TaskApi.Models;

namespace TaskApi.Repositories
{
    public interface IToDoRepository
    {
        Task<IEnumerable<ToDoItem>> GetAllAsync();
        Task<IEnumerable<ToDoItem>> GetPagedAsync(int pageNumber, int pageSize);
        Task<ToDoItem?> GetByIdAsync(int id);
        Task AddAsync(ToDoItem item);
        Task UpdateAsync(ToDoItem item);
        Task DeleteAsync(int id);
        Task<IEnumerable<ToDoItem>> GetByDateRangeAsync(long startEpoch, long endEpoch);
        Task<IEnumerable<ToDoItem>> SearchAsync(string query);
    }
}

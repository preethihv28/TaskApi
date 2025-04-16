using TaskApi.Models;

namespace TaskApi.Services
{
    public interface IToDoService
    {
        Task<IEnumerable<ToDoItem>> GetAllAsync();
        Task<IEnumerable<ToDoItem>> GetPagedAsync(int pageNumber, int pageSize);
        Task<ToDoItem?> GetByIdAsync(int id);
        Task CreateAsync(ToDoItem item);
        Task UpdateAsync(ToDoItem item);
        Task DeleteAsync(int id);
        Task<IEnumerable<ToDoItem>> GetByDateRangeAsync(long startEpoch, long endEpoch);
    }
}

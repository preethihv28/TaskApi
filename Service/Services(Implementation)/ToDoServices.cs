using TaskApi.Models;
using TaskApi.Repositories;
using Microsoft.Extensions.Logging;

namespace TaskApi.Services
{
    public class ToDoService : IToDoService
    {
        private readonly IToDoRepository _repository;
        private readonly ILogger<ToDoService> _logger;

        public ToDoService(IToDoRepository repository, ILogger<ToDoService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<IEnumerable<ToDoItem>> GetAllAsync()
        {
            _logger.LogInformation("Fetching all ToDo items...");

            var items = await _repository.GetAllAsync();

            _logger.LogInformation("Retrieved {Count} items", items.Count());
            return items;
        }

        public async Task<IEnumerable<ToDoItem>> GetPagedAsync(int pageNumber, int pageSize)
        {
            _logger.LogInformation("Fetching paged ToDo items: Page {Page}, Size {Size}", pageNumber, pageSize);

            var items = await _repository.GetPagedAsync(pageNumber, pageSize);

            _logger.LogInformation("Paged result contains {Count} items", items.Count());
            return items;
        }

        public async Task<ToDoItem?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching ToDo item with ID {Id}", id);

            var item = await _repository.GetByIdAsync(id);

            if (item == null)
            {
                _logger.LogWarning("ToDo item with ID {Id} not found", id);
            }
            else
            {
                _logger.LogInformation("ToDo item with ID {Id} retrieved successfully", id);
            }

            return item;
        }

        public async Task CreateAsync(ToDoItem item)
        {
            _logger.LogInformation("Creating new ToDo item...");

            item.CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            await _repository.AddAsync(item);

            _logger.LogInformation("ToDo item created with ID {Id}", item.Id);
        }

        public async Task UpdateAsync(ToDoItem item)
        {
            _logger.LogInformation("Updating ToDo item with ID {Id}", item.Id);

            await _repository.UpdateAsync(item);

            _logger.LogInformation("ToDo item with ID {Id} updated successfully", item.Id);
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting ToDo item with ID {Id}", id);

            await _repository.DeleteAsync(id);

            _logger.LogInformation("ToDo item with ID {Id} deleted", id);
        }

        public async Task<IEnumerable<ToDoItem>> GetByDateRangeAsync(long startEpoch, long endEpoch)
        {
            _logger.LogInformation("Fetching ToDo items from {Start} to {End}", startEpoch, endEpoch);

            var items = await _repository.GetByDateRangeAsync(startEpoch, endEpoch);

            _logger.LogInformation("Retrieved {Count} items for the date range", items.Count());
            return items;
        }


        // New SearchAsync method to handle search queries
        public async Task<IEnumerable<ToDoItem>> SearchAsync(string query)
        {
            _logger.LogInformation("Searching ToDo items with query: {Query}", query);

            // Ensure the query isn't null or empty before attempting to search
            if (string.IsNullOrWhiteSpace(query))
            {
                _logger.LogWarning("Search query is empty or null");
                return Enumerable.Empty<ToDoItem>(); // Return an empty list if query is invalid
            }

            // Fetch search results from the repository
            var items = await _repository.SearchAsync(query);

            _logger.LogInformation("Found {Count} items matching query: {Query}", items.Count(), query);
            return items;
        }
    }
}

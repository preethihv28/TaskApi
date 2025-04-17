using Dapper;
using System.Data;
using TaskApi.Data;
using TaskApi.Models;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace TaskApi.Repositories
{
    public class ToDoRepository : IToDoRepository
    {
        private readonly DapperDbContext _context;
        private readonly ILogger<ToDoRepository> _logger;

        public ToDoRepository(DapperDbContext context, ILogger<ToDoRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<ToDoItem>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all ToDo items");
                using var conn = _context.CreateConnection();
                var result = await conn.QueryAsync<ToDoItem>("SELECT * FROM \"ToDoItems\"");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching all ToDo items");
                throw;
            }
        }

        public async Task<IEnumerable<ToDoItem>> GetPagedAsync(int pageNumber, int pageSize)
        {
            try
            {
                _logger.LogInformation("Fetching paged ToDo items. Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);
                using var conn = _context.CreateConnection();
                var offset = (pageNumber - 1) * pageSize;

                string sql = @"
                    SELECT * FROM ""ToDoItems""
                    ORDER BY ""Id""
                    LIMIT @PageSize OFFSET @Offset";

                var result = await conn.QueryAsync<ToDoItem>(sql, new { PageSize = pageSize, Offset = offset });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching paged ToDo items.");
                throw;
            }
        }

        public async Task<ToDoItem?> GetByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Fetching ToDo item with ID: {Id}", id);
                using var conn = _context.CreateConnection();
                var result = await conn.QuerySingleOrDefaultAsync<ToDoItem>(
                    "SELECT * FROM \"ToDoItems\" WHERE \"Id\" = @Id",
                    new { Id = id }
                );
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching ToDo item with ID: {Id}", id);
                throw;
            }
        }

        public async Task AddAsync(ToDoItem item)
        {
            try
            {
                _logger.LogInformation("Adding a new ToDo item.");
                using var conn = _context.CreateConnection();

                var sql = @"
                    INSERT INTO public.""ToDoItems"" 
                    (""Title"", ""Description"", ""IsCompleted"", ""CreatedAt"")
                    VALUES (@Title, @Description, @IsCompleted, @CreatedAt)";

                await conn.ExecuteAsync(sql, item);
                _logger.LogInformation("ToDo item added successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding a new ToDo item.");
                throw;
            }
        }

        public async Task UpdateAsync(ToDoItem item)
        {
            try
            {
                _logger.LogInformation("Updating ToDo item with ID: {Id}", item.Id);
                using var conn = _context.CreateConnection();

                string sql = @"
                    UPDATE ""ToDoItems""
                    SET ""Title"" = @Title,
                        ""Description"" = @Description,
                        ""IsCompleted"" = @IsCompleted
                    WHERE ""Id"" = @Id";

                await conn.ExecuteAsync(sql, item);
                _logger.LogInformation("ToDo item with ID: {Id} updated successfully.", item.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating ToDo item with ID: {Id}", item.Id);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                _logger.LogInformation("Deleting ToDo item with ID: {Id}", id);
                using var conn = _context.CreateConnection();

                await conn.ExecuteAsync(
                    "DELETE FROM \"ToDoItems\" WHERE \"Id\" = @Id",
                    new { Id = id }
                );

                _logger.LogInformation("ToDo item with ID: {Id} deleted successfully.", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting ToDo item with ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<ToDoItem>> GetByDateRangeAsync(long startEpoch, long endEpoch)
        {
            try
            {
                _logger.LogInformation("Fetching ToDo items from {StartEpoch} to {EndEpoch}", startEpoch, endEpoch);
                using var conn = _context.CreateConnection();

                string sql = @"
                    SELECT * FROM ""ToDoItems""
                    WHERE ""CreatedAt"" >= @StartEpoch AND ""CreatedAt"" < @EndEpoch
                    ORDER BY ""CreatedAt"" ASC";

                var result = await conn.QueryAsync<ToDoItem>(sql, new { StartEpoch = startEpoch, EndEpoch = endEpoch });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching ToDo items by date range.");
                throw;
            }
        }

        public async Task<IEnumerable<ToDoItem>> SearchAsync(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    _logger.LogWarning("Search query is null or empty.");
                    return Enumerable.Empty<ToDoItem>();
                }

                _logger.LogInformation("Searching ToDo items with query: {Query}", query);
                using var conn = _context.CreateConnection();

                string sql = @"
                    SELECT * FROM ""ToDoItems""
                    WHERE ""Title"" ILIKE @Query OR ""Description"" ILIKE @Query
                    ORDER BY ""CreatedAt"" DESC";

                var result = await conn.QueryAsync<ToDoItem>(sql, new { Query = "%" + query + "%" });
                _logger.LogInformation("Found {Count} ToDo items matching the search query.", result.Count());
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching ToDo items.");
                throw;
            }
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text;
using AutoMapper;
using TaskApi.Models;
using TaskApi.Models.DTOs;
using TaskApi.Services;
using Microsoft.Extensions.Logging;

namespace TaskApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ToDoController : ControllerBase
    {
        private readonly IToDoService _service;
        private readonly IMapper _mapper;
        private readonly ILogger<ToDoController> _logger;

        public ToDoController(IToDoService service, IMapper mapper, ILogger<ToDoController> logger)
        {
            _service = service;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("GetAll() started");

            try
            {
                var items = await _service.GetAllAsync();
                var dtoList = _mapper.Map<IEnumerable<ToDoItemDto>>(items);

                _logger.LogInformation("GetAll() completed successfully with {Count} items", dtoList.Count());
                return Ok(dtoList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in GetAll()");
                return StatusCode(500, "Internal server error.");
            }
        }


        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            _logger.LogInformation("Search() called with query={Query}", query);

            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Search query cannot be empty.");

            try
            {
                var results = await _service.SearchAsync(query);
                var dtoList = _mapper.Map<IEnumerable<ToDoItemDto>>(results);

                _logger.LogInformation("Search() found {Count} items matching '{Query}'", dtoList.Count(), query);
                return Ok(dtoList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in Search()");
                return StatusCode(500, "Internal server error.");
            }
        }


        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged(int? pageNumber, int? pageSize)
        {
            _logger.LogInformation("GetPaged() called with pageNumber={PageNumber}, pageSize={PageSize}", pageNumber, pageSize);

            if (pageNumber <= 0)
                return BadRequest("Page number must be greater than 0.");

            if (pageSize <= 0 || pageSize > 100)
                return BadRequest("Page size must be between 1 and 100.");

            try
            {
                int page = pageNumber ?? 1;
                int size = pageSize ?? 5;

                var pagedItems = await _service.GetPagedAsync(page, size);
                var dtoList = _mapper.Map<IEnumerable<ToDoItemDto>>(pagedItems);

                _logger.LogInformation("GetPaged() returned {Count} items", dtoList.Count());
                return Ok(dtoList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in GetPaged()");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("GetById() called with id={Id}", id);

            if (id <= 0)
                return BadRequest("Invalid ID.");

            try
            {
                var item = await _service.GetByIdAsync(id);
                if (item == null)
                {
                    _logger.LogWarning("Item with id={Id} not found", id);
                    return NotFound();
                }

                var dto = _mapper.Map<ToDoItemDto>(item);
                _logger.LogInformation("GetById() successful for id={Id}", id);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in GetById()");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateToDoItemDto dto)
        {
            _logger.LogInformation("Create() called");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest("Title is required.");

            try
            {
                var toDoItem = _mapper.Map<ToDoItem>(dto);
                toDoItem.CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                await _service.CreateAsync(toDoItem);
                var resultDto = _mapper.Map<ToDoItemDto>(toDoItem);

                _logger.LogInformation("Create() successful with new item id={Id}", toDoItem.Id);
                return CreatedAtAction(nameof(GetById), new { id = toDoItem.Id }, resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in Create()");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] ToDoItemDto dto)
        {
            _logger.LogInformation("Update() called for id={Id}", dto.Id);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var item = _mapper.Map<ToDoItem>(dto);
                await _service.UpdateAsync(item);

                _logger.LogInformation("Update() successful for id={Id}", dto.Id);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in Update()");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Delete() called for id={Id}", id);

            if (id <= 0)
                return BadRequest("Invalid ID.");

            try
            {
                await _service.DeleteAsync(id);
                _logger.LogInformation("Delete() successful for id={Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in Delete()");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportToCsv([FromQuery] string date)
        {
            _logger.LogInformation("ExportToCsv() called for date={Date}", date);

            if (!DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                return BadRequest("Invalid date format. Use yyyy-MM-dd.");

            try
            {
                long startEpoch = new DateTimeOffset(parsedDate).ToUnixTimeSeconds();
                long endEpoch = new DateTimeOffset(parsedDate.AddDays(1)).ToUnixTimeSeconds();

                var items = await _service.GetByDateRangeAsync(startEpoch, endEpoch);

                var csv = new StringBuilder();
                csv.AppendLine("Id,Title,IsCompleted,CreatedAt");

                foreach (var item in items)
                {
                    csv.AppendLine($"{item.Id},{item.Title},{item.Iscompleted},{item.CreatedAt}");
                }

                var bytes = Encoding.UTF8.GetBytes(csv.ToString());

                _logger.LogInformation("ExportToCsv() successful. Exported {Count} items", items.Count());
                return File(bytes, "text/csv", $"todo_export_{date}.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in ExportToCsv()");
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}

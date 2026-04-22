using Microsoft.AspNetCore.Mvc;
using FinProjectTaskTracker.Models;
using FinProjectTaskTracker.Services;

namespace FinProjectTaskTracker.Controllers;

[ApiController]
[Route("api/boards")]
public class BoardsController : ControllerBase
{
    private readonly IBoardService _boardService;

    public BoardsController(IBoardService boardService)
    {
        _boardService = boardService;
    }

    // 1. GET /api/boards
    [HttpGet]
    public async Task<IActionResult> GetBoards()
    {
        var boards = await _boardService.GetBoardsAsync();
        return Ok(boards);
    }

    // 2. POST /api/boards
    [HttpPost]
    public async Task<IActionResult> CreateBoard([FromBody] Board board)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var created = await _boardService.CreateBoardAsync(board);
            return CreatedAtAction(nameof(GetBoards), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // 3. GET /api/boards/{id}/tasks
    [HttpGet("{id}/tasks")]
    public async Task<IActionResult> GetBoardTasks(
        Guid id,
        [FromQuery] Status? status,
        [FromQuery] Priority? priority)
    {
        try
        {
            var tasks = await _boardService.GetTasksAsync(id, status, priority);
            return Ok(tasks);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // 4. POST /api/boards/{id}/tasks
    [HttpPost("{id}/tasks")]
    public async Task<IActionResult> CreateTask(Guid id, [FromBody] TaskItem task)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var createdTask = await _boardService.AddTaskToBoardAsync(id, task);
            return CreatedAtAction(nameof(GetBoardTasks), new { id = id }, createdTask);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
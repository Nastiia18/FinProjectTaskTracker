using Microsoft.AspNetCore.Mvc;
using FinProjectTaskTracker.Models;
using FinProjectTaskTracker.Services;

namespace FinProjectTaskTracker.Controllers;

[ApiController]
[Route("api/tasks")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    // 1. PUT /api/tasks/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(Guid id, [FromBody] TaskItem updatedTask)
    {
        try
        {
            var result = await _taskService.UpdateTaskAsync(id, updatedTask);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // 2. PATCH /api/tasks/{id}/status
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] Status newStatus)
    {
        try
        {
            var result = await _taskService.ChangeStatusAsync(id, newStatus);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // 3. DELETE /api/tasks/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(Guid id)
    {
        try
        {
            await _taskService.DeleteTaskAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    // 4. GET /api/tasks/overdue
    [HttpGet("overdue")]
    public async Task<IActionResult> GetOverdueTasks()
    {
        var result = await _taskService.GetOverdueAsync();
        return Ok(result);
    }
}
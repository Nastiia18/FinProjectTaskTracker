using Microsoft.AspNetCore.Mvc;
using FinProjectTaskTracker.Models;

namespace FinProjectTaskTracker.Controllers;

[ApiController]
[Route("api")]
public class TasksController : ControllerBase
{
   private static List<TaskItem> tasks = new List<TaskItem>();
   
   //GET
   [HttpGet("boards/{boardId}/tasks")]
   public IActionResult GetTasks(Guid boardId, [FromQuery] Status? status, [FromQuery] Priority? priority)
   {
       var query = tasks.Where(t => t.BoardId == boardId);

       if (status.HasValue)
           query = query.Where(t => t.Status == status.Value);
       
       if (priority.HasValue)
           query = query.Where(t => t.Priority == priority.Value);
       
       return Ok(query.ToList());
   }
  
   //POST
   [HttpPost("boards/{boardId}/tasks")]
   public IActionResult CreateTask(Guid boardId, TaskItem task)
   {
       if (task.DueDate <= DateTime.UtcNow)
           return BadRequest("Due date must be in the future");

       if (task.Priority == Priority.Critical && task.AssigneeId == null)
           return BadRequest("Critical tasks must have assignee");

       task.Id = Guid.NewGuid();
       task.BoardId = boardId;
       task.CreatedAt = DateTime.UtcNow;
       task.Status = Status.Todo;

       tasks.Add(task);

       return Ok(task);
   }
  
   //PATCH status
   [HttpPatch("tasks/{id}/status")]
   public IActionResult ChangeStatus(Guid id, [FromBody] Status newStatus)
   {
       var task = tasks.FirstOrDefault(t => t.Id == id);
       
       if (task == null)
           return NotFound();
       
       if (task.Status == Status.Todo && newStatus == Status.Done)
           return BadRequest("Cannot skip InProgress");

       if (task.Status == Status.Done)
           return BadRequest("Cannot change status from Done");
       
       if (task.Status == Status.InProgress && newStatus == Status.Todo)
           return BadRequest("Cannot go back to Todo");

       task.Status = newStatus;

       return Ok(task);
   }
  
   //DELETE
   [HttpDelete("tasks/{id}")]
   public IActionResult DeleteTask(Guid id)
   {
       var task = tasks.FirstOrDefault(t => t.Id == id);
       if (task == null)
           return NotFound();
       
       tasks.Remove(task);

       return NoContent();
   }
  
   //PUT task
   [HttpPut("tasks/{id}")]
   public IActionResult UpdateTask(Guid id, TaskItem updatedTask)
   {
       var task = tasks.FirstOrDefault(t => t.Id == id);
       
       if (task == null)
           return NotFound();
       
       if (updatedTask.DueDate <= DateTime.UtcNow)
           return BadRequest("Due date must be in the future");

       if (updatedTask.Priority == Priority.Critical && updatedTask.AssigneeId == null)
           return BadRequest("Critical tasks must have assignee");


       task.Title = updatedTask.Title;
       task.Description = updatedTask.Description;
       task.Priority = updatedTask.Priority;
       task.AssigneeId = updatedTask.AssigneeId;
       task.DueDate = updatedTask.DueDate;
       
       return Ok(task);
   }
  
   //overdue
   [HttpGet("tasks/overdue")]
   public IActionResult GetOverdueTasks()
   {
       var overdue = tasks
           .Where(t => t.DueDate < DateTime.UtcNow && t.Status != Status.Done)
           .ToList();
       
       return Ok(overdue);
   }
}

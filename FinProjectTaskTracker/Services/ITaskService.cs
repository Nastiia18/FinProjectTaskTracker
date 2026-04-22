using FinProjectTaskTracker.Models;

namespace FinProjectTaskTracker.Services;

public interface ITaskService
{
    Task<TaskItem> UpdateTaskAsync(Guid id, TaskItem updatedTask);
    Task<TaskItem> ChangeStatusAsync(Guid id, Status newStatus);
    Task DeleteTaskAsync(Guid id);
    Task<List<TaskItem>> GetOverdueAsync();
}
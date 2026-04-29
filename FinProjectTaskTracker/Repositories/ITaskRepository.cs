namespace FinProjectTaskTracker.Repositories;
using FinProjectTaskTracker.Models;
public interface ITaskRepository
{
    Task<TaskItem?> GetByIdAsync(Guid id);
    Task<List<TaskItem>> GetOverdueAsync();
    Task AddAsync(TaskItem task);
    Task UpdateAsync(TaskItem task);
    Task DeleteAsync(TaskItem task);
    Task BulkUpdateStatusAsync(Guid boardId, Status oldStatus, Status newStatus);
}
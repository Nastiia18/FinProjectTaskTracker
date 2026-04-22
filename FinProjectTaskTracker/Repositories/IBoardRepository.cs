using FinProjectTaskTracker.Models;

namespace FinProjectTaskTracker.Repositories;

public interface IBoardRepository
{
    Task<List<Board>> GetAllAsync();
    Task<bool> ExistsByNameAsync(string name);
    Task<bool> ExistsByIdAsync(Guid id);
    Task AddAsync(Board board);
    
    Task<List<TaskItem>> GetTasksByBoardIdAsync(Guid boardId, Status? status, Priority? priority);
    Task AddTaskAsync(TaskItem task);
}
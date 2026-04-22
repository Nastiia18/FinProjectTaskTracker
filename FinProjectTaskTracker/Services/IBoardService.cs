using FinProjectTaskTracker.Models;

namespace FinProjectTaskTracker.Services;

public interface IBoardService
{
    Task<List<Board>> GetBoardsAsync();
    Task<Board> CreateBoardAsync(Board board);

    Task<List<TaskItem>> GetTasksAsync(Guid boardId, Status? status, Priority? priority);
    Task<TaskItem> AddTaskToBoardAsync(Guid boardId, TaskItem task);
}
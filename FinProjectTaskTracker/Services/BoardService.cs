using FinProjectTaskTracker.Models;
using FinProjectTaskTracker.Repositories;

namespace FinProjectTaskTracker.Services;

public class BoardService : IBoardService
{
    private readonly IBoardRepository _boardRepo;

    public BoardService(IBoardRepository boardRepo)
    {
        _boardRepo = boardRepo;
    }

    public async Task<List<Board>> GetBoardsAsync()
        => await _boardRepo.GetAllAsync();

    public async Task<Board> CreateBoardAsync(Board board)
    {
        if (await _boardRepo.ExistsByNameAsync(board.Name))
            throw new InvalidOperationException("Board name must be unique");

        board.Id = Guid.NewGuid();
        board.CreatedAt = DateTime.UtcNow;

        await _boardRepo.AddAsync(board);
        return board;
    }
    
    public async Task<List<TaskItem>> GetTasksAsync(Guid boardId, Status? status, Priority? priority)
    {
        var exists = await _boardRepo.ExistsByIdAsync(boardId);
        if (!exists)
            throw new KeyNotFoundException("Board not found");

        return await _boardRepo.GetTasksByBoardIdAsync(boardId, status, priority);
    }

    public async Task<TaskItem> AddTaskToBoardAsync(Guid boardId, TaskItem task)
    {
        var exists = await _boardRepo.ExistsByIdAsync(boardId);
        if (!exists)
            throw new KeyNotFoundException("Board not found");

        if (task.DueDate <= DateTime.UtcNow)
            throw new ArgumentException("Due date must be in the future");

        if (task.Priority == Priority.Critical && task.AssigneeId == null)
            throw new ArgumentException("Critical tasks must have assignee");

        task.Id = Guid.NewGuid();
        task.BoardId = boardId;
        task.CreatedAt = DateTime.UtcNow;
        task.Status = Status.Todo;

        await _boardRepo.AddTaskAsync(task);
        return task;
    }
}
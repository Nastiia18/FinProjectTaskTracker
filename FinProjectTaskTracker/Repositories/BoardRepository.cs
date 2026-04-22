using Microsoft.EntityFrameworkCore;
using FinProjectTaskTracker.Data;
using FinProjectTaskTracker.Models;

namespace FinProjectTaskTracker.Repositories;

public class BoardRepository : IBoardRepository
{
    private readonly AppDbContext _context;

    public BoardRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Board>> GetAllAsync()
        => await _context.Boards.ToListAsync();

    public async Task<bool> ExistsByNameAsync(string name)
        => await _context.Boards.AnyAsync(b => b.Name.ToLower() == name.ToLower());

    public async Task<bool> ExistsByIdAsync(Guid id)
        => await _context.Boards.AnyAsync(b => b.Id == id);

    public async Task AddAsync(Board board)
    {
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();
    }
    public async Task<List<TaskItem>> GetTasksByBoardIdAsync(Guid boardId, Status? status, Priority? priority)
    {
        var query = _context.Tasks.Where(t => t.BoardId == boardId);

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        if (priority.HasValue)
            query = query.Where(t => t.Priority == priority.Value);

        return await query.ToListAsync();
    }

    public async Task AddTaskAsync(TaskItem task)
    {
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
    }
}
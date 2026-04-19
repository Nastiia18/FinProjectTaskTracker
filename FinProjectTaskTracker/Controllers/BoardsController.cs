using Microsoft.AspNetCore.Mvc;
using FinProjectTaskTracker.Models;


namespace FinProjectTaskTracker.Controllers;


[ApiController]
[Route("api/[controller]")]
public class BoardsController : ControllerBase
{
    private static List<Board> boards = new List<Board>();


    [HttpGet]
    public IActionResult GetBoards()
    {
        return Ok(boards);
    }


    [HttpPost]
    public IActionResult CreateBoard(Board board)
    {
        if (boards.Any(b => b.Name == board.Name))
            return BadRequest("Board name must be unique");


        board.Id = Guid.NewGuid();
        board.CreatedAt = DateTime.UtcNow;


        boards.Add(board);


        return Ok(board);
    }
}
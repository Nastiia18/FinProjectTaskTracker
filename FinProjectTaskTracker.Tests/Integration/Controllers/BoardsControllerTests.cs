using System.Net;
using System.Net.Http.Json;
using FinProjectTaskTracker.Tests.Factories;
using FinProjectTaskTracker.Models;
using FinProjectTaskTracker.Data;
using Microsoft.Extensions.DependencyInjection;


namespace FinProjectTaskTracker.Tests.Integration.Controllers;


public class BoardsControllerTests
   : IClassFixture<CustomWebApplicationFactory>
{
   private readonly HttpClient _client;
   private readonly CustomWebApplicationFactory _factory;
 
   public BoardsControllerTests(CustomWebApplicationFactory factory)
   {
       _factory = factory;
       _client = factory.CreateClient();
   }




   [Fact]
   public async Task GetBoards_ShouldReturnOk()
   {
       var response = await _client.GetAsync("/api/boards");
      
       Assert.Equal(HttpStatusCode.OK, response.StatusCode);
   }
  
   [Fact]
   public async Task CreateBoard_ShouldReturnCreated()
   {
       var board = new
       {
           name = Guid.NewGuid().ToString(),
           description = "Test board"
       };


       var response = await _client.PostAsJsonAsync("/api/boards", board);


       Assert.Equal(HttpStatusCode.Created, response.StatusCode);
   }
  
   [Fact]
   public async Task GetBoardTasks_ShouldReturnOk()
   {
       var board = new { name = Guid.NewGuid().ToString(), description = "Test board" };
       var createResponse = await _client.PostAsJsonAsync("/api/boards", board);
       var createdBoard = await createResponse.Content.ReadFromJsonAsync<BoardResponseDto>();
      
       var response = await _client.GetAsync($"/api/boards/{createdBoard!.Id}/tasks");
       Assert.Equal(HttpStatusCode.OK, response.StatusCode);
   }
  
   [Fact]
   public async Task CreateTask_OnBoard_ShouldReturnCreated()
   {
       var board = new { name = Guid.NewGuid().ToString(), description = "Test board" };


       var boardResponse = await _client.PostAsJsonAsync("/api/boards", board);
       var createdBoard = await boardResponse.Content.ReadFromJsonAsync<BoardResponseDto>();


       var boardId = createdBoard!.Id;


       var scope = _factory.Services.CreateScope();
       var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();


       var assigneeId = db.Assignees.OrderBy(x => Guid.NewGuid()).First().Id;


       var task = new
       {
           title = "Task 1",
           description = "Test task",
           dueDate = DateTime.UtcNow.AddDays(2),
           priority = Priority.Medium,
           status = Status.Todo,
           assigneeId = assigneeId
       };


       var response = await _client.PostAsJsonAsync($"/api/boards/{boardId}/tasks", task);


       Assert.Equal(HttpStatusCode.Created, response.StatusCode);
   }
}
public class BoardResponseDto
{
   public Guid Id { get; set; }
}

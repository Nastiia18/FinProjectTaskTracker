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
   public async Task GetBoards_ShouldReturnOkAndDataAsync()
   {
       var response = await _client.GetAsync("/api/boards");

       Assert.Equal(HttpStatusCode.OK, response.StatusCode);

       var boards = await response.Content.ReadFromJsonAsync<List<BoardResponseDto>>();

       Assert.NotNull(boards);
       Assert.NotEmpty(boards);
   }
  
   [Fact]
   public async Task CreateBoard_ShouldReturnCreated_WithValidDataAsync()
   {
       var board = new
       {
           name = Guid.NewGuid().ToString(),
           description = "Test board"
       };

       var response = await _client.PostAsJsonAsync("/api/boards", board);

       Assert.Equal(HttpStatusCode.Created, response.StatusCode);

       var created = await response.Content.ReadFromJsonAsync<BoardResponseDto>();

       Assert.NotNull(created);
       Assert.NotEqual(Guid.Empty, created.Id);
   }
  
   [Fact]
   public async Task GetBoardTasks_ShouldReturnOkAsync()
   {
       var board = new { name = Guid.NewGuid().ToString(), description = "Test board" };
       var createResponse = await _client.PostAsJsonAsync("/api/boards", board);

       var createdBoard = await createResponse.Content.ReadFromJsonAsync<BoardResponseDto>();

       var response = await _client.GetAsync($"/api/boards/{createdBoard!.Id}/tasks");

       Assert.Equal(HttpStatusCode.OK, response.StatusCode);

       var tasks = await response.Content.ReadFromJsonAsync<List<TaskResponseDto>>();

       Assert.NotNull(tasks);
   }
  
   [Fact]
   public async Task CreateTask_OnBoard_ShouldReturnCreated_AndBeRetrievableAsync()
   {
       var board = new { name = Guid.NewGuid().ToString(), description = "Test board" };

       var boardResponse = await _client.PostAsJsonAsync("/api/boards", board);
       var createdBoard = await boardResponse.Content.ReadFromJsonAsync<BoardResponseDto>();

       var scope = _factory.Services.CreateScope();
       var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

       var assigneeId = db.Assignees.First().Id;

       var task = new
       {
           title = "Task 1",
           description = "Test task",
           dueDate = DateTime.UtcNow.AddDays(2),
           priority = Priority.Medium,
           status = Status.Todo,
           assigneeId
       };
       
       var response = await _client.PostAsJsonAsync(
           $"/api/boards/{createdBoard!.Id}/tasks",
           task);
       
       Assert.Equal(HttpStatusCode.Created, response.StatusCode);
       
       var createdTask = await response.Content.ReadFromJsonAsync<TaskResponseDto>();

       Assert.NotNull(createdTask);
       Assert.Equal("Task 1", createdTask!.Title);
   }
   
   //Фільтрація
   [Fact]
   public async Task GetBoardTasks_FilterByStatus_ShouldReturnOnlyMatchingTasks()
   {
       var board = new { name = Guid.NewGuid().ToString(), description = "Test" };

       var boardResponse = await _client.PostAsJsonAsync("/api/boards", board);
       var createdBoard = await boardResponse.Content.ReadFromJsonAsync<BoardResponseDto>();

       var response = await _client.GetAsync(
           $"/api/boards/{createdBoard!.Id}/tasks?status=Todo");

       Assert.Equal(HttpStatusCode.OK, response.StatusCode);

       var tasks = await response.Content
           .ReadFromJsonAsync<List<TaskResponseDto>>();

       Assert.NotNull(tasks);

       Assert.All(tasks, t =>
       {
           Assert.Equal(Status.Todo, t.Status);
       });
   }
   
   [Fact]
   public async Task GetBoardTasks_FilterByPriority_ShouldReturnOnlyMatchingTasks()
   {
       var board = new
       {
           name = Guid.NewGuid().ToString(),
           description = "Test board"
       };

       var boardResponse = await _client.PostAsJsonAsync("/api/boards", board);
       var createdBoard = await boardResponse.Content.ReadFromJsonAsync<BoardResponseDto>();
       
       var response = await _client.GetAsync(
           $"/api/boards/{createdBoard!.Id}/tasks?priority=High");
       
       Assert.Equal(HttpStatusCode.OK, response.StatusCode);

       var tasks = await response.Content
           .ReadFromJsonAsync<List<TaskResponseDto>>();

       Assert.NotNull(tasks);

       Assert.All(tasks, t => Assert.Equal(Priority.High, t.Priority));
   }
   
}
public class BoardResponseDto
{
   public Guid Id { get; set; }
}
public class TaskResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public DateTime DueDate { get; set; }
    
    public Status Status { get; set; }
    public Priority Priority { get; set; }
}
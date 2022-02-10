using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
var app = builder.Build();

// app home route
app.MapGet("/", () => "Hello World");

// fetch all todo items from TodoDatabase
app.MapGet("/todoitems", async (TodoDb db) =>
    await db.Todos.Select(x => new TodoItemDTO(x)).ToListAsync()); // select and fetch todoitems of the child data class in an asynchorous manner

// fetch todo item by id
app.MapGet("/todoitems/{id}", async (int id, TodoDb db) =>
    await db.Todos.FindAsync(id) // search for todo item with matching id
        is Todo todo
            ? Results.Ok(new TodoItemDTO(todo)) // if found return the matching todoItem
            : Results.NotFound()); // if not found return "not found"

// add new todo item in the child data class format to the TodoDatabase
app.MapPost("/todoitems", async (TodoItemDTO todoItemDTO, TodoDb db) =>
{
    var todoItem = new Todo
    {
        IsComplete = todoItemDTO.IsComplete, //fetch user input todoItem IsComplete data from views
        Name = todoItemDTO.Name //fetch user input todoItem Name data from views
    };

    db.Todos.Add(todoItem); // add user input todoItem data to TodoDatabase
    await db.SaveChangesAsync(); //save changes made to Tododatabase

    // return the result of the Http request status code
    return Results.Created($"/todoitems/{todoItem.Id}", new TodoItemDTO(todoItem));
});

// Modify a todoItem existing on the TodoDatabase using the child data class model
app.MapPut("/todoitems/{id}", async (int id, TodoItemDTO todoItemDTO, TodoDb db) =>
{
    var todo = await db.Todos.FindAsync(id); // find the existing todoItem by id

    if (todo is null) return Results.NotFound(); // if no match is found return not found as the result of the Http request

    todo.Name = todoItemDTO.Name; // for match found update todoItems name field
    todo.IsComplete = todoItemDTO.IsComplete; // for match found update todoItems IsComplete status

    await db.SaveChangesAsync(); // Save changes to TodoDatabase in an Asynchronous manner

    return Results.NoContent(); // Clear out the user display after the update http request has complete
});

// delete an existing todoItem from the TodoDatabase
app.MapDelete("/todoitems/{id}", async (int id, TodoDb db) =>
{
    if (await db.Todos.FindAsync(id) is Todo todo) // search for matching todoItem with id in an Asynchronous manner
    {
        db.Todos.Remove(todo); // if match is found delete todoItem
        await db.SaveChangesAsync(); // save changes to TodoDatabase in an Asychronous manner
        return Results.Ok(new TodoItemDTO(todo));  // return the result of the Http request status code 200
    }

    return Results.NotFound();  // return the result of the Http request status code 404
});

app.Run(); // start the app on the browser

// Main/ Parent Data Class Model
public class Todo
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsComplete { get; set; }
    public string? Secret { get; set; }
}

// Partial/ Child Data Class Model
public class TodoItemDTO
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsComplete { get; set; }

    public TodoItemDTO() { }
    public TodoItemDTO(Todo todoItem) =>
    (Id, Name, IsComplete) = (todoItem.Id, todoItem.Name, todoItem.IsComplete);
}


// TodoApi App Database
class TodoDb : DbContext
{
    public TodoDb(DbContextOptions<TodoDb> options) //TodoDb Constructor(TypeCast<TodoDb> to have options parameters/ or receive options argument which is inherited from DBContext Options)
        : base(options) { }

    public DbSet<Todo> Todos => Set<Todo>(); // what does this mean? I'd probably get the answer from studying Entity FrameWork Core
}
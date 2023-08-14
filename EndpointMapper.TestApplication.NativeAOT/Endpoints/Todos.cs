using Microsoft.AspNetCore.Http.HttpResults;

namespace EndpointMapper.TestApplication.NativeAOT.Endpoints;

public static class TodoSingleton
{
    public static readonly Todo[] Todos = new Todo[] {
    new(1, "Walk the dog"),
    new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
    new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
    new(4, "Clean the bathroom"),
    new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
};
}

public class GetTodosEndpoint : IEndpoint
{
    [HttpMap(HttpMapMethod.Get, "/")]
    public static Todo[]? GetTodos()
    {
        return TodoSingleton.Todos;
    }
}

public class GetTodoEndpoint : IEndpoint
{
    [HttpMap(HttpMapMethod.Get, "/{id}")]
    public static Results<Ok<Todo>, NotFound> GetTodo(int id)
    {
        return TodoSingleton.Todos.FirstOrDefault(a => a.Id == id) is Todo todo
            ? TypedResults.Ok(todo)
            : TypedResults.NotFound();
    }
}

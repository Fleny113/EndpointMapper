namespace EndpointMapper.TestApplication.NativeAOT.Endpoints;

public class Todos : IEndpoint
{
    [HttpMap(HttpMapMethod.Get, "/todos", "/maybe"), HttpMap(HttpMapMethod.Trace, "/opts")]
    public static Todo[]? NoTodos()
    {
        return new Todo[] {
            new(1, "Walk the dog"),
            new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
            new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
            new(4, "Clean the bathroom"),
            new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
        };
    }
}

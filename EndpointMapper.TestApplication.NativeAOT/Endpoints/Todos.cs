namespace EndpointMapper.TestApplication.NativeAOT.Endpoints;

public class Todos : IEndpoint
{
    private readonly string _idkSomeValue;

    public Todos(string idkSomeValue)
    {
        _idkSomeValue = idkSomeValue;
    }

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

    [HttpMap(HttpMapMethod.Post, "/idk")]
    public string Hi()
    {
        return "Hi! I'm being mapped by the source generator!!!";
    }

    [HttpMap(HttpMapMethod.Post, "/idk2")]
    public string Hi2()
    {
        return $"Hi! I'm an Instance Method and the _idkSomeValue has value: {_idkSomeValue}";
    }
}

public class Test : IEndpoint
{
    [HttpMap(HttpMapMethod.Get, "/hi")]
    public string Hi()
    {
        return "Hi from the Test class!";
    }
}

namespace CartHost.Marten.Domain.Hello;

public class Hello
{
    public string Id { get; set; } = "Hello, World!";
    
    public void Apply(E.HelloCreated e)
    {
        Id = e.Id;
    }
    
    public static class E
    {
        public record HelloCreated(string Id);
    }
}
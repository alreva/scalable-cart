namespace SharedMessages;

public record Topic(string Name)
{
    public static implicit operator string(Topic topic) => topic.Name;
}
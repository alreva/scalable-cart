namespace SharedMessages;

public readonly struct Paging(int skip = 0, int take = 10)
{
    public int Skip { get; } = skip;
    public int Take { get; } = take;
}
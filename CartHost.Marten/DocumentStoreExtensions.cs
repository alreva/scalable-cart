using Marten;

namespace CartHost.Marten;

public static class DocumentStoreExtensions
{
    public static async Task Upsert<T>(
        IDocumentStore store,
        IQuerySession querySession,
        string id,
        params object[] events) where T : class
    {
        var aggregate = await querySession.Events.AggregateStreamAsync<T>(id);
        if (aggregate is null)
        {
            await using var session = store.LightweightSession();
            session.Events.StartStream<T>(id, events);
            await session.SaveChangesAsync();
        }
        else
        {
            await using var session = store.LightweightSession();
            session.Events.Append(id, events);
            await session.SaveChangesAsync();
        }
    }
}
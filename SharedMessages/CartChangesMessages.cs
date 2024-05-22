namespace SharedMessages;

public static class CartChangesMessages
{
    public record ClientCartConnected(string ConnectionId, int CartId);
    public record ClientCartDisconnected(string ConnectionId);
}
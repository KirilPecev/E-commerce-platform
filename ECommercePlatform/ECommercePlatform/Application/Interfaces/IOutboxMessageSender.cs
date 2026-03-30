namespace ECommercePlatform.Application.Interfaces
{
    public interface IOutboxMessageSender
    {
        Task SendAsync(object message, Type messageType, CancellationToken cancellationToken);
    }
}

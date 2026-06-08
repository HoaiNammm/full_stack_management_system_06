namespace TaskService.Events
{
    public interface IEventPublisher
    {
        System.Threading.Tasks.Task PublishAsync<T>(string eventType, T payload);
    }
}

namespace Mango.Services.OrderAPI.RabbitMQSender
{
    public interface IRabbitMQOrderMessageSender
    {
       Task SendMessageAsync(object message, string exchangeName);
    }
}   

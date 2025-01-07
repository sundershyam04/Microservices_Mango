namespace Mango.Services.AuthAPI.RabbitMQSender
{
    public interface IRabbitMQAuthMessageSender
    {
       Task SendMessageAsync(object message, string queueName);
    }
}   

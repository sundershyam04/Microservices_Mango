namespace Mango.Services.ShoppingCartAPI.RabbitMQSender
{
    public interface IRabbitMQCartMessageSender
    {
       Task SendMessageAsync(object message, string queueName);
    }
}   

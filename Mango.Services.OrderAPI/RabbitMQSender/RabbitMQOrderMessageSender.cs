using Mango.Services.OrderAPI.RabbitMQSender;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace Mango.Services.ShoppingCartAPI.RabbitMQSender
{
    public class RabbitMQOrderMessageSender : IRabbitMQOrderMessageSender
    {
        private readonly string _hostName;
        private readonly string _userName;
        private readonly string _password;
        private IConnection _connection;

        public RabbitMQOrderMessageSender()
        {
            _hostName = "localhost";
            _userName = "guest";
            _password = "guest";
        }
        public async Task SendMessageAsync(object message, string exchangeName)
        {
            if( await ConnectionExists())
            {
                using var channel = await _connection.CreateChannelAsync();
                await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Fanout, false);
                // Serialize message to be sent to RabbitMQ
                var json = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(json);

                await channel.BasicPublishAsync(
                    exchange: exchangeName,
                    routingKey: "",
                    body: body
                );
            }
        }
        private async Task CreateConnection()
        {
            var factory = new ConnectionFactory()
            {
                HostName = _hostName,
                UserName = _userName,
                Password = _password
            };
            _connection = await factory.CreateConnectionAsync();
        }

        private async Task<bool> ConnectionExists()
        {
            if(_connection != null)
            {
                return true;
            }
            await CreateConnection();
            return true;
        }
    }
}

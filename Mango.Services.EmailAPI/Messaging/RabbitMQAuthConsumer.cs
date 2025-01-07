
using Mango.Services.EmailAPI.Service;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Mango.Services.EmailAPI.Messaging
{
    public class RabbitMQAuthConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;
        private IConnection _connection;
        private IChannel _channel;


        public RabbitMQAuthConsumer(IConfiguration configuration,EmailService emailService)
        {
            _configuration = configuration;
            _emailService = emailService;           
        }
       
        protected override async Task<Task> ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory()
            {
                HostName = _configuration.GetValue<string>("RabbitMQ:HostName"),
                UserName = _configuration.GetValue<string>("RabbitMQ:UserName"),
                Password = _configuration.GetValue<string>("RabbitMQ:Password")
            };
            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
            await _channel.QueueDeclareAsync(
                queue:_configuration.GetValue<string>("TopicAndQueueNames:EmailRegisterUserQueue"),
                durable:true,
                exclusive:false,
                autoDelete:false,
                arguments:null);

            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async(ch, ea) =>
            {
                var content = Encoding.UTF8.GetString(ea.Body.ToArray());
                string email = JsonConvert.DeserializeObject<string>(content);
                HandleMessage(email).GetAwaiter().GetResult();
                await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            string consumerTag = _channel.BasicConsumeAsync(
                queue: _configuration.GetValue<string>("TopicAndQueueNames:EmailRegisterUserQueue"),
                autoAck: false,
                consumer: consumer
              ).GetAwaiter().GetResult();

            return Task.CompletedTask;
        }

        private async Task HandleMessage(string email)
        {
            await _emailService.EmailAndLogRegisterUser(email);
        }
    }
}

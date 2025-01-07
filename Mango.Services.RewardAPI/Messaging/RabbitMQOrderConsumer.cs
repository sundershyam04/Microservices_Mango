
using Mango.Services.RewardAPI.Message;
using Mango.Services.RewardAPI.Models;
using Mango.Services.RewardAPI.Service;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Mango.Services.RewardAPI.Messaging
{
    public class RabbitMQOrderConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IRewardService _rewardService;
        private IConnection _connection;
        private IChannel _channel;


        public RabbitMQOrderConsumer(IConfiguration configuration,IRewardService rewardService)
        {
            _configuration = configuration;
            _rewardService = rewardService;           
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
            await _channel.ExchangeDeclareAsync(
                exchange: _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic"),
                ExchangeType.Fanout,
                false
            );
            // Get queue name from default Q generated for channel
            var queueDeclareOk = await _channel.QueueDeclareAsync("rewards-order-queue");
            string queueName = queueDeclareOk.QueueName;
            
                //_channel.CurrentQueue;
            // ! Bind queue --> exchange!
            await _channel.QueueBindAsync(queueName, _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic"), "");

            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async(ch, ea) =>
            {
                var content = Encoding.UTF8.GetString(ea.Body.ToArray());
                var rewardsMessage = JsonConvert.DeserializeObject<RewardsMessage>(content);
                HandleMessage(rewardsMessage).GetAwaiter().GetResult();
                await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            string consumerTag = _channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: false,
                consumer: consumer
              ).GetAwaiter().GetResult();

            return Task.CompletedTask;
        }

        private async Task HandleMessage(RewardsMessage reward)
        {
            await _rewardService.RewardsUpdate(reward);
        }
    }
}

using Azure.Messaging.ServiceBus;
using Mango.Services.RewardAPI.Message;
using Mango.Services.RewardAPI.Service;
using Newtonsoft.Json;
using System.Text;
using System.Text.Json.Serialization;

namespace Mango.Services.RewardAPI.Messaging
{
    public class AzureServiceBusConsumer: IAzureServiceBusConsumer
    {
        private readonly string serviceBusConnectionString;
        private readonly string orderCreatedTopic;
        private readonly string orderCreatedRewardsSub;
        private readonly IConfiguration _configuration;
        private readonly ServiceBusProcessor _rewardsProcessor;
        private readonly ServiceBusProcessor _emailRegisterUserProcessor;
        private readonly IRewardService _rewardService;
        public AzureServiceBusConsumer(IConfiguration configuration, IRewardService rewardService)
        {
            _configuration = configuration;
            serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");
            orderCreatedTopic = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic");
            orderCreatedRewardsSub = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreated_Rewards_Subscription");
            var client = new ServiceBusClient(serviceBusConnectionString);
            _rewardsProcessor = client.CreateProcessor(orderCreatedTopic, orderCreatedRewardsSub);
            _rewardService = rewardService;
        }

        public async Task Start()
        {
            _rewardsProcessor.ProcessMessageAsync += OnRewardsRequestReceived;
            _rewardsProcessor.ProcessErrorAsync += ErrorHandler;
            await _rewardsProcessor.StartProcessingAsync();
        }
        
        public async Task Stop()
        {         
            await _rewardsProcessor.StopProcessingAsync();      
            await _rewardsProcessor.DisposeAsync();     
        }
        private async Task OnRewardsRequestReceived(ProcessMessageEventArgs args)
        {
            // log message and email details in db
            var message = args.Message;
            string json = Encoding.UTF8.GetString(message.Body);
            RewardsMessage? objMsg = JsonConvert.DeserializeObject<RewardsMessage>(json);
            try
            {
                await _rewardService.RewardsUpdate(objMsg);
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
      
       private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }
                
    }
}

using Azure.Messaging.ServiceBus;
using Mango.Services.EmailAPI.Models.Dto;
using Mango.Services.EmailAPI.Service;
using Newtonsoft.Json;
using System.Text;
using System.Text.Json.Serialization;

namespace Mango.Services.EmailAPI.Messaging
{
    public class AzureServiceBusConsumer: IAzureServiceBusConsumer
    {
        private readonly string serviceBusConnectionString;
        private readonly string emailCartQueue;
        private readonly string emailRegisterUserQueue;
        private readonly IConfiguration _configuration;
        private readonly ServiceBusProcessor _emailCartProcessor;
        private readonly ServiceBusProcessor _emailRegisterUserProcessor;
        private readonly EmailService _emailService;
        public AzureServiceBusConsumer(IConfiguration configuration, EmailService emailService)
        {
            _configuration = configuration;
            serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");
            emailCartQueue = _configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCartQueue");
            emailRegisterUserQueue = _configuration.GetValue<string>("TopicAndQueueNames:EmailRegisterUserQueue");

            var client = new ServiceBusClient(serviceBusConnectionString);
            _emailCartProcessor = client.CreateProcessor(emailCartQueue);
            _emailRegisterUserProcessor = client.CreateProcessor(emailRegisterUserQueue);
            _emailService = emailService;
        }

        public async Task Start()
        {
            _emailCartProcessor.ProcessMessageAsync += OnEmailCartRequestReceived;
            _emailCartProcessor.ProcessErrorAsync += ErrorHandler;
            await _emailCartProcessor.StartProcessingAsync();

            _emailRegisterUserProcessor.ProcessMessageAsync += OnUserRegisterRequestReceived;
            _emailRegisterUserProcessor.ProcessErrorAsync += ErrorHandler;                   
            await _emailRegisterUserProcessor.StartProcessingAsync();           
        }

       
        public async Task Stop()
        {         
            await _emailCartProcessor.StopProcessingAsync();      
            await _emailCartProcessor.DisposeAsync();

            await _emailRegisterUserProcessor.StopProcessingAsync();
            await _emailRegisterUserProcessor.DisposeAsync();
        }
        private async Task OnEmailCartRequestReceived(ProcessMessageEventArgs args)
        {
            // log message and email details in db
            var message = args.Message;
            string json = Encoding.UTF8.GetString(message.Body);
            CartDto objMsg = JsonConvert.DeserializeObject<CartDto>(json);
            try
            {
                await _emailService.EmailAndLogCart(objMsg);
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        private async Task OnUserRegisterRequestReceived(ProcessMessageEventArgs args)
        {
            var message = args.Message;
            var emailmsg = Encoding.UTF8.GetString(message.Body);
            try
            {
                await _emailService.EmailAndLogRegisterUser(emailmsg);
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

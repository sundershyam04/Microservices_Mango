using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Mango.Messaging
{
    public class MessageBus : IMessageBus
    {
        string sb_connectionString = "Endpoint=sb://sb-mangomicroservices.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=zg2m28HgKjTGkoAjv/dxnxTNMBe4vuFBX+ASbAcPwSw=";
           
        public async Task PublishMessage(object message, string topic_queue_Name)
        {
            var client = new ServiceBusClient(sb_connectionString);
            ServiceBusSender sender = client.CreateSender(topic_queue_Name);
            string messagestr = JsonConvert.SerializeObject(message);
            ServiceBusMessage finalmsg = new ServiceBusMessage(Encoding.UTF8.GetBytes(messagestr))
            {
                CorrelationId = Guid.NewGuid().ToString()
            };
            await sender.SendMessageAsync(finalmsg);
            await client.DisposeAsync();
        }
    }
}

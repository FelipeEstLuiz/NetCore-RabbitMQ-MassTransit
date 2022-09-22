using MassTransit;
using RabbitMQ.MassTransit.Messages;
using System.Threading.Tasks;

namespace RabbitMQ.MassTransit.Service
{
    internal class MessageConsumer : IConsumer<IMessage>
    {
        public Task Consume(ConsumeContext<IMessage> context)
        {
            SendMessageApi sendMessageApi = new();
            sendMessageApi.SendMessage(context.Message.Message).Wait();
            return Task.CompletedTask;
        }
    }
}

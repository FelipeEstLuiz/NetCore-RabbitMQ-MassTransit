using System.Threading.Tasks;

namespace RabbitMQ.MassTransit.Service
{
    public interface ISendMessage
    {
        public Task SendMessage(string message);
    }
}

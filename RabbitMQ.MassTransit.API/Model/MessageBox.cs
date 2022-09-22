using System;

namespace RabbitMQ.MassTransit.API.Model
{
    public class MessageBox
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public DateTime ReceivingDate { get; set; }
    }
}

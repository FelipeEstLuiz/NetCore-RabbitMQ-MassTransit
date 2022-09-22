using Polly;
using RestSharp;
using System;
using System.Net;
using System.Threading.Tasks;

namespace RabbitMQ.MassTransit.Service
{
    public class SendMessageApi
    {
        public async Task SendMessage(string message)
        {
            string messageApi = Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                Message = message,
                ReceivingDate = DateTime.Now
            });

            IRestResponse response = null;

            await Policy
                .Handle<Exception>()
                .RetryAsync(3, async (exception, retryCount) =>
                {
                    await Task.Delay(300).ConfigureAwait(false);
                })
                .ExecuteAsync(async () =>
                {
                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                    RestClient client = new(@"https://localhost:44300/api/message/masstransit");
                    RestRequest request = new(Method.POST);
                    request.RequestFormat = DataFormat.Json;
                    request.AddJsonBody(messageApi);
                    response = await client.ExecuteAsync(request);
                })
                .ConfigureAwait(false);
        }
    }
}

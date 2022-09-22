using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace RabbitMQ.MassTransit.API.Model
{
    public static class Initialize
    {
        public static IServiceCollection MassTransit(
           this IServiceCollection services,
           IConfiguration configuration
        )
        {
            IConfigurationSection config = configuration.GetSection("MassTransit");

            if (config == null)
                throw new ArgumentException("The configuration was not available for Masstransit");

            MassTransitCfg massConfig = new();
            config.Bind(massConfig);

            services.AddMassTransit(builder =>
            {
                builder.AddBus(busBuilder =>
                {
                    IBusControl bus = Bus.Factory.CreateUsingRabbitMq(cfg =>
                    {
                        cfg.Host(massConfig.HostName, massConfig.Port, massConfig.VirtualHost, h =>
                        {
                            h.Username(massConfig.User);
                            h.Password(massConfig.Password);
                            h.Heartbeat(massConfig.HeartBeat);
                        });
                    });
                    return bus;
                });
            });
            services.AddMassTransitHostedService(true);

            return services;
        }
    }

    internal class MassTransitCfg
    {
        public string HostName { get; set; }
        public ushort Port { get; set; }
        public string VirtualHost { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public ushort HeartBeat { get; set; }
    }
}

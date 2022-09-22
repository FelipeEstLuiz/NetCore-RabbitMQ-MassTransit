using GreenPipes;
using MassTransit;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace RabbitMQ.MassTransit.Service
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

            MasstransitCfg massConfig = new();

            config.Bind(massConfig);

            services.AddMassTransit(builder =>
            {
                builder.AddConsumer<MessageConsumer>();

                builder.AddBus(busBuilder =>
                {
                    IBusControl bus = Bus.Factory.CreateUsingRabbitMq(cfg =>
                    {
                        ConfigurarHost(cfg, massConfig);
                        ConfigurarEndpoint(cfg, massConfig, busBuilder);
                    });
                    return bus;
                });
            });
            services.AddMassTransitHostedService();
            return services;
        }

        private static void ConfigurarEndpoint(
            IRabbitMqBusFactoryConfigurator cfg,
            MasstransitCfg massConfig,
            IBusRegistrationContext busBuilder
        )
        {
            cfg.ReceiveEndpoint(massConfig.EndpointName, receiveEnpointCfg =>
            {
                receiveEnpointCfg.ConcurrentMessageLimit = massConfig.ConcurrentProcess;
                receiveEnpointCfg.Durable = true;
                receiveEnpointCfg.UseJsonSerializer();

                if (massConfig.ImmediateRetries > 0)
                    receiveEnpointCfg.UseMessageRetry(r => r.Immediate(massConfig.ImmediateRetries));

                if (massConfig.DelayedRetries > 0)
                    receiveEnpointCfg.UseDelayedRedelivery(r => r.Incremental(
                        massConfig.DelayedRetries,
                        TimeSpan.FromSeconds(massConfig.DelayedRetriesInSeconds),
                        TimeSpan.FromSeconds(massConfig.DelayedRetriesInSeconds)));

                receiveEnpointCfg.UseInMemoryOutbox(c => c.ConcurrentMessageDelivery = true);

                receiveEnpointCfg.ConfigureConsumer<MessageConsumer>(busBuilder);
            });

            cfg.ConfigureEndpoints(busBuilder);
        }

        private static void ConfigurarHost(IRabbitMqBusFactoryConfigurator cfg, MasstransitCfg massConfig)
        {
            cfg.Host(massConfig.HostName, massConfig.Port, massConfig.VirtualHost, hostConfig =>
            {
                hostConfig.Username(massConfig.User);
                hostConfig.Password(massConfig.Password);
                hostConfig.Heartbeat(massConfig.HeartBeat);
                hostConfig.ConfigureBatchPublish(bcfg =>
                {
                    bcfg.Enabled = true;
                    bcfg.MessageLimit = 100;
                    bcfg.SizeLimit = 200000;
                    cfg.PrefetchCount = 100;
                    bcfg.Timeout = TimeSpan.FromMilliseconds(3);
                });
                hostConfig.PublisherConfirmation = false;
            });
        }
    }

    internal class MasstransitCfg
    {
        public string EndpointName { get; set; }
        public string HostName { get; set; }
        public ushort Port { get; set; }
        public string VirtualHost { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public int ConcurrentProcess { get; set; }
        public ushort HeartBeat { get; set; }
        public int ImmediateRetries { get; set; }
        public int DelayedRetries { get; set; }
        public int DelayedRetriesInSeconds { get; set; }
    }
}

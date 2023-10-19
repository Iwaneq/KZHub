using AutoMapper;
using KZHub.CardStoringService.DTOs;
using KZHub.CardStoringService.Models;
using KZHub.CardStoringService.Repositories;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Text;
using System.Text.Json;

namespace KZHub.CardStoringService.AsyncDataServices
{
    public class MessageBusSubscriber : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;
        private IConnection? _connection;
        private IModel? _channel;

        #region Setup / Constructor

        public MessageBusSubscriber(IConfiguration configuration, IServiceScopeFactory scopeFactory)
        {
            _configuration = configuration;
            _scopeFactory = scopeFactory;

            InitializeRabbitMQ();
        }

        private void InitializeRabbitMQ()
        {
            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQHost"],
                Port = int.Parse(_configuration["RabbitMQPort"]!)
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "cardStorage_queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            Console.WriteLine("--> Listening on the RabbitMQ");

            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
        }

        private void RabbitMQ_ConnectionShutdown(object? sender, ShutdownEventArgs e)
        {
            Console.WriteLine("--> Connection was shutted down");
        }


        #endregion

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_channel is null) throw new ChannelAllocationException();

            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (ModuleHandle, ea) =>
            {
                var state = CallSaveCardProcessor(ea);

                SendSaveStateBackToWebClient(state, ea);
            };

            _channel.BasicConsume(queue: "cardStorage_queue", autoAck: false, consumer: consumer);

            return Task.CompletedTask;
        }

        private SaveCardStateDTO CallSaveCardProcessor(BasicDeliverEventArgs ea)
        {
            using(var scope = _scopeFactory.CreateScope())
            {
                return scope.ServiceProvider.GetRequiredService<ISaveCardProcessor>().SaveCard(ea);
            }
        }

        private void SendSaveStateBackToWebClient(SaveCardStateDTO state, BasicDeliverEventArgs ea)
        {
            var props = ea.BasicProperties;
            var replyProps = _channel!.CreateBasicProperties();
            replyProps.CorrelationId = props.CorrelationId;

            var dataJson = JsonSerializer.Serialize(state);
            var data = Encoding.UTF8.GetBytes(dataJson);

            _channel!.BasicPublish(exchange: string.Empty,
                routingKey: props.ReplyTo,
                mandatory: false,
                basicProperties: replyProps,
                body: data);
            _channel!.BasicAck(deliveryTag: ea.DeliveryTag, multiple: true);
        }

        #region IDisposable

        public override void Dispose()
        {
            if (_channel != null && _channel.IsOpen)
            {
                _channel.Close();
                _connection?.Close();
            }

            base.Dispose();
        } 

        #endregion
    }
}

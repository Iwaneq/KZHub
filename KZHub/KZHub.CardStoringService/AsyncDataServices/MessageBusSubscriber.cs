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

        private void SaveCard(BasicDeliverEventArgs ea)
        {
            if (_channel is null) throw new ChannelAllocationException();

            var body = ea.Body;
            var notificationMessage = Encoding.UTF8.GetString(body.ToArray());

            CreateCardDTO? cardDTO = JsonSerializer.Deserialize<CreateCardDTO>(notificationMessage);

            Console.WriteLine($"--> Consumer Reciving... {cardDTO}");
            if(cardDTO is not null)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var cardRepository = scope.ServiceProvider.GetRequiredService<ICardDataService>();
                    var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

                    try
                    {
                        var card = mapper.Map<Card>(cardDTO);
                        cardRepository.SaveCard(card);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"--> EXCEPTION WAS THROWN WHILE SAVING CARD: {ex.Message}");
                    }
                }
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (ModuleHandle, ea) =>
            {
                SaveCard(ea);
            };

            _channel.BasicConsume(queue: "cardStorage_queue", autoAck: false, consumer: consumer);

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            if(_channel != null && _channel.IsOpen)
            {
                _channel.Close();
                _connection?.Close();
            }

            base.Dispose();
        }
    }
}

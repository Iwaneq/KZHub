using KZHub.CardGenerationService.DTOs.Card;
using KZHub.CardGenerationService.Services.CardProcessing.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SkiaSharp;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace KZHub.CardGenerationService.AsyncDataServices
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
            var factory = new ConnectionFactory() { HostName = _configuration["RabbitMQHost"], Port = int.Parse(_configuration["RabbitMQPort"]!) };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "cardGeneration_queue",
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

        private void GenerateCardFromCreateCardDTO(BasicDeliverEventArgs ea)
        {
            if (_channel is null) throw new ChannelClosedException("Channel was closed");

            var props = ea.BasicProperties;
            var replyProps = _channel.CreateBasicProperties();
            replyProps.CorrelationId = props.CorrelationId;

            var body = ea.Body;
            var notificationMessage = Encoding.UTF8.GetString(body.ToArray());

            CreateCardDTO? cardDTO = JsonSerializer.Deserialize<CreateCardDTO>(notificationMessage);

            Console.WriteLine($"--> Consumer Reciving... {cardDTO}");
            if (cardDTO is not null)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var cardGenerator = scope.ServiceProvider.GetRequiredService<ICardGenerator>();
                    SKBitmap card;

                    try
                    {
                        card = cardGenerator.GenerateCard(cardDTO);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"--> EXCEPTION WAS THROWN WHILE GENERATING CARD: {ex.Message}");
                        throw;
                    }

                    Console.WriteLine("--> Consumer Received");

                    using (MemoryStream memStream = new MemoryStream())
                    using (SKManagedWStream wstream = new SKManagedWStream(memStream))
                    {
                        card.Encode(wstream, SKEncodedImageFormat.Png, 100);
                        byte[] data = memStream.ToArray();

                        _channel.BasicPublish(exchange: string.Empty,
                             routingKey: props.ReplyTo,
                             basicProperties: replyProps,
                             body: data);
                        _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: true);
                        Console.WriteLine("--> Card was sent to the Client");
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
                GenerateCardFromCreateCardDTO(ea);
            };

            _channel.BasicConsume(queue: "cardGeneration_queue", autoAck: false, consumer: consumer);

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            if (_channel != null && _channel.IsOpen)
            {
                _channel.Close();
                _connection?.Close();
            }

            base.Dispose();
        }
    }
}

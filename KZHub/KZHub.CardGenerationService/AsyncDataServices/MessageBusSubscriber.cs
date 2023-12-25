using KZHub.CardGenerationService.DTOs.Card;
using KZHub.CardGenerationService.Services.CardProcessing.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace KZHub.CardGenerationService.AsyncDataServices
{
    public class MessageBusSubscriber : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private IConnection? _connection;
        private IModel? _channel;

        private readonly IServiceScopeFactory _scopeFactory;


        #region Setup / Constructor 

        public MessageBusSubscriber(IConfiguration configuration, IServiceScopeFactory scopeFactory)
        {
            _configuration = configuration;
            _scopeFactory = scopeFactory;

            InitializeRabbitMQ();
        }

        private void InitializeRabbitMQ()
        {
            try{
                var factory = new ConnectionFactory() { HostName = _configuration["RabbitMQHost"], Port = int.Parse(_configuration["RabbitMQPort"]!) };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                _channel.QueueDeclare(queue: "cardGeneration_queue",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);
                _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                Console.WriteLine("--> Listening on the RabbitMQ...");

                _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"--> Error occured while initializing RabbitMQ: {ex.Message}");
            }
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
                byte[] data = CallCardGenerator(ea);

                SendDataBackToWebClient(data, ea);
            };

            _channel.BasicConsume(queue: "cardGeneration_queue", autoAck: false, consumer: consumer);

            return Task.CompletedTask;
        }

        private byte[] CallCardGenerator(BasicDeliverEventArgs ea)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                try
                {
                   return scope.ServiceProvider.GetRequiredService<ICardGenerationProcessor>().GenerateCardFromCreateCardDTO(ea);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"--> Exception was thrown while calling Card Generator: {ex.Message}");
                }

                return new byte[0];
            }
        }

        private void SendDataBackToWebClient(byte[] data, BasicDeliverEventArgs ea)
        {
            var props = ea.BasicProperties;
            var replyProps = _channel!.CreateBasicProperties();
            replyProps.CorrelationId = props.CorrelationId;

            _channel.BasicPublish(exchange: string.Empty,
                    routingKey: props.ReplyTo,
                    basicProperties: replyProps,
                    body: data);
            _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: true);
            Console.WriteLine("--> Card was sent to the Client");
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

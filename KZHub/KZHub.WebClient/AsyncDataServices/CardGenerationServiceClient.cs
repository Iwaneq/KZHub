using KZHub.WebClient.DTOs.Card;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace KZHub.WebClient.AsyncDataServices
{
    public class CardGenerationServiceClient : ICardGenerationServiceClient
    {
        private readonly IConfiguration _configuration;
        private readonly IConnection? _connection;
        private readonly IModel? _channel;
        private readonly string? _replyQueueName;

        private readonly ConcurrentDictionary<string, TaskCompletionSource<byte[]>> callbackMapper = new();

        #region Setup / Constructor

        public CardGenerationServiceClient(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQHost"],
                Port = int.Parse(_configuration["RabbitMQPort"]!)
            };

            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _replyQueueName = _channel.QueueDeclare().QueueName;
                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += (model, ea) =>
                {
                    if (!callbackMapper.TryRemove(ea.BasicProperties.CorrelationId, out var tcs))
                        return;

                    var body = ea.Body.ToArray();

                    tcs.TrySetResult(body);
                };

                _channel.BasicConsume(consumer: consumer,
                                      queue: _replyQueueName,
                                      autoAck: true);

                _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;

                Console.WriteLine("--> Connected Card Generation Service Client to RabbitMQ!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not connect Card Generation Service Client to RabbitMQ: {ex.Message}");
            }
        }
        private void RabbitMQ_ConnectionShutdown(object? sender, ShutdownEventArgs e)
        {
            Console.WriteLine("--> RabbitMQ Card Generation Service Connection was shutted down!");
        }

        #endregion

        public Task<byte[]> SendCardToGenerate(CreateCardDTO createCard, CancellationToken cancellationToken = default)
        {
            if (_channel is null) throw new ChannelAllocationException();

            IBasicProperties props = _channel.CreateBasicProperties();
            var correlationId = Guid.NewGuid().ToString();
            props.CorrelationId = correlationId;
            props.ReplyTo = _replyQueueName;
            var message = JsonSerializer.Serialize(createCard);

            var tcs = new TaskCompletionSource<byte[]>();
            callbackMapper.TryAdd(correlationId, tcs);

            if (_connection != null && _connection.IsOpen)
            {
                Console.WriteLine("--> Sending Card to CardGenerationService...");
                var body = Encoding.UTF8.GetBytes(message);

                _channel.BasicPublish(exchange: string.Empty,
                    routingKey: "cardGeneration_queue",
                    basicProperties: props,
                    body: body);

                cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out _));
                return tcs.Task;
            }
            throw new ConnectFailureException("Card Generation Service Connection was not open", null);
        }
    }
}

using KZHub.WebClient.DTOs.Card;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace KZHub.WebClient.AsyncDataServices
{
    public class CardStorageServiceClient : ICardStorageServiceClient
    {
        private readonly IConfiguration _configuration;
        private readonly IConnection? _connection;
        private readonly IModel? _channel;
        private readonly string? _replyQueueName;

        private readonly ConcurrentDictionary<string, TaskCompletionSource<SaveCardStateDTO>> callbackMapper = new();

        public CardStorageServiceClient(IConfiguration configuration)
        {
            _configuration = configuration;
            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQHost"],
                Port = int.Parse(configuration["RabbitMQPort"]!),
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

                    var dataJson = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var state = JsonSerializer.Deserialize<SaveCardStateDTO>(dataJson);

                    if(state == null) throw new NullReferenceException("Save Card State was null");

                    tcs.TrySetResult(state);
                };

                _channel.BasicConsume(consumer: consumer,
                    queue: _replyQueueName,
                    autoAck: true);

                _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;

                Console.WriteLine("--> Connected Card Storage Service Client to RabbitMQ!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not connect Card Storage Service Client to RabbitMQ: {ex.Message}");
            }
        }
        

        public Task<SaveCardStateDTO> SendCardToStorage(CreateCardDTO createCard, CancellationToken cancellationToken = default)
        {
            if (_channel is null) throw new ChannelAllocationException();

            IBasicProperties props = _channel.CreateBasicProperties();
            var correlationId = Guid.NewGuid().ToString();
            props.CorrelationId = correlationId;
            props.ReplyTo = _replyQueueName;

            var message = JsonSerializer.Serialize(createCard);

            var tcs = new TaskCompletionSource<SaveCardStateDTO>();
            callbackMapper.TryAdd(correlationId, tcs);

            if(_connection != null && _connection.IsOpen)
            {
                var body = Encoding.UTF8.GetBytes(message);

                _channel.BasicPublish(
                    exchange: string.Empty,
                    routingKey: "cardStorage_queue",
                    basicProperties: props,
                    body: body);

                cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out _));
                return tcs.Task;
            }
            throw new ConnectFailureException("Card Storage Service Connection was not open", null);
        }

        private void RabbitMQ_ConnectionShutdown(object? sender, ShutdownEventArgs e)
        {
            Console.WriteLine("--> RabbitMQ Card Storage Service Connection was shutted down!");
        }
    }
}

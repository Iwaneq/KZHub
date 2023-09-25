using KZHub.WebClient.DTOs.Card;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System.Text;
using System.Text.Json;

namespace KZHub.WebClient.AsyncDataServices
{
    public class CardStorageServiceClient : ICardStorageServiceClient
    {
        private readonly IConfiguration _configuration;
        private readonly IConnection? _connection;
        private readonly IModel? _channel;

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

                _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;

                Console.WriteLine("--> Connected Card Storage Service Client to RabbitMQ!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not connect Card Storage Service Client to RabbitMQ: {ex.Message}");
            }
        }
        

        public Task<bool> SendCardToStorage(CreateCardDTO createCard, CancellationToken cancellationToken = default)
        {
            if (_channel is null) throw new ChannelAllocationException();

            var message = JsonSerializer.Serialize(createCard);

            if(_connection != null && _connection.IsOpen)
            {
                var body = Encoding.UTF8.GetBytes(message);

                _channel.BasicPublish(
                    exchange: string.Empty,
                    routingKey: "cardStorage_queue",
                    body: body);

                return Task.FromResult(true);
            }
            else
            {
                throw new ConnectFailureException("Card Storage Service Connection was not open", null);
            }
        }

        private void RabbitMQ_ConnectionShutdown(object? sender, ShutdownEventArgs e)
        {
            Console.WriteLine("--> RabbitMQ Card Storage Service Connection was shutted down!");
        }
    }
}

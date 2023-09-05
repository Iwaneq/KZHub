using KZHub.WebClient.DTOs.Card;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SkiaSharp;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using static System.Net.Mime.MediaTypeNames;

namespace KZHub.WebClient.AsyncDataServices
{
    public class MessageBusClient : IMessageBusClient
    {
        private readonly IConfiguration _configuration;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _replyQueueName;

        public MessageBusClient(IConfiguration configuration)
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
                    var body = ea.Body.ToArray();
                    var bitmap = new SKBitmap();
                    var gcHandle = GCHandle.Alloc(body, GCHandleType.Pinned);

                    var info = new SKImageInfo(1164,1653, SKImageInfo.PlatformColorType, SKAlphaType.Unpremul);
                    bitmap.InstallPixels(info, gcHandle.AddrOfPinnedObject(), info.RowBytes, delegate { gcHandle.Free(); }, null);
                    using (var data = bitmap.Encode(SKEncodedImageFormat.Png, 80))
                    using (var stream = File.OpenWrite(Path.Combine(@"C:\data\KartaZbiorkiMaker\Karty", "1.png")))
                    {
                        // save the data to a stream
                        data.SaveTo(stream);
                    }

                    //DO SOMETHING WITH RESPONSE
                };

                _channel.BasicConsume(consumer: consumer,
                                      queue: _replyQueueName,
                                      autoAck: true);

                _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;

                Console.WriteLine("--> Connected to RabbitMQ");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not connect to RabbitMQ: {ex.Message}");
            }
        }

        public void SendCardToGenerate(CreateCardDTO createCard)
        {
            IBasicProperties props = _channel.CreateBasicProperties();
            var correlationId = Guid.NewGuid().ToString();
            props.CorrelationId = correlationId;
            props.ReplyTo = _replyQueueName;
            var message = JsonSerializer.Serialize(createCard);

            if(_connection.IsOpen)
            {
                var body = Encoding.UTF8.GetBytes(message);

                _channel.BasicPublish(exchange: string.Empty,
                    routingKey: "cardGeneration_queue",
                    basicProperties: props,
                    body: body);
            }
        }

        private void Dispose()
        {
            if (_channel.IsOpen)
            {
                _channel.Close();
                _connection.Close();
            }
        }

        private void RabbitMQ_ConnectionShutdown(object? sender, ShutdownEventArgs e)
        {
            Console.WriteLine("--> RabbitMQ Connection was shutted down!");
        }
    }
}

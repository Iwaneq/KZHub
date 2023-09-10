﻿using KZHub.WebClient.DTOs.Card;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace KZHub.WebClient.AsyncDataServices
{
    public class MessageBusClient : IMessageBusClient
    {
        private readonly IConfiguration _configuration;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _replyQueueName;

        private readonly ConcurrentDictionary<string, TaskCompletionSource<byte[]>> callbackMapper = new();

        public MessageBusClient(IConfiguration configuration, IServiceProvider serviceProvider)
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

                Console.WriteLine("--> Connected to RabbitMQ");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not connect to RabbitMQ: {ex.Message}");
            }
        }

        public Task<byte[]> SendCardToGenerate(CreateCardDTO createCard, CancellationToken cancellationToken = default)
        {
            IBasicProperties props = _channel.CreateBasicProperties();
            var correlationId = Guid.NewGuid().ToString();
            props.CorrelationId = correlationId;
            props.ReplyTo = _replyQueueName;
            var message = JsonSerializer.Serialize(createCard);
            var tcs = new TaskCompletionSource<byte[]>();
            callbackMapper.TryAdd(correlationId, tcs);

            if (_connection.IsOpen)
            {
                var body = Encoding.UTF8.GetBytes(message);

                _channel.BasicPublish(exchange: string.Empty,
                    routingKey: "cardGeneration_queue",
                    basicProperties: props,
                body: body);

                cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out _));
                return tcs.Task;
            }
            throw new ConnectFailureException("Connection was not open", null);
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

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PlatformsService.Dtos;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace PlatformsService.AsyncDataServices
{
    public class MessageBusClient : IMessageBusClient
    {
        private readonly IConfiguration _configuration;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public MessageBusClient(IConfiguration configuration)
        {
            _configuration = configuration;

            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQHost"],
                Port = int.Parse(_configuration["RabbitMQPort"])
            };

            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();


                try
                {
                    _channel.ExchangeDeclare("my_trigger", ExchangeType.Fanout, durable: true);
                }
                catch (Exception ex)
                {
                    // Handle exceptions (log, retry, etc.)
                    Console.WriteLine($"Error declaring exchange: {ex.Message}");
                }

                _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;

                Console.WriteLine("--> Connected to MessageBus");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not connect to the Message Bus: {ex.Message}");
            }
        }

        public void PublishNewPlatform(PlatformPublishedDto platformPublishedDto)
        {
            var message = JsonSerializer.Serialize(platformPublishedDto);

            if (_connection.IsOpen)
            {
                Console.WriteLine("--> RabbitMQ Connection Open, sending message...");
                SendMessage(message);
            }
            else
            {
                Console.WriteLine("--> RabbitMQ connection is closed, not sending");
            }

        }

        private void SendMessage(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true; // Make the message persistent

            _channel.QueueDeclare(queue: "my_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);

            _channel.BasicPublish(exchange: "my_trigger",
                            routingKey: "",
                            basicProperties: properties,
                            body: body);
            Console.WriteLine($"--> We have sent {message}");
        }

        public void Dispose()
        {
            Console.WriteLine("MessageBus Disposed");
            if (_channel.IsOpen)
            {
                _channel.Close();
                _connection.Close();
            }
        }

        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            Console.WriteLine("--> RabbitMQ Connection Shutdown");
        }
    }
}

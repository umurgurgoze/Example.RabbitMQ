using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;

namespace ExampleRabbitMQWeb.Watermark.Services
{
    //RabbitMQ bağlantı servisi
    public class RabbitMQClientService : IDisposable //Dispose olduğunda RabbitMQ ile ilgili bağlantıları kapatmak için alıyoruz.
    {
        private readonly ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _channel;
        public static string ExchangeName = "ImageDirectExchange"; //Exchance
        public static string RoutingWatermark = "watermark-route-image"; // Route
        public static string QueueName = "queue-watermark-image"; // Queue

        private readonly ILogger<RabbitMQClientService> _logger;
        public RabbitMQClientService(ConnectionFactory connectionFactory, ILogger<RabbitMQClientService> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
            Connect(); // İlk nesne örneği alındığında(DI) bağlantı direkt kurulsun.

        }
        public IModel Connect()
        {
            _connection = _connectionFactory.CreateConnection();
            if (_channel is { IsOpen: true })
            {
                return _channel;
            }
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(ExchangeName, type: "direct", true, false); // Exchange oluştur.
            _channel.QueueDeclare(QueueName, true, false, false, null); //Queue oluştur.
            _channel.QueueBind(exchange: ExchangeName, queue: QueueName, routingKey: RoutingWatermark);
            _logger.LogInformation("RabbitMQ ile bağlantı kuruldu...");
            return _channel;

        }

        public void Dispose()
        {
            _channel?.Close(); //Channel null değil ise kapatalım.
            _channel?.Dispose(); // Channel var ise dispose edelim.

            _connection?.Close();
            _connection.Dispose();

            _logger.LogInformation("RabbitMQ ile bağlantı koptu...");

        }
    }
}

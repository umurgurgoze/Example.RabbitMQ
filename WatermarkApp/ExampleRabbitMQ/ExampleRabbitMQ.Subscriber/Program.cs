using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace ExampleRabbitMQ.Subscriber
{
    public class Program
    {
        static void Main(string[] args)
        {

            var factory = new ConnectionFactory();
            factory.Uri = new Uri("amqps://haaaxiwv:KKR97TelzVm0anpxsjpV0dYkO-84wiK-@moose.rmq.cloudamqp.com/haaaxiwv");

            using var connection = factory.CreateConnection();

            var channel = connection.CreateModel();

            channel.ExchangeDeclare("header-exchange", durable: true, type: ExchangeType.Headers);

            channel.BasicQos(0, 1, false);
            var consumer = new EventingBasicConsumer(channel);
            var queueName = channel.QueueDeclare().QueueName;// Random kuyruk isimlerini oluşturuyoruz.

            Dictionary<string, object> headers = new Dictionary<string, object>();
            headers.Add("format", "pdf");
            headers.Add("shape", "a4");
            headers.Add("x-match", "any"); // Subscriber tarafına x-match ekliyoruz.
                                           // "All" format = pdf ve shape = a4, "Any" format = pdf veya shape = a4
                                           // Publisher ve subscriber headerları üzerindeki verileri karşılaştırıyor.Ona göre mesaj gönderiyor.

            channel.QueueBind(queueName, "header-exchange", string.Empty, headers);
            //Exchange oluşmadan subscriber çalışırsa hata alınır.Önce subscriber çalışacak bu yüzden bu sayfada üstte Exchange-Declare var.
            //Önce subscriber çalışma nedeni bind oluşarak dinleme mekanizmasının aktif hale gelmesi.

            channel.BasicConsume(queueName, false, consumer);
            Console.WriteLine("Loglar dinleniyor...");
            consumer.Received += (object sender, BasicDeliverEventArgs e) =>
            {
                var message = Encoding.UTF8.GetString(e.Body.ToArray());
                Product product = JsonSerializer.Deserialize<Product>(message); //Bize body'de gelen mesajı deserialize edip console yazdırdık.
                //Thread.Sleep(1500);
                Console.WriteLine($"Gelen Mesaj: {product.Id}-{product.Name}-{product.Price}-{product.Stock}");
                channel.BasicAck(e.DeliveryTag, false);
            };
            Console.ReadLine();
        }


    }
}

using RabbitMQ.Client;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace ExampleRabbitMQ.Publisher
{
    public enum LogNames
    {
        Critical = 1,
        Error = 2,
        Warning = 3,
        Info = 4

    }
    public class Program
    {
        static void Main(string[] args)
        {

            var factory = new ConnectionFactory();
            factory.Uri = new Uri("amqps://haaaxiwv:KKR97TelzVm0anpxsjpV0dYkO-84wiK-@moose.rmq.cloudamqp.com/haaaxiwv");

            using var connection = factory.CreateConnection();

            var channel = connection.CreateModel();
            //durable = uygulama restart attığında exchange kaybolmasın fiziksel kaydedilsin.
            channel.ExchangeDeclare("header-exchange", durable: true, type: ExchangeType.Headers);
            //Burada loglar ile çalışmadığımız için header-exchange yazdık. RouteKey değil header üzerinden göndereceğiz.

            Dictionary<string, object> headers = new Dictionary<string, object>();
            //<Key string, Value object>

            headers.Add("format", "pdf");
            headers.Add("shape", "a4");

            var properties = channel.CreateBasicProperties();
            properties.Headers = headers;
            //properties.Persistent = true; Yazarsak mesajlar silinmez arka planda kalıcı hale gelir.

            var product = new Product { Id = 1, Name = "Kalem", Price = 100, Stock = 10 };
            var productJsonString = JsonSerializer.Serialize(product);
            //Bir class oluşturduk.Burada nesnesini alıp örnek değer verdik ve serialize ettik.Aşağıda GetBytes içinde body'e ekledik.

            channel.BasicPublish("header-exchange", string.Empty, properties, Encoding.UTF8.GetBytes(productJsonString));
            //(1-ExchangeName,2-Route üzerinden çalışmıyoruz string.empty,3-Properties 5-6 satır yukarıda tanımladığımız properties.Headers,
            //4-Yukarıda nesne örneği aldığımız ve serialize ettiğimiz productJsonString.)
            Console.WriteLine("Mesaj Gönderilmiştir.");
            Console.ReadLine();

        }
    }
}

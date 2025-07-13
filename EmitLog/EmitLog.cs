using System.Text;
using RabbitMQ.Client;

var factory = new ConnectionFactory() { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.ExchangeDeclareAsync(exchange: "logs", type: "fanout");
// Fanout type -> broadcasts all the messages it receives to all the queues it know

var message = GetMessage(args);
var body = Encoding.UTF8.GetBytes(message);
await channel.BasicPublishAsync(exchange: "logs", routingKey: string.Empty, body);
Console.WriteLine($" [x] Sent {message}");

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();

static string GetMessage(string[] args)
{
    return ((args.Length > 0) ? string.Join(" ", args) : "Hello World!");
};
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

if (args.Length < 1)
{
    Console.Error.WriteLine("Usage: {0} [info] [warning] [error]", Environment.GetCommandLineArgs()[0]);
    Console.WriteLine(" Press [enter] to exit.");
    Console.ReadLine();
    Environment.ExitCode = 1;
    return;   
}

var factory = new ConnectionFactory() { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.ExchangeDeclareAsync("direct_logs", "direct");

var queueDeclareResult = await channel.QueueDeclareAsync();
string queueName = queueDeclareResult.QueueName;

foreach (string? severity in args)
{
    await channel.QueueBindAsync(queueName, "direct_logs", severity);
}

var consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    var routingKey = ea.RoutingKey;
    Console.WriteLine($" [x] Received '{routingKey}': '{message}'");
    
    return Task.CompletedTask;
};

await channel.BasicConsumeAsync(queueName, true, consumer);

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();
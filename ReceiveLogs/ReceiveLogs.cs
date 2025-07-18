﻿using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var factory = new ConnectionFactory() { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.ExchangeDeclareAsync(exchange: "logs", type: "fanout");

QueueDeclareOk queueDeclareResult = await channel.QueueDeclareAsync();
string queueName = queueDeclareResult.QueueName;
await channel.QueueBindAsync(queue: queueName, "logs", string.Empty);
// A binding is a relationship between an exchange and a queue
// The queue is interested in messages from this exchange

Console.WriteLine(" [*] Waiting for logs.");

var consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += (model, ea) =>
{
  byte[] body = ea.Body.ToArray();
  var message = Encoding.UTF8.GetString(body);
  Console.WriteLine($" [x] Received {message}");
  return Task.CompletedTask;
};

await channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer);

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();
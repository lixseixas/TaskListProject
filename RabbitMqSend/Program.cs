using RabbitMQ.Client;
using RabbitMqSend;
using System.Text;
using System.Text.Json;

const string queueName = "hello";

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false,
    arguments: new Dictionary<string, object?> { { "x-queue-type", "quorum" } });

var report = new WeeklyTaskReportMessage
{
    WeekStartDate = DateTime.Parse("2026-05-21T19:29:44.915Z"),
    WeekEndDate = DateTime.Parse("2026-05-21T19:29:44.915Z"),
    WeekNumber = 6,
    Year = 2026,
    TotalTasks = 7,
    CompletedTasks = 2,
    PendingTasks = 5,
    CompletionPercentage = 20
};

var message = JsonSerializer.Serialize(report, new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = true
});

var body = Encoding.UTF8.GetBytes(message);

await channel.BasicPublishAsync(exchange: string.Empty, routingKey: queueName, body: body);
Console.WriteLine($" [x] Sent {message}");

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();
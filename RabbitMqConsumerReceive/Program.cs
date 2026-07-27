using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMqReceive;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

const string queueName = "hello";
const string reportEndpoint = "https://localhost:44322/api/TaskReport/weekly";

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false,
    arguments: new Dictionary<string, object?> { { "x-queue-type", "quorum" } });

using var httpClient = new HttpClient(new HttpClientHandler
{
    // The API uses the development HTTPS certificate on localhost.
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
});

Console.WriteLine(" [*] Waiting for messages.");

var consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += async (model, ea) =>
{
    try
    {
        var message = Encoding.UTF8.GetString(ea.Body.Span);
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
        var report = JsonSerializer.Deserialize<WeeklyTaskReportModel>(message, options);

        if (report is null)
        {
            Console.WriteLine(" [!] Received an empty or invalid report payload.");
            await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
            return;
        }

        // Generate Id if not provided by sender
        if (report.Id == Guid.Empty)
        {
            report.Id = Guid.NewGuid();
        }

        using var response = await httpClient.PostAsJsonAsync(reportEndpoint, report);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($" [!] API returned {(int)response.StatusCode}: {responseBody}");
            await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
            return;
        }

        Console.WriteLine($" [x] Report inserted: {report.WeekStartDate:yyyy-MM-dd}");
        await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
    }
    catch (JsonException ex)
    {
        Console.WriteLine($" [!] Invalid JSON message: {ex.Message}");
        await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
    }
    catch (Exception ex)
    {
        Console.WriteLine($" [!] Error processing message: {ex.Message}");
        await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
    }
};

await channel.BasicConsumeAsync(queueName, autoAck: false, consumer: consumer);

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();


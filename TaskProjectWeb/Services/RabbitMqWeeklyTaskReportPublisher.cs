using System.Text;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using TaskProject.Models;

namespace TaskProject.Services;

public sealed class RabbitMqWeeklyTaskReportPublisher : IWeeklyTaskReportPublisher
{
    private readonly RabbitMqOptions _options;

    public RabbitMqWeeklyTaskReportPublisher(IOptions<RabbitMqOptions> options)
    {
        _options = options.Value;
    }

    public async Task PublishAsync(WeeklyTaskReportModel report, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(report);

        var factory = new ConnectionFactory { HostName = _options.HostName };
        await using var connection = await factory.CreateConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: _options.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?> { ["x-queue-type"] = "quorum" },
            cancellationToken: cancellationToken);

        var message = JsonSerializer.Serialize(report, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: _options.QueueName,
            mandatory: false,
            basicProperties: new BasicProperties { Persistent = true },
            body: Encoding.UTF8.GetBytes(message),
            cancellationToken: cancellationToken);
    }
}

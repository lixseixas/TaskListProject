namespace TaskProject.Services;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string HostName { get; init; } = "localhost";

    public string QueueName { get; init; } = "hello";
}

using System.Text.Json;
using Bll.@new.Messaging;
using Microsoft.Extensions.Options;
using Models.Messaging;
using RabbitMQ.Client;

namespace Api.Messaging;

public class RabbitMqDocumentMessagePublisher(
    IOptions<RabbitMqOptions> options,
    ILogger<RabbitMqDocumentMessagePublisher> logger) : IDocumentMessagePublisher
{
    public async Task PublishAsync(ProcessDocumentMessage message)
    {
        var settings = options.Value;
        var factory = new ConnectionFactory
        {
            HostName = settings.HostName,
            Port = settings.Port,
            UserName = settings.UserName,
            Password = settings.Password
        };

        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync(new CreateChannelOptions(
            publisherConfirmationsEnabled: true,
            publisherConfirmationTrackingEnabled: true));

        await channel.QueueDeclareAsync(
            queue: settings.QueueName, durable: true, exclusive: false, autoDelete: false);

        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            MessageId = message.MessageId.ToString()
        };

        // The default exchange routes by queue name, as in the work-queue tutorial.
        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: settings.QueueName,
            mandatory: true,
            basicProperties: properties,
            body: JsonSerializer.SerializeToUtf8Bytes(message, JsonSerializerOptions.Web));

        logger.LogInformation(
            "Published OCR message {MessageId} for document {DocumentId} to {QueueName}.",
            message.MessageId, message.DocumentId, settings.QueueName);
    }
}

using System.Text.Json;
using Microsoft.Extensions.Options;
using Models.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OcrWorker;

public class Worker(IOptions<RabbitMqOptions> options, ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        var factory = new ConnectionFactory
        {
            HostName = settings.HostName,
            Port = settings.Port,
            UserName = settings.UserName,
            Password = settings.Password
        };

        await using var connection = await factory.CreateConnectionAsync(stoppingToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: settings.QueueName, durable: true, exclusive: false, autoDelete: false,
            cancellationToken: stoppingToken);
        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, delivery) =>
        {
            ProcessDocumentMessage? message;
            try
            {
                message = JsonSerializer.Deserialize<ProcessDocumentMessage>(
                    delivery.Body.Span, JsonSerializerOptions.Web);
                if (message is null || message.MessageId == Guid.Empty || message.DocumentId <= 0
                    || string.IsNullOrWhiteSpace(message.FileName))
                {
                    throw new JsonException("Invalid document processing message.");
                }
            }
            catch (JsonException exception)
            {
                logger.LogWarning(exception, "Rejected invalid OCR message {MessageId}.",
                    delivery.BasicProperties.MessageId);
                await channel.BasicRejectAsync(delivery.DeliveryTag, requeue: false,
                    cancellationToken: stoppingToken);
                return;
            }

            // Sprint 3: only log receipt. Actual OCR is added in the next sprint.
            logger.LogInformation(
                "OCR placeholder received message {MessageId} for document {DocumentId} ({FileName}).",
                message.MessageId, message.DocumentId, message.FileName);

            await channel.BasicAckAsync(delivery.DeliveryTag, multiple: false,
                cancellationToken: stoppingToken);
        };

        await channel.BasicConsumeAsync(queue: settings.QueueName, autoAck: false,
            consumer: consumer, cancellationToken: stoppingToken);
        logger.LogInformation("OCR worker is waiting for messages on {QueueName}.", settings.QueueName);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}

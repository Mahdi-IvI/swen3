namespace Models.Messaging;

public class RabbitMqOptions
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "dsmuser";
    public string Password { get; set; } = "dsmpassword";
    public string QueueName { get; set; } = "ocr-jobs";
}

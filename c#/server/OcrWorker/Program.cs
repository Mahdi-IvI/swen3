using Models.Messaging;
using OcrWorker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddHostedService<Worker>();

await builder.Build().RunAsync();

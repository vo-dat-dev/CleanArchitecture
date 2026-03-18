using MassTransit;
using Minio;
using PdfExportService.Worker.Consumers;
using PdfExportService.Worker.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

// MinIO
var minioConnectionString = builder.Configuration.GetConnectionString("minio")
    ?? throw new InvalidOperationException("Connection string 'minio' not found.");

// Parse "Endpoint=host:port;AccessKey=...;SecretKey=..." or plain "http://host:port"
var minioParts = minioConnectionString
    .Split(';', StringSplitOptions.RemoveEmptyEntries)
    .Select(p => p.Split('=', 2))
    .Where(p => p.Length == 2)
    .ToDictionary(p => p[0].Trim(), p => p[1].Trim(), StringComparer.OrdinalIgnoreCase);

var minioEndpointRaw = minioParts.GetValueOrDefault("Endpoint") ?? minioConnectionString;
var minioAccessKey   = minioParts.GetValueOrDefault("AccessKey") ?? "minioadmin";
var minioSecretKey   = minioParts.GetValueOrDefault("SecretKey") ?? "minioadmin";
var minioSsl         = minioParts.TryGetValue("SSL", out var ssl) && bool.Parse(ssl);

// Strip scheme if present (http://host:port → host:port)
if (Uri.TryCreate(minioEndpointRaw, UriKind.Absolute, out var minioUri))
{
    minioEndpointRaw = $"{minioUri.Host}:{minioUri.Port}";
    if (!minioSsl) minioSsl = minioUri.Scheme == "https";
}

builder.Services.AddMinio(cfg =>
{
    cfg.WithEndpoint(minioEndpointRaw)
       .WithCredentials(minioAccessKey, minioSecretKey)
       .WithSSL(minioSsl)
       .Build();
});

// MassTransit + RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<CustomerExportBatchConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqConnectionString = builder.Configuration.GetConnectionString("rabbitmq")
            ?? throw new InvalidOperationException("Connection string 'rabbitmq' not found.");

        cfg.Host(new Uri(rabbitMqConnectionString));

        cfg.ReceiveEndpoint("customer-export-batch", e =>
        {
            e.ConfigureConsumer<CustomerExportBatchConsumer>(context);
        });
    });
});

builder.Services.AddSingleton<CustomerPdfGenerator>();

var app = builder.Build();
app.Run();

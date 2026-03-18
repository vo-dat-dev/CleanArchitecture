using CustomerService.Application.Common.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;
using PdfExportService.Worker.Services;

namespace PdfExportService.Worker.Consumers;

public class CustomerExportBatchConsumer : IConsumer<CustomerExportBatch>
{
    private const string BucketName = "customer-exports";

    private readonly CustomerPdfGenerator _pdfGenerator;
    private readonly IMinioClient _minioClient;
    private readonly ILogger<CustomerExportBatchConsumer> _logger;

    public CustomerExportBatchConsumer(
        CustomerPdfGenerator pdfGenerator,
        IMinioClient minioClient,
        ILogger<CustomerExportBatchConsumer> logger)
    {
        _pdfGenerator = pdfGenerator;
        _minioClient = minioClient;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CustomerExportBatch> context)
    {
        var batch = context.Message;

        _logger.LogInformation("Received batch {BatchIndex}/{TotalBatches} for job {JobId} ({Count} customers)",
            batch.BatchIndex + 1, batch.TotalBatches, batch.JobId, batch.Customers.Count);

        var pdfBytes = _pdfGenerator.Generate(batch.JobId, batch.BatchIndex, batch.TotalBatches, batch.Customers);

        await EnsureBucketExistsAsync(context.CancellationToken);

        var objectName = $"{batch.JobId}/batch-{batch.BatchIndex + 1:D4}.pdf";

        using var stream = new MemoryStream(pdfBytes);
        await _minioClient.PutObjectAsync(new PutObjectArgs()
            .WithBucket(BucketName)
            .WithObject(objectName)
            .WithStreamData(stream)
            .WithObjectSize(pdfBytes.Length)
            .WithContentType("application/pdf"),
            context.CancellationToken);

        _logger.LogInformation("Uploaded PDF batch {BatchIndex}/{TotalBatches} for job {JobId} → {ObjectName}",
            batch.BatchIndex + 1, batch.TotalBatches, batch.JobId, objectName);
    }

    private async Task EnsureBucketExistsAsync(CancellationToken cancellationToken)
    {
        var exists = await _minioClient.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(BucketName), cancellationToken);

        if (!exists)
            await _minioClient.MakeBucketAsync(
                new MakeBucketArgs().WithBucket(BucketName), cancellationToken);
    }
}

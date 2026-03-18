using CustomerService.Application.Common.Interfaces;
using CustomerService.Application.Common.Messages;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CustomerService.Application.Customers.Queries.ExportCustomersPdf;

public record ExportCustomersPdfQuery : IRequest<ExportJobResult>;

public record ExportJobResult(Guid JobId, int TotalBatches);

public class ExportCustomersPdfQueryHandler : IRequestHandler<ExportCustomersPdfQuery, ExportJobResult>
{
    private const int BatchSize = 500;

    private const string QueueName = "customer-export-batch";

    private readonly IApplicationDbContext _context;
    private readonly ISendEndpointProvider _sendEndpointProvider;
    private readonly ILogger<ExportCustomersPdfQueryHandler> _logger;

    public ExportCustomersPdfQueryHandler(IApplicationDbContext context, ISendEndpointProvider sendEndpointProvider, ILogger<ExportCustomersPdfQueryHandler> logger)
    {
        _context = context;
        _sendEndpointProvider = sendEndpointProvider;
        _logger = logger;
    }

    public async Task<ExportJobResult> Handle(ExportCustomersPdfQuery request, CancellationToken cancellationToken)
    {
        var jobId = Guid.NewGuid();

        var totalCount = await _context.Customers.CountAsync(cancellationToken);
        var totalBatches = (int)Math.Ceiling((double)totalCount / BatchSize);

        _logger.LogInformation("Starting export job {JobId}: {TotalCount} customers, {TotalBatches} batches", jobId, totalCount, totalBatches);

        var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{QueueName}"));

        for (var i = 0; i < totalBatches; i++)
        {
            var customers = await _context.Customers
                .OrderBy(c => c.Id)
                .Skip(i * BatchSize)
                .Take(BatchSize)
                .Select(c => new CustomerExportItem(
                    c.TransactionId, c.CustomerId, c.CustomerDob, c.CustGender,
                    c.CustLocation, c.CustAccountBalance, c.TransactionDate,
                    c.TransactionTime, c.Status, c.Player, c.ProductId,
                    c.CategoryId, c.CategoryCode, c.Brand, c.Price,
                    c.PaymentMode, c.Frequency))
                .ToListAsync(cancellationToken);

            await sendEndpoint.Send(
                new CustomerExportBatch(jobId, i, totalBatches, customers),
                cancellationToken);

            _logger.LogInformation("Sent batch {BatchIndex}/{TotalBatches} for job {JobId}", i + 1, totalBatches, jobId);
        }

        _logger.LogInformation("Export job {JobId} completed: {TotalBatches} batches published to RabbitMQ", jobId, totalBatches);

        return new ExportJobResult(jobId, totalBatches);
    }
}

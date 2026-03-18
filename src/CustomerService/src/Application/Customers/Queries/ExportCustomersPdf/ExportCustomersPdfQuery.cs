using CustomerService.Application.Common.Interfaces;
using CustomerService.Application.Common.Messages;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.Application.Customers.Queries.ExportCustomersPdf;

public record ExportCustomersPdfQuery : IRequest<ExportJobResult>;

public record ExportJobResult(Guid JobId, int TotalBatches);

public class ExportCustomersPdfQueryHandler : IRequestHandler<ExportCustomersPdfQuery, ExportJobResult>
{
    private const int BatchSize = 500;

    private readonly IApplicationDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public ExportCustomersPdfQueryHandler(IApplicationDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<ExportJobResult> Handle(ExportCustomersPdfQuery request, CancellationToken cancellationToken)
    {
        var jobId = Guid.NewGuid();

        var totalCount = await _context.Customers.CountAsync(cancellationToken);
        var totalBatches = (int)Math.Ceiling((double)totalCount / BatchSize);

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

            await _publishEndpoint.Publish(
                new CustomerExportBatch(jobId, i, totalBatches, customers),
                cancellationToken);
        }

        return new ExportJobResult(jobId, totalBatches);
    }
}

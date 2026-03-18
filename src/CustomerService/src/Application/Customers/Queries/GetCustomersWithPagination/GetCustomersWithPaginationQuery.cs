using CustomerService.Application.Common.Interfaces;
using CustomerService.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.Application.Customers.Queries.GetCustomersWithPagination;

public record GetCustomersWithPaginationQuery(int PageNumber = 1, int PageSize = 20) : IRequest<PaginatedList<CustomerDto>>;

public record CustomerDto(
    int Id,
    string TransactionId,
    string CustomerId,
    string CustomerDob,
    string CustGender,
    string CustLocation,
    decimal CustAccountBalance,
    string TransactionDate,
    string TransactionTime,
    string Status,
    string Brand,
    decimal Price,
    string PaymentMode
);

public class GetCustomersWithPaginationQueryHandler : IRequestHandler<GetCustomersWithPaginationQuery, PaginatedList<CustomerDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCustomersWithPaginationQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<CustomerDto>> Handle(GetCustomersWithPaginationQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Customers
            .OrderBy(c => c.Id)
            .Select(c => new CustomerDto(
                c.Id,
                c.TransactionId,
                c.CustomerId,
                c.CustomerDob,
                c.CustGender,
                c.CustLocation,
                c.CustAccountBalance,
                c.TransactionDate,
                c.TransactionTime,
                c.Status,
                c.Brand,
                c.Price,
                c.PaymentMode
            ));

        return await PaginatedList<CustomerDto>.CreateAsync(query, request.PageNumber, request.PageSize, cancellationToken);
    }
}

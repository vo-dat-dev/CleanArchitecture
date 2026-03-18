using CustomerService.Application.Customers.Queries.ExportCustomersPdf;
using CustomerService.Application.Customers.Queries.GetCustomersWithPagination;

namespace CustomerService.Web.Endpoints;

public class Customers : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetCustomers, "");
        groupBuilder.MapPost(ExportPdf, "export-pdf");
    }

    [EndpointSummary("Get Customers with Pagination")]
    [EndpointDescription("Returns a paginated list of customers.")]
    public static async Task<IResult> GetCustomers(ISender sender, int pageNumber = 1, int pageSize = 20)
    {
        var result = await sender.Send(new GetCustomersWithPaginationQuery(pageNumber, pageSize));
        return Results.Ok(result);
    }

    [EndpointSummary("Export Customers to PDF")]
    [EndpointDescription("Batches all customers from DB and pushes each batch to the queue for PDF generation. Returns a jobId to track progress.")]
    public static async Task<IResult> ExportPdf(ISender sender)
    {
        var result = await sender.Send(new ExportCustomersPdfQuery());

        return Results.Accepted($"/api/Customers/export-pdf/{result.JobId}", new
        {
            result.JobId,
            result.TotalBatches,
            Message = $"Queued {result.TotalBatches} batches for processing."
        });
    }
}

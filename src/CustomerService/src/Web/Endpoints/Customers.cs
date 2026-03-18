using CustomerService.Application.Customers.Queries.ExportCustomersPdf;

namespace CustomerService.Web.Endpoints;

public class Customers : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(ExportPdf, "export-pdf");
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

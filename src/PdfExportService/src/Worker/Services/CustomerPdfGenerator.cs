using CustomerService.Application.Common.Messages;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PdfExportService.Worker.Services;

public class CustomerPdfGenerator
{
    private readonly ILogger<CustomerPdfGenerator> _logger;

    public CustomerPdfGenerator(ILogger<CustomerPdfGenerator> logger)
    {
        _logger = logger;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Generate(Guid jobId, int batchIndex, int totalBatches, List<CustomerExportItem> customers)
    {
        _logger.LogInformation("Generating PDF for job {JobId} batch {BatchIndex}/{TotalBatches} ({Count} customers)",
            jobId, batchIndex + 1, totalBatches, customers.Count);

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);

                page.Header().Text($"Customer Export — Job {jobId} — Batch {batchIndex + 1}/{totalBatches}")
                    .FontSize(11).Bold().AlignCenter();

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(2); // CustomerId
                        cols.RelativeColumn(1); // Gender
                        cols.RelativeColumn(2); // Location
                        cols.RelativeColumn(1); // DOB
                        cols.RelativeColumn(2); // Balance
                        cols.RelativeColumn(2); // TxDate
                        cols.RelativeColumn(1); // Status
                        cols.RelativeColumn(2); // Brand
                        cols.RelativeColumn(2); // Price
                        cols.RelativeColumn(2); // PaymentMode
                    });

                    // Header
                    static IContainer HeaderCell(IContainer c) =>
                        c.Background(Colors.Grey.Darken2).Padding(4);

                    table.Header(header =>
                    {
                        foreach (var col in new[] { "Customer ID", "Gender", "Location", "DOB", "Balance", "Tx Date", "Status", "Brand", "Price", "Payment" })
                            header.Cell().Element(HeaderCell).Text(col).FontColor(Colors.White).FontSize(8).Bold();
                    });

                    // Rows
                    foreach (var c in customers)
                    {
                        static IContainer Cell(IContainer x) => x.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3);

                        table.Cell().Element(Cell).Text(c.CustomerId).FontSize(7);
                        table.Cell().Element(Cell).Text(c.CustGender).FontSize(7);
                        table.Cell().Element(Cell).Text(c.CustLocation).FontSize(7);
                        table.Cell().Element(Cell).Text(c.CustomerDob).FontSize(7);
                        table.Cell().Element(Cell).Text(c.CustAccountBalance.ToString("N2")).FontSize(7);
                        table.Cell().Element(Cell).Text(c.TransactionDate).FontSize(7);
                        table.Cell().Element(Cell).Text(c.Status).FontSize(7);
                        table.Cell().Element(Cell).Text(c.Brand).FontSize(7);
                        table.Cell().Element(Cell).Text(c.Price.ToString("N2")).FontSize(7);
                        table.Cell().Element(Cell).Text(c.PaymentMode).FontSize(7);
                    }
                });

                page.Footer().Text(text =>
                {
                    text.Span($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC  |  ");
                    text.Span("Page ").FontSize(8);
                    text.CurrentPageNumber().FontSize(8);
                    text.Span(" of ").FontSize(8);
                    text.TotalPages().FontSize(8);
                });
            });
        });

        return pdf.GeneratePdf();
    }
}

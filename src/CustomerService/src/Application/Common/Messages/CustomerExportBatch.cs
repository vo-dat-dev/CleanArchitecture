namespace CustomerService.Application.Common.Messages;

public record CustomerExportBatch(
    Guid JobId,
    int BatchIndex,
    int TotalBatches,
    List<CustomerExportItem> Customers);

public record CustomerExportItem(
    string TransactionId,
    string CustomerId,
    string CustomerDob,
    string CustGender,
    string CustLocation,
    decimal CustAccountBalance,
    string TransactionDate,
    string TransactionTime,
    string Status,
    string Player,
    string ProductId,
    string CategoryId,
    string CategoryCode,
    string Brand,
    decimal Price,
    string PaymentMode,
    string Frequency);

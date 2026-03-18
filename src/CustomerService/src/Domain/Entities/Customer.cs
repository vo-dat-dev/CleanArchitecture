namespace CustomerService.Domain.Entities;

public class Customer : BaseAuditableEntity
{
    public string TransactionId { get; set; } = string.Empty;

    public string CustomerId { get; set; } = string.Empty;

    public string CustomerDob { get; set; } = string.Empty;

    public string CustGender { get; set; } = string.Empty;

    public string CustLocation { get; set; } = string.Empty;

    public decimal CustAccountBalance { get; set; }

    public string TransactionDate { get; set; } = string.Empty;

    public string TransactionTime { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Player { get; set; } = string.Empty;

    public string ProductId { get; set; } = string.Empty;

    public string CategoryId { get; set; } = string.Empty;

    public string CategoryCode { get; set; } = string.Empty;

    public string Brand { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string PaymentMode { get; set; } = string.Empty;

    public string Frequency { get; set; } = string.Empty;
}

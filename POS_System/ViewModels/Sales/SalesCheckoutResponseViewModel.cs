namespace POS_System.ViewModels.Sales;

public class SalesCheckoutResponseViewModel
{
    public bool Succeeded { get; set; }

    public string Message { get; set; } = string.Empty;

    public long SaleId { get; set; }

    public string InvoiceNo { get; set; } = string.Empty;

    public string RedirectUrl { get; set; } = string.Empty;

    public SalesIndexViewModel State { get; set; } = new();
}

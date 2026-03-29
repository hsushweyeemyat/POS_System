namespace POS_System.ViewModels.Sales;

public class SalesInvoiceViewModel
{
    public long SaleId { get; set; }

    public string InvoiceNo { get; set; } = string.Empty;

    public DateTime SaleDate { get; set; }

    public int TotalAmount { get; set; }

    public string CashierName { get; set; } = string.Empty;

    public List<SalesInvoiceItemViewModel> Items { get; set; } = new();
}

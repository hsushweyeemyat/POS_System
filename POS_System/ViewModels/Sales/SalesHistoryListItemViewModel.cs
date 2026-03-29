namespace POS_System.ViewModels.Sales;

public class SalesHistoryListItemViewModel
{
    public long SaleId { get; set; }

    public string InvoiceNo { get; set; } = string.Empty;

    public DateTime SaleDate { get; set; }

    public int TotalAmount { get; set; }

    public string UserName { get; set; } = string.Empty;

    public int LineItemCount { get; set; }

    public int TotalQuantity { get; set; }
}

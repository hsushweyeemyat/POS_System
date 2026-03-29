namespace POS_System.ViewModels.Sales;

public class SalesInvoiceItemViewModel
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public int Price { get; set; }

    public int Subtotal => Quantity * Price;
}

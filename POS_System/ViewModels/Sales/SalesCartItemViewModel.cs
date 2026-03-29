namespace POS_System.ViewModels.Sales;

public class SalesCartItemViewModel
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;

    public int UnitPrice { get; set; }

    public int Quantity { get; set; }

    public int AvailableStock { get; set; }

    public int Subtotal => UnitPrice * Quantity;

    public bool CanIncreaseQuantity { get; set; }

    public bool IsAvailable { get; set; }

    public string ValidationMessage { get; set; } = string.Empty;
}

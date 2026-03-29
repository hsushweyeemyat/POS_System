namespace POS_System.ViewModels.Sales;

public class SalesCartViewModel
{
    public List<SalesCartItemViewModel> Items { get; set; } = new();

    public int TotalQuantity { get; set; }

    public int TotalAmount { get; set; }

    public bool CanCheckout { get; set; }

    public string ValidationMessage { get; set; } = string.Empty;
}

namespace POS_System.ViewModels.Sales;

public class SalesProductViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;

    public int Price { get; set; }

    public int StockQty { get; set; }

    public int QuantityInCart { get; set; }

    public int RemainingStock { get; set; }

    public bool CanAddMore { get; set; }
}

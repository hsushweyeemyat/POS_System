namespace POS_System.Models.Sales;

public class PosCartSession
{
    public List<PosCartSessionItem> Items { get; set; } = new();
}

public class PosCartSessionItem
{
    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public int UnitPrice { get; set; }
}

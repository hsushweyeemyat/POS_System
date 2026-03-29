namespace POS_System.ViewModels.Products;

public class ProductListItemViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;

    public int Price { get; set; }

    public int StockQty { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }
}

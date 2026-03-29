namespace POS_System.ViewModels.Sales;

public class SalesIndexViewModel
{
    public string SearchTerm { get; set; } = string.Empty;

    public int ProductCount { get; set; }

    public List<SalesProductViewModel> Products { get; set; } = new();

    public SalesCartViewModel Cart { get; set; } = new();
}

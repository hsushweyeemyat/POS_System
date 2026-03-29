namespace POS_System.ViewModels.Sales;

public class SalesMutationResponseViewModel
{
    public bool Succeeded { get; set; }

    public string Message { get; set; } = string.Empty;

    public SalesIndexViewModel State { get; set; } = new();
}

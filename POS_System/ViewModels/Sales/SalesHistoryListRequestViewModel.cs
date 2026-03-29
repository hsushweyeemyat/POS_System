using POS_System.ViewModels.Shared;

namespace POS_System.ViewModels.Sales;

public class SalesHistoryListRequestViewModel : PaginationRequest
{
    public string SearchTerm { get; set; } = string.Empty;

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }
}

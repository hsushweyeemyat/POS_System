using POS_System.ViewModels.Shared;

namespace POS_System.ViewModels.Sales;

public class SalesHistoryIndexViewModel
{
    public SalesHistoryListRequestViewModel Query { get; set; } = new();

    public PagedResult<SalesHistoryListItemViewModel> SalesPage { get; set; }
        = PagedResult<SalesHistoryListItemViewModel>.Empty();

    public bool IsAdminView { get; set; }

    public string ScopeLabel { get; set; } = string.Empty;

    public string FilterLabel { get; set; } = string.Empty;

    public int VisibleTotalAmount { get; set; }
}

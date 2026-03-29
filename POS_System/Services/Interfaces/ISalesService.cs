using POS_System.ViewModels.Sales;

namespace POS_System.Services.Interfaces;

public interface ISalesService
{
    Task<SalesIndexViewModel> BuildIndexViewModelAsync(
        int userId,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);

    Task<SalesMutationResponseViewModel> AddToCartAsync(
        int userId,
        int productId,
        int quantity,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);

    Task<SalesMutationResponseViewModel> UpdateCartItemAsync(
        int userId,
        int productId,
        int quantity,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);

    Task<SalesMutationResponseViewModel> RemoveCartItemAsync(
        int userId,
        int productId,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);

    Task<SalesCheckoutResponseViewModel> CheckoutAsync(
        int userId,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);

    Task<SalesHistoryIndexViewModel> BuildHistoryViewModelAsync(
        int userId,
        bool isAdmin,
        SalesHistoryListRequestViewModel request,
        CancellationToken cancellationToken = default);

    Task<SalesInvoiceViewModel?> GetInvoiceAsync(
        long saleId,
        int userId,
        bool isAdmin,
        CancellationToken cancellationToken = default);
}

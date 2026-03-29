using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using POS_System.Data.Entities;
using POS_System.Extensions;
using POS_System.Models.Sales;
using POS_System.Repositories.Interfaces;
using POS_System.Services.Interfaces;
using POS_System.ViewModels.Sales;
using POS_System.ViewModels.Shared;

namespace POS_System.Services.Implementations;

public class SalesService : ISalesService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ISalesRepository _salesRepository;

    public SalesService(IHttpContextAccessor httpContextAccessor, ISalesRepository salesRepository)
    {
        _httpContextAccessor = httpContextAccessor;
        _salesRepository = salesRepository;
    }

    public Task<SalesIndexViewModel> BuildIndexViewModelAsync(
        int userId,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        return BuildStateAsync(userId, searchTerm, cancellationToken);
    }

    public async Task<SalesMutationResponseViewModel> AddToCartAsync(
        int userId,
        int productId,
        int quantity,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        if (quantity < 1)
        {
            return await BuildFailureResponseAsync(
                userId,
                searchTerm,
                "Quantity must be at least 1.",
                cancellationToken);
        }

        var product = await _salesRepository.GetActiveProductByIdAsync(productId, cancellationToken);

        if (product is null || product.StockQty < 1)
        {
            return await BuildFailureResponseAsync(
                userId,
                searchTerm,
                "The selected product is not available for sale.",
                cancellationToken);
        }

        var cart = GetNormalizedCart(userId, out _);
        var existingItem = cart.Items.FirstOrDefault(item => item.ProductId == productId);
        var newQuantity = quantity + (existingItem?.Quantity ?? 0);

        if (newQuantity > product.StockQty)
        {
            return await BuildFailureResponseAsync(
                userId,
                searchTerm,
                BuildStockMessage(product),
                cancellationToken);
        }

        if (existingItem is null)
        {
            cart.Items.Add(new PosCartSessionItem
            {
                ProductId = product.Id,
                Quantity = quantity,
                ProductName = product.Name,
                UnitPrice = product.Price
            });
        }
        else
        {
            existingItem.Quantity = newQuantity;
            existingItem.ProductName = product.Name;
            existingItem.UnitPrice = product.Price;
        }

        SaveCart(userId, cart);

        return new SalesMutationResponseViewModel
        {
            Succeeded = true,
            Message = $"{product.Name} added to cart.",
            State = await BuildStateAsync(userId, searchTerm, cancellationToken)
        };
    }

    public async Task<SalesMutationResponseViewModel> UpdateCartItemAsync(
        int userId,
        int productId,
        int quantity,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        if (quantity < 1)
        {
            return await BuildFailureResponseAsync(
                userId,
                searchTerm,
                "Quantity must be at least 1.",
                cancellationToken);
        }

        var cart = GetNormalizedCart(userId, out _);
        var existingItem = cart.Items.FirstOrDefault(item => item.ProductId == productId);

        if (existingItem is null)
        {
            return await BuildFailureResponseAsync(
                userId,
                searchTerm,
                "The selected cart item could not be found.",
                cancellationToken);
        }

        var product = await _salesRepository.GetActiveProductByIdAsync(productId, cancellationToken);

        if (product is null)
        {
            return await BuildFailureResponseAsync(
                userId,
                searchTerm,
                "This product is no longer active and cannot be updated.",
                cancellationToken);
        }

        if (quantity > product.StockQty)
        {
            return await BuildFailureResponseAsync(
                userId,
                searchTerm,
                BuildStockMessage(product),
                cancellationToken);
        }

        existingItem.Quantity = quantity;
        existingItem.ProductName = product.Name;
        existingItem.UnitPrice = product.Price;
        SaveCart(userId, cart);

        return new SalesMutationResponseViewModel
        {
            Succeeded = true,
            Message = $"{product.Name} quantity updated.",
            State = await BuildStateAsync(userId, searchTerm, cancellationToken)
        };
    }

    public async Task<SalesMutationResponseViewModel> RemoveCartItemAsync(
        int userId,
        int productId,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var cart = GetNormalizedCart(userId, out _);
        var removedItem = cart.Items.FirstOrDefault(item => item.ProductId == productId);

        if (removedItem is not null)
        {
            cart.Items.Remove(removedItem);
            SaveCart(userId, cart);
        }

        return new SalesMutationResponseViewModel
        {
            Succeeded = true,
            Message = "Item removed from cart.",
            State = await BuildStateAsync(userId, searchTerm, cancellationToken)
        };
    }

    public async Task<SalesCheckoutResponseViewModel> CheckoutAsync(
        int userId,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var cart = GetNormalizedCart(userId, out _);
        var cashierName = GetCurrentCashierName(userId);

        if (cart.Items.Count == 0)
        {
            return await BuildCheckoutFailureResponseAsync(
                userId,
                searchTerm,
                "Add at least one product before checkout.",
                cancellationToken);
        }

        var productIds = cart.Items
            .Select(item => item.ProductId)
            .Distinct()
            .ToArray();

        await using var transaction = await _salesRepository.BeginTransactionAsync(cancellationToken);

        try
        {
            var trackedProducts = await _salesRepository.GetTrackedProductsByIdsAsync(productIds, cancellationToken);
            var productsById = trackedProducts.ToDictionary(product => product.Id);
            var saleItems = new List<TblSaleItem>();
            var totalAmount = 0;

            foreach (var cartItem in cart.Items)
            {
                if (!productsById.TryGetValue(cartItem.ProductId, out var product) || product.IsActive != 1)
                {
                    await transaction.RollbackAsync(cancellationToken);

                    return await BuildCheckoutFailureResponseAsync(
                        userId,
                        searchTerm,
                        "One or more cart items are no longer active.",
                        cancellationToken);
                }

                if (product.StockQty < cartItem.Quantity)
                {
                    await transaction.RollbackAsync(cancellationToken);

                    return await BuildCheckoutFailureResponseAsync(
                        userId,
                        searchTerm,
                        BuildStockMessage(product),
                        cancellationToken);
                }

                product.StockQty -= cartItem.Quantity;
                totalAmount += product.Price * cartItem.Quantity;

                saleItems.Add(new TblSaleItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Qty = cartItem.Quantity,
                    Price = product.Price
                });
            }

            var saleTimestamp = GetCurrentUtcTimestamp();
            var sale = new TblSale
            {
                InvoiceNo = await GenerateUniqueInvoiceNoAsync(cancellationToken),
                SaleDate = saleTimestamp,
                TotalAmount = totalAmount,
                UserId = userId,
                CashierName = cashierName,
                CreatedDate = saleTimestamp
            };

            foreach (var saleItem in saleItems)
            {
                sale.SaleItems.Add(saleItem);
            }

            await _salesRepository.AddSaleAsync(sale, cancellationToken);
            await _salesRepository.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            ClearCart(userId);

            return new SalesCheckoutResponseViewModel
            {
                Succeeded = true,
                Message = $"Checkout completed successfully. Invoice {sale.InvoiceNo} is ready.",
                SaleId = sale.Id,
                InvoiceNo = sale.InvoiceNo
            };
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<SalesInvoiceViewModel?> GetInvoiceAsync(
        long saleId,
        int userId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var sale = await _salesRepository.GetSaleByIdAsync(
            saleId,
            isAdmin ? null : userId,
            cancellationToken);

        if (sale is null)
        {
            return null;
        }

        return new SalesInvoiceViewModel
        {
            SaleId = sale.Id,
            InvoiceNo = sale.InvoiceNo,
            SaleDate = ConvertStoredUtcToLocal(sale.SaleDate),
            TotalAmount = sale.TotalAmount,
            CashierName = ResolveCashierName(sale),
            Items = sale.SaleItems
                .Select((item, index) => new SalesInvoiceItemViewModel
                {
                    ProductId = item.ProductId,
                    ItemLabel = $"Line {index + 1:00}",
                    ProductName = ResolveProductName(item),
                    Quantity = item.Qty,
                    Price = item.Price
                })
                .ToList()
        };
    }

    public async Task<SalesHistoryIndexViewModel> BuildHistoryViewModelAsync(
        int userId,
        bool isAdmin,
        SalesHistoryListRequestViewModel request,
        CancellationToken cancellationToken = default)
    {
        var normalizedRequest = NormalizeHistoryRequest(request, isAdmin);
        DateTime? startDateInclusiveUtc = normalizedRequest.StartDate.HasValue
            ? ConvertLocalBoundaryToUtc(normalizedRequest.StartDate.Value.Date)
            : null;
        DateTime? endDateExclusiveUtc = normalizedRequest.EndDate.HasValue
            ? ConvertLocalBoundaryToUtc(normalizedRequest.EndDate.Value.Date.AddDays(1))
            : null;
        var historyPage = await _salesRepository.GetSaleHistoryAsync(
            isAdmin ? null : userId,
            normalizedRequest.SearchTerm,
            startDateInclusiveUtc,
            endDateExclusiveUtc,
            normalizedRequest,
            cancellationToken);

        var saleItems = historyPage.Items
            .Select(item => new SalesHistoryListItemViewModel
            {
                SaleId = item.SaleId,
                InvoiceNo = item.InvoiceNo,
                SaleDate = ConvertStoredUtcToLocal(item.SaleDate),
                TotalAmount = item.TotalAmount,
                UserName = item.UserName,
                LineItemCount = item.LineItemCount,
                TotalQuantity = item.TotalQuantity
            })
            .ToList();

        return new SalesHistoryIndexViewModel
        {
            Query = normalizedRequest,
            IsAdminView = isAdmin,
            ScopeLabel = BuildHistoryScopeLabel(normalizedRequest, isAdmin),
            FilterLabel = BuildHistoryFilterLabel(normalizedRequest, isAdmin),
            VisibleTotalAmount = saleItems.Sum(item => item.TotalAmount),
            SalesPage = new PagedResult<SalesHistoryListItemViewModel>(
                saleItems,
                historyPage.Page,
                historyPage.PageSize,
                historyPage.TotalCount)
        };
    }

    private async Task<SalesIndexViewModel> BuildStateAsync(
        int userId,
        string? searchTerm,
        CancellationToken cancellationToken)
    {
        var normalizedSearchTerm = searchTerm?.Trim() ?? string.Empty;
        var cart = GetNormalizedCart(userId, out var cartChanged);
        var cartProductIds = cart.Items
            .Select(item => item.ProductId)
            .Distinct()
            .ToArray();

        var activeCartProducts = await _salesRepository.GetActiveProductsByIdsAsync(cartProductIds, cancellationToken);
        var cartProductsById = activeCartProducts.ToDictionary(product => product.Id);
        var cartItems = new List<SalesCartItemViewModel>();
        var snapshotChanged = cartChanged;

        foreach (var cartItem in cart.Items)
        {
            cartProductsById.TryGetValue(cartItem.ProductId, out var currentProduct);

            var productName = currentProduct?.Name ?? cartItem.ProductName;
            var unitPrice = currentProduct?.Price ?? cartItem.UnitPrice;
            var categoryName = currentProduct?.Category?.Name ?? string.Empty;
            var availableStock = Math.Max(currentProduct?.StockQty ?? 0, 0);
            var isActive = currentProduct is not null;
            var isStockAvailable = isActive && availableStock >= cartItem.Quantity && availableStock > 0;
            var validationMessage = isActive
                ? isStockAvailable
                    ? string.Empty
                    : availableStock == 0
                        ? "Out of stock."
                        : $"Only {availableStock:N0} left in stock."
                : "Product is no longer active.";

            if (currentProduct is not null &&
                (cartItem.ProductName != currentProduct.Name || cartItem.UnitPrice != currentProduct.Price))
            {
                cartItem.ProductName = currentProduct.Name;
                cartItem.UnitPrice = currentProduct.Price;
                snapshotChanged = true;
            }

            cartItems.Add(new SalesCartItemViewModel
            {
                ProductId = cartItem.ProductId,
                ProductName = string.IsNullOrWhiteSpace(productName) ? $"Product #{cartItem.ProductId}" : productName,
                CategoryName = categoryName,
                UnitPrice = unitPrice,
                Quantity = cartItem.Quantity,
                AvailableStock = availableStock,
                CanIncreaseQuantity = isActive && availableStock > cartItem.Quantity,
                IsAvailable = isStockAvailable,
                ValidationMessage = validationMessage
            });
        }

        if (snapshotChanged)
        {
            SaveCart(userId, cart);
        }

        var quantityByProductId = cartItems.ToDictionary(item => item.ProductId, item => item.Quantity);
        var sellableProducts = await _salesRepository.SearchSellableProductsAsync(normalizedSearchTerm, cancellationToken);
        var productCards = sellableProducts
            .Select(product =>
            {
                quantityByProductId.TryGetValue(product.Id, out var quantityInCart);
                var remainingStock = Math.Max(product.StockQty - quantityInCart, 0);

                return new SalesProductViewModel
                {
                    Id = product.Id,
                    Name = product.Name,
                    CategoryName = product.Category?.Name ?? string.Empty,
                    Price = product.Price,
                    StockQty = product.StockQty,
                    QuantityInCart = quantityInCart,
                    RemainingStock = remainingStock,
                    CanAddMore = remainingStock > 0
                };
            })
            .ToList();

        var canCheckout = cartItems.Count > 0 && cartItems.All(item => item.IsAvailable);
        var cartMessage = cartItems.Count == 0
            ? "Cart is empty."
            : canCheckout
                ? string.Empty
                : "Review unavailable cart items before checkout.";

        return new SalesIndexViewModel
        {
            SearchTerm = normalizedSearchTerm,
            ProductCount = productCards.Count,
            Products = productCards,
            Cart = new SalesCartViewModel
            {
                Items = cartItems,
                TotalQuantity = cartItems.Sum(item => item.Quantity),
                TotalAmount = cartItems.Sum(item => item.Subtotal),
                CanCheckout = canCheckout,
                ValidationMessage = cartMessage
            }
        };
    }

    private async Task<SalesMutationResponseViewModel> BuildFailureResponseAsync(
        int userId,
        string? searchTerm,
        string message,
        CancellationToken cancellationToken)
    {
        return new SalesMutationResponseViewModel
        {
            Succeeded = false,
            Message = message,
            State = await BuildStateAsync(userId, searchTerm, cancellationToken)
        };
    }

    private async Task<SalesCheckoutResponseViewModel> BuildCheckoutFailureResponseAsync(
        int userId,
        string? searchTerm,
        string message,
        CancellationToken cancellationToken)
    {
        return new SalesCheckoutResponseViewModel
        {
            Succeeded = false,
            Message = message,
            State = await BuildStateAsync(userId, searchTerm, cancellationToken)
        };
    }

    private PosCartSession GetNormalizedCart(int userId, out bool wasChanged)
    {
        var session = GetSession();
        var cart = session.GetJson<PosCartSession>(GetCartSessionKey(userId)) ?? new PosCartSession();
        cart.Items ??= new List<PosCartSessionItem>();
        var mergedItems = new Dictionary<int, PosCartSessionItem>();
        var orderedProductIds = new List<int>();

        wasChanged = false;

        foreach (var item in cart.Items)
        {
            if (item.ProductId <= 0)
            {
                wasChanged = true;
                continue;
            }

            var quantity = item.Quantity < 1 ? 1 : item.Quantity;
            var unitPrice = item.UnitPrice < 0 ? 0 : item.UnitPrice;
            var productName = item.ProductName?.Trim() ?? string.Empty;

            if (item.Quantity != quantity || item.UnitPrice != unitPrice || item.ProductName != productName)
            {
                wasChanged = true;
            }

            if (!mergedItems.TryGetValue(item.ProductId, out var mergedItem))
            {
                mergedItem = new PosCartSessionItem
                {
                    ProductId = item.ProductId,
                    Quantity = 0,
                    ProductName = productName,
                    UnitPrice = unitPrice
                };

                mergedItems[item.ProductId] = mergedItem;
                orderedProductIds.Add(item.ProductId);
            }
            else
            {
                wasChanged = true;
            }

            mergedItem.Quantity += quantity;

            if (!string.IsNullOrWhiteSpace(productName))
            {
                mergedItem.ProductName = productName;
            }

            mergedItem.UnitPrice = unitPrice;
        }

        var normalizedItems = orderedProductIds
            .Select(productId => mergedItems[productId])
            .ToList();

        if (normalizedItems.Count != cart.Items.Count)
        {
            wasChanged = true;
        }

        cart.Items = normalizedItems;

        if (wasChanged)
        {
            SaveCart(userId, cart);
        }

        return cart;
    }

    private ISession GetSession()
    {
        return _httpContextAccessor.HttpContext?.Session
            ?? throw new InvalidOperationException("An active session is required for the POS cart.");
    }

    private void SaveCart(int userId, PosCartSession cart)
    {
        GetSession().SetJson(GetCartSessionKey(userId), cart);
    }

    private void ClearCart(int userId)
    {
        GetSession().Remove(GetCartSessionKey(userId));
    }

    private string GetCartSessionKey(int userId)
    {
        return $"pos-sales-cart:{userId}";
    }

    private async Task<string> GenerateUniqueInvoiceNoAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            var invoiceNo = $"INV-{GetCurrentUtcTimestamp():yyyyMMddHHmmssfff}-{RandomNumberGenerator.GetInt32(100000, 999999)}";

            if (!await _salesRepository.InvoiceExistsAsync(invoiceNo, cancellationToken))
            {
                return invoiceNo;
            }
        }

        return $"INV-{Guid.NewGuid():N}".ToUpperInvariant();
    }

    private static string BuildStockMessage(TblProduct product)
    {
        return product.StockQty <= 0
            ? $"{product.Name} is out of stock."
            : $"Only {product.StockQty:N0} of {product.Name} available in stock.";
    }

    private string GetCurrentCashierName(int userId)
    {
        var cashierName = _httpContextAccessor.HttpContext?.User
            ?.FindFirstValue("full_name")
            ?.Trim();

        return string.IsNullOrWhiteSpace(cashierName)
            ? $"User #{userId:N0}"
            : cashierName;
    }

    private static string ResolveCashierName(TblSale sale)
    {
        var cashierName = sale.CashierName?.Trim();

        if (!string.IsNullOrWhiteSpace(cashierName))
        {
            return cashierName;
        }

        return sale.User?.FullName?.Trim() switch
        {
            { Length: > 0 } fullName => fullName,
            _ => "Unknown cashier"
        };
    }

    private static string ResolveProductName(TblSaleItem saleItem)
    {
        var productName = saleItem.ProductName?.Trim();

        if (!string.IsNullOrWhiteSpace(productName))
        {
            return productName;
        }

        return saleItem.Product?.Name?.Trim() switch
        {
            { Length: > 0 } currentProductName => currentProductName,
            _ => $"Product #{saleItem.ProductId}"
        };
    }

    private static DateTime GetCurrentUtcTimestamp()
    {
        return DateTime.UtcNow;
    }

    private static DateTime ConvertStoredUtcToLocal(DateTime value)
    {
        var utcValue = value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);

        return TimeZoneInfo.ConvertTimeFromUtc(utcValue, TimeZoneInfo.Local);
    }

    private static DateTime ConvertLocalBoundaryToUtc(DateTime value)
    {
        var localValue = value.Kind == DateTimeKind.Unspecified
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Unspecified);

        return TimeZoneInfo.ConvertTimeToUtc(localValue, TimeZoneInfo.Local);
    }

    private static SalesHistoryListRequestViewModel NormalizeHistoryRequest(
        SalesHistoryListRequestViewModel? request,
        bool isAdmin)
    {
        var normalizedRequest = new SalesHistoryListRequestViewModel
        {
            Page = request?.Page ?? PaginationRequest.DefaultPage,
            PageSize = request?.PageSize ?? PaginationRequest.DefaultPageSize,
            SearchTerm = request?.SearchTerm?.Trim() ?? string.Empty,
            StartDate = request?.StartDate?.Date,
            EndDate = request?.EndDate?.Date
        };

        if (normalizedRequest.StartDate.HasValue && !normalizedRequest.EndDate.HasValue)
        {
            normalizedRequest.EndDate = normalizedRequest.StartDate;
        }
        else if (!normalizedRequest.StartDate.HasValue && normalizedRequest.EndDate.HasValue)
        {
            normalizedRequest.StartDate = normalizedRequest.EndDate;
        }

        if (!isAdmin && !normalizedRequest.StartDate.HasValue && !normalizedRequest.EndDate.HasValue)
        {
            var today = DateTime.Today;
            normalizedRequest.StartDate = today;
            normalizedRequest.EndDate = today;
        }

        if (normalizedRequest.StartDate.HasValue &&
            normalizedRequest.EndDate.HasValue &&
            normalizedRequest.EndDate.Value < normalizedRequest.StartDate.Value)
        {
            (normalizedRequest.StartDate, normalizedRequest.EndDate) =
                (normalizedRequest.EndDate, normalizedRequest.StartDate);
        }

        return normalizedRequest;
    }

    private static string BuildHistoryFilterLabel(
        SalesHistoryListRequestViewModel request,
        bool isAdmin)
    {
        if (!request.StartDate.HasValue || !request.EndDate.HasValue)
        {
            return isAdmin ? "All sale dates" : "No date filter applied";
        }

        var startDate = request.StartDate.Value;
        var endDate = request.EndDate.Value;

        if (startDate.Date == endDate.Date)
        {
            return startDate.ToString("dd MMM yyyy");
        }

        return $"{startDate:dd MMM yyyy} - {endDate:dd MMM yyyy}";
    }

    private static string BuildHistoryScopeLabel(
        SalesHistoryListRequestViewModel request,
        bool isAdmin)
    {
        if (isAdmin)
        {
            return "All completed sales";
        }

        return request.StartDate.HasValue &&
               request.EndDate.HasValue &&
               request.StartDate.Value.Date == request.EndDate.Value.Date &&
               request.StartDate.Value.Date == DateTime.Today
            ? "Today's completed sales"
            : "Your completed sales";
    }
}

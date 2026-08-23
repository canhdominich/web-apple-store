using AppleStore.Web.Models.Entities;
using AppleStore.Web.Models.ViewModels;

namespace AppleStore.Web.Services.Interfaces;

public interface IOrderService
{
    Task<(Order? Order, Payment? Payment, string? Error)> CreateAsync(string cartKey, string? userId, CheckoutViewModel model);
    Task<Order?> GetForUserAsync(int id, string userId);
    Task<Order?> GetReceiptAsync(int id, string? userId, string? orderCode);
    Task<IReadOnlyList<Order>> GetHistoryAsync(string userId);
    Task<(Order? Order, Payment? Payment, string? Error)> RetryPaymentAsync(int id, string userId);
    Task<PaymentResultViewModel> ProcessVnPayCallbackAsync(VnPayResponseModel response);
}

using AppleStore.Web.Models.Entities;
using AppleStore.Web.Models.ViewModels;

namespace AppleStore.Web.Services.Interfaces;

public interface ICartService
{
    Task<CartViewModel> GetAsync(string cartKey);
    Task<(bool Success, string Message)> AddAsync(string cartKey, AddToCartRequest request);
    Task<(bool Success, string Message)> UpdateAsync(string cartKey, int itemId, int quantity);
    Task<bool> RemoveAsync(string cartKey, int itemId);
    Task ClearAsync(string cartKey);
    Task<int> CountAsync(string cartKey);
    Task MergeAsync(string sourceCartKey, string targetCartKey);
}

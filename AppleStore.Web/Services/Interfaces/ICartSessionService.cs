namespace AppleStore.Web.Services.Interfaces;

public interface ICartSessionService
{
    string GetCartKey();
    string EnsureCartKey();
    string? GetGuestCartKey();
    void ClearGuestCart();
}

namespace AppleStore.Web.Models.ViewModels;

public class VnPaySettings
{
    public string TmnCode { get; set; } = string.Empty;
    public string HashSecret { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
    public string Version { get; set; } = "2.1.0";
    public string Command { get; set; } = "pay";
    public string CurrCode { get; set; } = "VND";
    public string Locale { get; set; } = "vn";
    public string OrderType { get; set; } = "other";
}
public class VnPayRequestModel
{
    public string TxnRef { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string OrderInfo { get; set; } = string.Empty;
    public string IpAddress { get; set; } = "127.0.0.1";
}
public class VnPayResponseModel
{
    public bool IsValidSignature { get; set; }
    public string ResponseCode { get; set; } = string.Empty;
    public string TransactionStatus { get; set; } = string.Empty;
    public string TxnRef { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string TransactionNo { get; set; } = string.Empty;
    public string PayDate { get; set; } = string.Empty;
    public string BankCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public IReadOnlyDictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
}

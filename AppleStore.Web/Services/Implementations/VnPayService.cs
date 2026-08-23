using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using AppleStore.Web.Models.ViewModels;
using AppleStore.Web.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace AppleStore.Web.Services.Implementations;

public class VnPayService(IOptions<VnPaySettings> options, ILogger<VnPayService> logger) : IVnPayService
{
    private readonly VnPaySettings settings = options.Value;

    public string CreatePaymentUrl(HttpContext context, VnPayRequestModel model)
    {
        if (string.IsNullOrWhiteSpace(settings.TmnCode) || string.IsNullOrWhiteSpace(settings.HashSecret)) throw new InvalidOperationException("VNPay chưa được cấu hình TmnCode/HashSecret.");
        var ip = string.IsNullOrWhiteSpace(model.IpAddress) ? context.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1" : model.IpAddress;
        var parameters = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["vnp_Version"] = settings.Version,
            ["vnp_Command"] = settings.Command,
            ["vnp_TmnCode"] = settings.TmnCode,
            ["vnp_Amount"] = ToVnPayAmount(model.Amount),
            ["vnp_CreateDate"] = DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss"),
            ["vnp_CurrCode"] = settings.CurrCode,
            ["vnp_IpAddr"] = ip,
            ["vnp_Locale"] = settings.Locale,
            ["vnp_OrderInfo"] = model.OrderInfo,
            ["vnp_OrderType"] = settings.OrderType,
            ["vnp_ReturnUrl"] = settings.ReturnUrl,
            ["vnp_TxnRef"] = model.TxnRef
        };
        var hashData = BuildQuery(parameters);
        var secureHash = ComputeHmacSha512(settings.HashSecret, hashData);
        var url = $"{settings.BaseUrl}?{hashData}&vnp_SecureHash={secureHash}";
        logger.LogInformation("VNPay payment URL created for {TxnRef}", model.TxnRef);
        return url;
    }

    public VnPayResponseModel ValidateCallback(IQueryCollection query)
    {
        var parameters = query.Where(x => x.Key.StartsWith("vnp_", StringComparison.OrdinalIgnoreCase) && x.Key is not "vnp_SecureHash" and not "vnp_SecureHashType")
            .ToDictionary(x => x.Key, x => x.Value.ToString(), StringComparer.Ordinal);
        var receivedHash = query["vnp_SecureHash"].ToString();
        var calculatedHash = ComputeHmacSha512(settings.HashSecret, BuildQuery(new SortedDictionary<string, string>(parameters, StringComparer.Ordinal)));
        var valid = !string.IsNullOrWhiteSpace(receivedHash) && CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(calculatedHash.ToLowerInvariant()), Encoding.UTF8.GetBytes(receivedHash.ToLowerInvariant()));
        if (!valid) logger.LogWarning("Invalid VNPay callback signature for {TxnRef}", query["vnp_TxnRef"].ToString()); else logger.LogInformation("VNPay callback signature validated for {TxnRef}", query["vnp_TxnRef"].ToString());
        return new VnPayResponseModel
        {
            IsValidSignature = valid,
            ResponseCode = query["vnp_ResponseCode"].ToString(),
            TransactionStatus = query["vnp_TransactionStatus"].ToString(),
            TxnRef = query["vnp_TxnRef"].ToString(),
            Amount = ParseVnPayAmount(query["vnp_Amount"].ToString()),
            TransactionNo = query["vnp_TransactionNo"].ToString(),
            PayDate = query["vnp_PayDate"].ToString(),
            BankCode = query["vnp_BankCode"].ToString(),
            Message = query["vnp_Message"].ToString(),
            Parameters = parameters
        };
    }

    public static string ToVnPayAmount(decimal amount) => decimal.ToInt64(decimal.Round(amount * 100, 0, MidpointRounding.AwayFromZero)).ToString(CultureInfo.InvariantCulture);
    public static decimal ParseVnPayAmount(string value) => long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var minor) ? minor / 100m : -1m;
    private static string BuildQuery(IEnumerable<KeyValuePair<string, string>> parameters) => string.Join("&", parameters.Where(x => !string.IsNullOrWhiteSpace(x.Value)).Select(x => $"{x.Key}={WebUtility.UrlEncode(x.Value)}"));
    private static string ComputeHmacSha512(string secret, string data)
    {
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secret));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(data))).ToLowerInvariant();
    }
}

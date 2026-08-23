using Microsoft.AspNetCore.Http;
using AppleStore.Web.Models.ViewModels;

namespace AppleStore.Web.Services.Interfaces;

public interface IVnPayService
{
    string CreatePaymentUrl(HttpContext context, VnPayRequestModel model);
    VnPayResponseModel ValidateCallback(IQueryCollection query);
}

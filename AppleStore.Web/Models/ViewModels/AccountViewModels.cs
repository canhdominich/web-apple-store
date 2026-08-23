using System.ComponentModel.DataAnnotations;

namespace AppleStore.Web.Models.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập email."), EmailAddress(ErrorMessage = "Email không đúng định dạng."), Display(Name = "Email")] public string Email { get; set; } = string.Empty;
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu."), DataType(DataType.Password), Display(Name = "Mật khẩu")] public string Password { get; set; } = string.Empty;
    [Required(ErrorMessage = "Tùy chọn ghi nhớ đăng nhập không hợp lệ."), Display(Name = "Ghi nhớ đăng nhập")] public bool RememberMe { get; set; }
    public string? ReturnUrl { get; set; }
}
public class RegisterViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ và tên."), Display(Name = "Họ và tên"), StringLength(150, ErrorMessage = "Họ và tên không được vượt quá 150 ký tự.")] public string FullName { get; set; } = string.Empty;
    [Required(ErrorMessage = "Vui lòng nhập email."), EmailAddress(ErrorMessage = "Email không đúng định dạng."), Display(Name = "Email")] public string Email { get; set; } = string.Empty;
    [Required(ErrorMessage = "Vui lòng nhập số điện thoại."), Phone(ErrorMessage = "Số điện thoại không đúng định dạng."), Display(Name = "Số điện thoại")] public string Phone { get; set; } = string.Empty;
    [Required(ErrorMessage = "Vui lòng nhập địa chỉ."), Display(Name = "Địa chỉ"), StringLength(500, ErrorMessage = "Địa chỉ không được vượt quá 500 ký tự.")] public string Address { get; set; } = string.Empty;
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu."), StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu phải từ 8 đến 100 ký tự."), DataType(DataType.Password), Display(Name = "Mật khẩu")] public string Password { get; set; } = string.Empty;
    [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu."), Compare(nameof(Password), ErrorMessage = "Mật khẩu xác nhận không khớp."), DataType(DataType.Password), Display(Name = "Xác nhận mật khẩu")] public string ConfirmPassword { get; set; } = string.Empty;
}
public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại."), DataType(DataType.Password), Display(Name = "Mật khẩu hiện tại")] public string CurrentPassword { get; set; } = string.Empty;
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới."), StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu mới phải từ 8 đến 100 ký tự."), DataType(DataType.Password), Display(Name = "Mật khẩu mới")] public string NewPassword { get; set; } = string.Empty;
    [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu mới."), Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu xác nhận không khớp."), DataType(DataType.Password), Display(Name = "Xác nhận mật khẩu mới")] public string ConfirmPassword { get; set; } = string.Empty;
}
public class ResetPasswordViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập email."), EmailAddress(ErrorMessage = "Email không đúng định dạng."), Display(Name = "Email")] public string Email { get; set; } = string.Empty;
    [Required(ErrorMessage = "Token đặt lại mật khẩu không hợp lệ.")] public string Token { get; set; } = string.Empty;
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới."), StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu mới phải từ 8 đến 100 ký tự."), DataType(DataType.Password), Display(Name = "Mật khẩu mới")] public string NewPassword { get; set; } = string.Empty;
    [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu mới."), Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu xác nhận không khớp."), DataType(DataType.Password), Display(Name = "Xác nhận mật khẩu mới")] public string ConfirmPassword { get; set; } = string.Empty;
}

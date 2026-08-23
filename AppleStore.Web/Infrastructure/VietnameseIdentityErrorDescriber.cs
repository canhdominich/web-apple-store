using Microsoft.AspNetCore.Identity;

namespace AppleStore.Web.Infrastructure;

public class VietnameseIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DefaultError() => Error("Đã xảy ra lỗi. Vui lòng thử lại.");
    public override IdentityError ConcurrencyFailure() => Error("Dữ liệu đã thay đổi bởi thao tác khác. Vui lòng tải lại trang.");
    public override IdentityError PasswordMismatch() => Error("Mật khẩu hiện tại không đúng.");
    public override IdentityError InvalidToken() => Error("Mã xác thực không hợp lệ hoặc đã hết hạn.");
    public override IdentityError LoginAlreadyAssociated() => Error("Tài khoản đăng nhập này đã được liên kết.");
    public override IdentityError InvalidUserName(string? userName) => Error($"Tên đăng nhập '{userName}' không hợp lệ.");
    public override IdentityError InvalidEmail(string? email) => Error($"Email '{email}' không hợp lệ.");
    public override IdentityError DuplicateUserName(string userName) => Error($"Tên đăng nhập '{userName}' đã được sử dụng.");
    public override IdentityError DuplicateEmail(string email) => Error($"Email '{email}' đã được sử dụng.");
    public override IdentityError InvalidRoleName(string? role) => Error($"Vai trò '{role}' không hợp lệ.");
    public override IdentityError DuplicateRoleName(string role) => Error($"Vai trò '{role}' đã tồn tại.");
    public override IdentityError UserAlreadyHasPassword() => Error("Tài khoản đã có mật khẩu.");
    public override IdentityError UserLockoutNotEnabled() => Error("Tài khoản này chưa bật khóa đăng nhập.");
    public override IdentityError UserAlreadyInRole(string role) => Error($"Người dùng đã thuộc vai trò '{role}'.");
    public override IdentityError UserNotInRole(string role) => Error($"Người dùng chưa thuộc vai trò '{role}'.");
    public override IdentityError PasswordTooShort(int length) => Error($"Mật khẩu phải có ít nhất {length} ký tự.");
    public override IdentityError PasswordRequiresNonAlphanumeric() => Error("Mật khẩu phải có ít nhất một ký tự đặc biệt.");
    public override IdentityError PasswordRequiresDigit() => Error("Mật khẩu phải có ít nhất một chữ số.");
    public override IdentityError PasswordRequiresLower() => Error("Mật khẩu phải có ít nhất một chữ thường.");
    public override IdentityError PasswordRequiresUpper() => Error("Mật khẩu phải có ít nhất một chữ hoa.");
    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) => Error($"Mật khẩu phải có ít nhất {uniqueChars} ký tự khác nhau.");
    public override IdentityError RecoveryCodeRedemptionFailed() => Error("Mã khôi phục không hợp lệ.");

    private static IdentityError Error(string description) => new() { Description = description };
}

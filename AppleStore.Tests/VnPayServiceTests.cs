using AppleStore.Web.Services.Implementations;
using Xunit;

namespace AppleStore.Tests;

public class VnPayServiceTests
{
    [Fact]
    public void AmountIsConvertedToMinorUnitWithoutFloatingPointDrift()
    {
        Assert.Equal("3099000000", VnPayService.ToVnPayAmount(30_990_000m));
        Assert.Equal(30_990_000m, VnPayService.ParseVnPayAmount("3099000000"));
    }

    [Theory]
    [InlineData("00", "Giao dịch thành công.")]
    [InlineData("24", "Khách hàng đã hủy giao dịch.")]
    [InlineData("51", "Tài khoản không đủ số dư.")]
    public void MapsCommonVnPayCodes(string code, string expected) => Assert.Equal(expected, OrderService.VnPayErrorMessage(code));
}

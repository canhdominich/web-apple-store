# AppleStoreSolution

Website thương mại điện tử Apple Store xây dựng bằng ASP.NET Core MVC .NET 8, Razor Views, EF Core Code First, SQL Server, ASP.NET Core Identity, Bootstrap 5, JavaScript thuần và VNPay Sandbox. Frontend và backend nằm trong cùng ứng dụng MVC; không dùng React, SPA framework, Tailwind hay SQLite.

## Tính năng

- Client: trang chủ, danh mục, tìm kiếm/lọc/sắp xếp/phân trang, chi tiết sản phẩm và variant, giỏ hàng, đăng ký/đăng nhập, profile, đổi mật khẩu, lịch sử đơn hàng.
- Checkout: COD và VNPay Sandbox; số tiền luôn lấy server-side; callback kiểm tra HMACSHA512, TxnRef, amount và trạng thái; payment history, retry payment và callback idempotency.
- Admin: dashboard Chart.js, category CRUD/disable, product + variant + stock CRUD, upload ảnh an toàn, khách hàng lock/unlock và order/payment detail, cập nhật trạng thái có kiểm tra transition.
- Database: migration runtime có retry hữu hạn; seed idempotent gồm roles, admin demo, 6 categories và 15 products với nhiều variants.

## Kiến trúc

`Controller → Service → Repository/EF Core → SQL Server → Razor View`

Các service chính nằm trong `AppleStore.Web/Services`; `ProductRepository` chịu truy vấn catalog. VNPay chỉ được xử lý ở `VnPayService`, không để logic ký/xác thực trong controller. Tồn kho được reserve khi tạo order trong transaction Serializable. Khi thanh toán online thất bại/cancel, tồn kho được release đúng một lần; retry sẽ reserve lại nếu còn hàng.

## Chạy bằng Docker

Yêu cầu Docker Engine và Docker Compose v2.

```bash
cp .env.example .env
docker compose config
docker compose up --build
```

Ứng dụng chạy tại [http://localhost:5000](http://localhost:5000), SQL Server tại `localhost:1433`. Trong container connection string dùng `Server=db,1433`; không đổi thành localhost. SQL Server có healthcheck, web chỉ khởi động sau khi DB healthy. Application startup chạy `Database.MigrateAsync()` rồi seed data, không dùng `EnsureCreated()`.

Các lệnh thường dùng:

```bash
docker compose build
docker compose up -d --build
docker compose ps
docker compose logs -f web
docker compose logs -f db
docker compose restart
docker compose down
docker compose down -v       # xóa volume database, dùng khi muốn reset dữ liệu
```

Volume `mssql_data` giữ dữ liệu SQL Server; volume `product_images` giữ ảnh upload khi container được recreate.

## URL và tài khoản demo

- Client: `http://localhost:5000`
- Admin: `http://localhost:5000/Admin`
- Admin: `admin@applestore.local` / `Admin@123456`

Đổi password demo trước khi dùng production. User đăng ký mới được gán role `Customer`; password được Identity hash, không lưu plaintext.

## VNPay Sandbox

1. Đăng ký merchant/test account ở [VNPay Sandbox Developer Registration](https://sandbox.vnpayment.vn/devreg/).
2. Lấy `TmnCode` và `HashSecret` từ tài khoản sandbox.
3. Điền vào `.env` (không commit file này):

```env
VnPaySettings__TmnCode=YOUR_TMN_CODE
VnPaySettings__HashSecret=YOUR_HASH_SECRET
VnPaySettings__BaseUrl=https://sandbox.vnpayment.vn/paymentv2/vpcpay.html
VnPaySettings__ReturnUrl=http://localhost:5000/Checkout/PaymentCallback
```

4. `docker compose restart web` sau khi thay đổi cấu hình.
5. Chọn VNPay ở checkout. Website redirect người dùng sang VNPay; không nhập thông tin thẻ trên website. `vnp_Amount` được tính bằng `decimal` và gửi theo đơn vị `Amount × 100`.

Nếu sandbox cần callback public, chạy:

```bash
ngrok http 5000
```

Sau đó đặt `VnPaySettings__ReturnUrl=https://YOUR_NGROK_DOMAIN/Checkout/PaymentCallback` và cấu hình URL tương ứng trong sandbox nếu merchant yêu cầu whitelist. Không hard-code domain ngrok.

### Test card

Thông tin thẻ sandbox có thể thay đổi theo merchant/tài liệu hiện hành. Không bịa card number hoặc OTP trong source. Hãy xem mục test card trong merchant portal và tài liệu chính thức [VNPay Developer](https://sandbox.vnpayment.vn/apis/). README này cố ý không lưu credential hay dữ liệu thẻ thật.

## Security/payment notes

- Tất cả POST nghiệp vụ có antiforgery; admin có `[Authorize(Roles = "Admin")]`.
- Customer chỉ đọc được order thuộc chính mình; admin không expose `PasswordHash` hay security fields trong view.
- VNPay callback loại secure hash khỏi payload hash, sort key ordinal, HMACSHA512 và so sánh constant-time. Callback invalid không cập nhật DB.
- Callback kiểm tra payment tồn tại, TxnRef, amount, response code và transaction status. Callback lặp lại không trừ stock hay ghi nhận doanh thu lần nữa.
- Upload chỉ chấp nhận JPG/JPEG/PNG/WEBP, content type tương ứng, tối đa 5MB và filename server-generated.
- Không log `HashSecret`, password, connection string hay secret config.

## Cấu trúc chính

```text
AppleStore.Web/
├── Areas/Admin/                  # Dashboard, category, product, customer, order
├── Controllers/                  # Client MVC controllers
├── Data/                         # DbContext, seed, migration
├── Models/Entities                # Identity + business entities
├── Models/ViewModels              # Input/output models
├── Repositories/                 # Catalog repository
├── Services/                     # Cart, Order, VNPay services
├── Views/                        # Razor client views/partials
└── wwwroot/                      # CSS, JS, persistent product image mount
```

## Build/test

Khi có .NET SDK 8 local:

```bash
dotnet restore
dotnet build
dotnet test
```

Môi trường hiện tại của workspace không cài `dotnet` native, nên Dockerfile dùng SDK `mcr.microsoft.com/dotnet/sdk:8.0` để build đúng runtime. Trước khi phát hành, chạy `docker compose config`, `docker compose build` và kiểm tra `docker compose ps`, `docker compose logs -f web`, `docker compose logs -f db`.

## Hạn chế có chủ đích

Forgot password dùng Identity token và có thể test bằng development reset link hiển thị sau khi submit. Production cần nối SMTP/email provider vào action này để gửi link thay vì hiển thị link development. VNPay sandbox credentials và test cards được cung cấp bởi merchant portal, không nằm trong repository.

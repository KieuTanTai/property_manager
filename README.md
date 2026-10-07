# Backend - Tổng quan và quy ước phát triển

## 1. Tổng quan dự án

Đây là backend của hệ thống quản lý mặt bằng trung tâm mua sắm. Hệ thống phục vụ các chủ đầu tư, ban quản lý và người vận hành trung tâm mua sắm trong việc:

- Quản lý mặt bằng, vị trí, loại hình kinh doanh và trạng thái cho thuê.
- Quản lý sản phẩm/dịch vụ được phép kinh doanh tại từng mặt bằng.
- Quản lý hợp đồng thuê, điều khoản, vi phạm, hóa đơn và biên nhận.
- Quản lý tài khoản, hồ sơ người dùng, vai trò và quyền truy cập.
- Quản lý ticket hỗ trợ, thông báo và người nhận thông báo.

Backend được tổ chức theo kiến trúc **module-based microservices**. Mỗi module là một ứng dụng ASP.NET Core độc lập, có thể chạy, đóng gói và triển khai riêng. Các module sở hữu phần nghiệp vụ và persistence của mình; mã nguồn dùng chung được đặt trong project `Shared`.

## 2. Công nghệ và nền tảng

- .NET 10 và ASP.NET Core.
- Entity Framework Core cho truy cập dữ liệu.
- MySQL/MariaDB thông qua `MySql.EntityFrameworkCore`.
- Redis/`StackExchange.Redis` cho các nhu cầu cache.
- RabbitMQ cho khả năng tích hợp bất đồng bộ giữa các service.
- Swagger/OpenAPI cho tài liệu và kiểm thử API.
- Serilog và các extension logging trong `Shared`.
- Docker Compose/Dockerfile cho môi trường chạy service.

## 3. Cấu trúc cấp backend

```text
Backend/
├── Backend.sln.DotSettings.user
├── compose.yaml
├── Module/
│   ├── Gateway/
│   │   ├── Program.cs
│   │   ├── Gateway.csproj
│   │   └── Gateway.http
│   ├── Identity/
│   │   ├── Application/
│   │   ├── Credential/
│   │   ├── Infrastructure/
│   │   ├── Interfaces/
│   │   ├── Models/
│   │   ├── Presentation/
│   │   ├── Utils/
│   │   ├── Program.cs
│   │   └── Identity.csproj
│   ├── Premise/
│   │   ├── Application/
│   │   ├── Infrastructures/
│   │   ├── Interfaces/
│   │   ├── Models/
│   │   ├── Program.cs
│   │   └── Premise.csproj
│   ├── Contract/
│   │   ├── Application/
│   │   ├── Infrastructure/
│   │   ├── Interfaces/
│   │   ├── Models/
│   │   ├── Utils/
│   │   ├── Program.cs
│   │   └── Contract.csproj
│   └── TicketAndNotification/
│       ├── Infrastructure/
│       ├── Models/
│       ├── Program.cs
│       └── TicketAndNotification.csproj
└── Shared/
    ├── Interfaces/
    ├── Logging/
    ├── ModelHelper/
    ├── Persistence/
    ├── Validate/
    └── Shared.csproj
```

`bin/` và `obj/` là thư mục sinh bởi quá trình build, không phải một phần của cấu trúc mã nguồn và không được dùng để đặt code nghiệp vụ.

## 4. Các module hiện có

### Gateway

Điểm vào tập trung cho client và là nơi phù hợp để định tuyến request đến các service. Gateway có `Program.cs` và project riêng. Các endpoint thử nghiệm hoặc endpoint trung gian phải được thay thế bằng route thực tế khi tích hợp các service.

### Identity

Phụ trách xác thực và phân quyền:

- Account và trạng thái tài khoản.
- User profile.
- Role, permission và additional permission.
- Login, register, đổi mật khẩu, vô hiệu hóa tài khoản.
- Cache role/permission và các helper liên quan API/account.

Đây là module đầy đủ nhất và là mẫu tham khảo cho cách tổ chức `Application`, `Infrastructure`, `Interfaces`, `Models`, `Presentation` và `Utils`.

### Premise

Phụ trách nghiệp vụ mặt bằng:

- Location.
- Premise và rented premise.
- Business type.
- Media của mặt bằng.
- Quan hệ premise-business type/product-business type.
- Whitelist sản phẩm.

### Contract

Phụ trách nghiệp vụ hợp đồng và tài chính liên quan:

- Contract và contract regulation.
- Contract violation.
- Regulation.
- Monthly invoice, invoice detail và receipt.
- Enum trạng thái hợp đồng/hóa đơn trong `Utils/Enum`.

### TicketAndNotification

Phụ trách ticket hỗ trợ và thông báo:

- Ticket.
- Ticket media.
- Notification.
- Notification recipient.

Module đã có DbContext, entity configuration và repository infrastructure; các lớp application/interface/presentation có thể được bổ sung theo cấu trúc chuẩn khi API nghiệp vụ được mở rộng.

## 5. Cấu trúc chuẩn của một module

Một module mới nên theo cấu trúc sau:

```text
<Module>/
├── Application/
│   ├── <Feature>Application.cs
│   └── ...
├── Credential/                         # tùy module
│   └── ...
├── Infrastructure/
│   ├── Cache/                          # cache, pool, cache adapter
│   ├── DIContainer/
│   │   ├── Collection<Module>Application.cs
│   │   └── Collection<Module>Repository.cs
│   ├── Persistence/
│   │   ├── Configurations/
│   │   │   └── Context<Entity>Configuration.cs
│   │   └── DbContext/
│   │       └── <Module>DbContext.cs
│   └── Repository/
│       ├── <Feature>Repository/
│       │   └── <Entity>Repository.cs
│       └── Ef<Module>UnitOfWork.cs
├── Interfaces/
│   ├── IApplication/
│   │   └── I<Feature>Application.cs
│   ├── IRepository/
│   │   └── I<Entity>Repository.cs
│   └── <Feature>/                       # nhóm interface đặc thù nếu cần
├── Models/
│   ├── <BoundedContext>/
│   │   └── <Entity>Model.cs
│   └── ...
├── Presentation/
│   ├── Controller/
│   │   ├── <Feature>Controller.cs
│   │   └── CustomControllerBase.cs
│   └── Record/
│       ├── <Feature>/
│       │   ├── Record<Create><Feature>Request.cs
│       │   └── Record<Feature>Response.cs
│       └── ...
├── Utils/
│   ├── Enum/
│   │   └── E<Domain>Status.cs
│   ├── <Feature>Helper.cs
│   └── ...
├── Program.cs
├── <Module>.csproj
└── <Module>.http
```

### Application

Chứa lớp điều phối use case và nghiệp vụ ở cấp ứng dụng. Application nhận input đã được kiểm tra cơ bản từ presentation, gọi repository/interface phù hợp, áp dụng quy tắc nghiệp vụ và trả kết quả cho controller. Không đặt truy vấn EF Core trực tiếp trong lớp application nếu truy vấn đó thuộc trách nhiệm repository.

Ví dụ hiện có: `Identity/Application/AccountApplication.cs`, `AuthorizationApplication.cs` và `UserProfileApplication.cs`.

### Infrastructure

Chứa phần triển khai kỹ thuật của module:

- `DIContainer`: các extension đăng ký dependency injection cho application và repository.
- `Persistence/DbContext`: `DbContext` của riêng module.
- `Persistence/Configurations`: ánh xạ model sang bảng/cột, khóa, index, quan hệ, default value và delete behavior bằng `IEntityTypeConfiguration<T>`.
- `Repository`: triển khai các interface repository, truy vấn EF Core, thao tác entity và unit of work.
- `Cache`: cache/pool, nếu module có nhu cầu.

Infrastructure được phép phụ thuộc vào EF Core, Redis, RabbitMQ hoặc thư viện hạ tầng; các layer khác nên phụ thuộc vào abstraction trong `Interfaces`.

### Interfaces

Chứa các interface định nghĩa **hợp đồng cấu trúc** giữa các layer:

- `IApplication`: public contract của use case application.
- `IRepository`: thao tác đọc/ghi dữ liệu và truy vấn theo domain.
- Interface helper hoặc interface đặc thù của module, chẳng hạn `IAccountHelper`, `IApiHelper`.

Interface không chứa logic triển khai. Tên interface bắt đầu bằng `I`, phương thức cần thể hiện rõ mục đích và hỗ trợ `CancellationToken` cho thao tác bất đồng bộ.

### Models

Chứa model/entity của module, thường phân chia tiếp theo bounded context hoặc feature như `Account`, `Role`, `Contract`, `Invoice`, `Premise`, `Notification`.

Quy ước model:

- Model phải mô tả rõ domain và dùng annotation khi annotation đó có ý nghĩa trực tiếp với dữ liệu/validation, ví dụ `[MaxLength(255)]`, `[Required]`, `[Key]` khi phù hợp.
- Ràng buộc mapping chuyên sâu (tên bảng/cột, index, quan hệ, delete behavior) đặt tại `Infrastructure/Persistence/Configurations`, không dồn toàn bộ vào model.
- Model có thể có hoặc không có navigation properties. Chỉ thêm navigation property khi domain hoặc use case cần quan hệ đó; không thêm chỉ để tiện truy vấn.
- Các setter nên được giới hạn (`private set`/`init`) và thay đổi trạng thái qua method có kiểm tra nghiệp vụ khi cần.
- Constructor và `ModelFieldGuard` được dùng để bảo vệ giá trị bắt buộc, độ dài và invariant của model.
- Không hardcode giá trị nghiệp vụ, quyền, trạng thái, giới hạn hoặc cấu hình trong code. Giá trị phải đến từ request, cấu hình, database, enum/domain constant hoặc abstraction phù hợp.
- Ngoại lệ được chấp nhận cho thời điểm khởi tạo như `DateTime.Now` trong model hiện tại; không dùng thời gian cố định hardcode để giả lập dữ liệu.
- Model dùng cho persistence không mặc nhiên là DTO của API. Khi request/response có hình dạng khác entity, tạo record riêng trong `Presentation/Record`.

### Presentation

Chứa lớp giao tiếp HTTP:

- `Controller`: định nghĩa route, binding request, kiểm tra input ở mức API, gọi application, chuyển kết quả thành HTTP response và ghi log theo convention.
- `Record`: DTO dạng `record` cho request/response. Nên chia theo feature (`Account`, `Profile`, `Authorization`) và đặt tên rõ hướng dữ liệu.
- `CustomControllerBase`: base controller dùng chung cho module nếu cần.

Controller không chứa truy vấn database và không nên chứa nghiệp vụ domain phức tạp. Các lỗi input phải trả response phù hợp và được log theo chuẩn của module; không dùng catch rộng để âm thầm bỏ qua lỗi.

> Một số tài liệu cũ có thể gọi nhóm này là “Persistence chứa controller và DTO”. Theo cấu trúc hiện tại, controller và DTO nằm ở `Presentation`; `Persistence`/`Infrastructure/Persistence` dành cho DbContext, configuration và data access.

### Persistence

Trong cấu trúc hiện tại, persistence chủ yếu nằm dưới `Infrastructure/Persistence`. Đây là nơi chứa:

- `DbContext` của service.
- Entity configurations.
- Các thiết lập mapping, migration/seeding nếu module có sử dụng.

Controller và DTO không đặt ở đây. Nếu một module tách `Persistence` ở cấp root, thư mục đó vẫn chỉ nên chứa thành phần truy cập/lưu trữ dữ liệu, không chứa HTTP endpoint.

### Utils

Chứa các tiện ích hỗ trợ module, không phải nơi gom tùy tiện mọi logic:

- Helper kiểm tra/chuẩn hóa input và nghiệp vụ nhỏ, ví dụ email/password hoặc mapping API.
- Enum theo domain như `EContractStatus`, `EInvoiceStatus`.
- Hashing, permission map, constant có tên rõ nghĩa hoặc adapter kỹ thuật.

Utility phải có trách nhiệm hẹp, dễ kiểm thử và không truy cập trực tiếp `DbContext` nếu trách nhiệm đó thuộc repository/application.

### Credential

Chứa kiểu dữ liệu hoặc placeholder liên quan credential/secret khi module cần. Không commit password, token, connection string hoặc secret thật vào source control; dùng configuration, secret store hoặc environment variables.

### Program.cs và project file

`Program.cs` là composition root của service: cấu hình authentication/authorization, controller, Swagger, middleware và endpoint mapping. Mỗi module có `.csproj` riêng, tham chiếu `Shared` khi dùng abstraction/extension dùng chung.

## 6. Shared project

`Backend/Shared` là project chứa các extension, abstraction và thành phần dùng chung cho nhiều module. Shared không chứa nghiệp vụ riêng của Identity, Premise, Contract hay TicketAndNotification.

```text
Shared/
├── Interfaces/
│   ├── IBaseAssociativeRepository.cs
│   ├── IBasePostRepository.cs
│   ├── IBaseReadRepository.cs
│   └── IUnitOfWork.cs
├── Logging/
│   ├── ILogPool.cs
│   ├── LogEntry.cs
│   ├── LogExtensions.cs
│   └── LogPool.cs
├── ModelHelper/
│   └── ModelFieldGuard.cs
├── Persistence/
│   ├── Record/
│   │   ├── Auth/
│   │   │   ├── RecordAuthRequest.cs
│   │   │   └── RecordAuthResponse.cs
│   │   └── RecordBaseCursorPage.cs
│   └── SharedGetApplyPagingRepository.cs
├── Validate/
│   └── SharedValidationMethods.cs
└── Shared.csproj
```

Vai trò chính:

- `Interfaces`: interface generic dùng lại cho read/post/associative repository và unit of work.
- `Logging`: abstraction, entry, pool và extension để các module ghi log thống nhất theo module/layer.
- `ModelHelper`: guard dùng chung để kiểm tra field bắt buộc và độ dài.
- `Persistence/Record`: record dùng chung cho authentication và cursor pagination.
- `SharedGetApplyPagingRepository`: logic áp dụng cursor paging dùng lại giữa các repository.
- `Validate`: các phương thức validation không thuộc riêng một module.

Khi thêm code vào `Shared`, phải bảo đảm code thật sự dùng được bởi từ hai module trở lên hoặc là infrastructure cross-cutting. Không đưa entity, controller, use case hay policy riêng của một module vào Shared.

## 7. Luồng xử lý request chuẩn

```text
Client
  -> Gateway (nếu request đi qua gateway)
  -> Controller / Presentation
  -> Record binding và validation input
  -> Application / use case
  -> Interface repository
  -> Infrastructure repository
  -> DbContext + Persistence configuration
  -> Database
```

Kết quả đi theo chiều ngược lại. Logging dùng abstraction của `Shared`; cache/message broker chỉ được gọi qua thành phần phù hợp của module hoặc abstraction dùng chung, không để controller tự thao tác trực tiếp.

## 8. Quy ước mã nguồn

### Namespace và tên file

- Namespace phản ánh đúng module và đường dẫn thư mục, ví dụ `Identity.Infrastructure.Repository.AccountRepository`.
- Class, method và property dùng PascalCase; biến/tham số dùng camelCase.
- Interface bắt đầu bằng `I`; application/repository/controller kết thúc bằng hậu tố tương ứng.
- Entity persistence kết thúc bằng `Model` theo convention hiện tại.
- Request/response HTTP dùng `Record...Request` và `Record...Response`.
- Tên DbContext theo `<Module>DbContext`; unit of work EF theo `Ef<Module>UnitOfWork`.

### Dependency và ranh giới layer

- Presentation gọi Application.
- Application dùng Interfaces.
- Infrastructure triển khai Interfaces và làm việc với external systems/database.
- Models không phụ thuộc controller.
- Shared chỉ cung cấp thành phần dùng chung, không tham chiếu ngược vào module.
- Không truy cập trực tiếp repository cụ thể từ controller nếu use case cần được điều phối qua application.

### Dữ liệu và validation

- Dùng async API của EF Core và truyền `CancellationToken` xuyên suốt khi có thể.
- Dùng `AsNoTracking` cho truy vấn chỉ đọc.
- Dùng `AsSplitQuery` khi include nhiều collection có nguy cơ tạo Cartesian explosion.
- Kiểm tra page size, email, password, identifier và collection rỗng ở ranh giới phù hợp.
- Ràng buộc database được thể hiện rõ bằng configuration: key, required, max length, unique index, timestamp, quan hệ và delete behavior.
- Không hardcode connection string, secret, password, permission runtime hoặc giá trị nghiệp vụ.

### Logging và lỗi

- Ghi log theo module và layer bằng `ILogPool`/`LogExtensions` hiện có.
- Log validation bị từ chối ở mức warning; log lỗi kỹ thuật kèm exception ở mức error.
- Không nuốt exception bằng catch rỗng hoặc trả về kết quả thành công giả.
- `OperationCanceledException` phải được xử lý nhất quán với convention của API và không được biến thành lỗi không rõ nguyên nhân.

### API

- Dùng attribute routing và `[ApiController]`.
- Dùng `[RequireHttps]` cho endpoint cần HTTPS theo convention hiện tại.
- API request/response dùng record riêng, không expose entity nếu không cần.
- Response status phải phản ánh kết quả: input không hợp lệ, không tìm thấy, thành công hoặc lỗi xử lý.
- Swagger chỉ là công cụ mô tả/kiểm thử; không đặt nghiệp vụ vào cấu hình Swagger.

## 9. Khi tạo module hoặc feature mới

1. Xác định bounded context, dữ liệu sở hữu và API của module; không dùng chung DbContext giữa các module.
2. Tạo model và constructor/invariant cần thiết; quyết định rõ navigation properties có thực sự cần hay không.
3. Tạo interface application/repository trước khi viết implementation.
4. Tạo DbContext và configuration cho key, column, index, timestamp và quan hệ.
5. Viết repository trong `Infrastructure`, application trong `Application`, rồi đăng ký DI trong `DIContainer`.
6. Tạo record request/response và controller trong `Presentation`.
7. Tách helper/enum vào `Utils`; chỉ đưa abstraction thực sự dùng chung vào `Shared`.
8. Cập nhật `.csproj`, compose/Docker và tài liệu API nếu service cần được chạy độc lập.


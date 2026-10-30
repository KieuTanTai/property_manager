namespace Shared.Enum
{
    // Mapping quyền mặc định theo role. Các quyền truy cập dữ liệu "own" vẫn phải được
    // giới hạn theo AccountId trong truy vấn/API, không chỉ dựa vào permission claim.
    public static class SystemRolePermission
    {
        // Customer là role duy nhất được đăng ký thuê mặt bằng. Các quyền còn lại chỉ
        // cho phép xem/cập nhật tài nguyên thuộc chính tài khoản này.
        public static IReadOnlySet<ESystemPermissionCode> Customer { get; } = new HashSet<ESystemPermissionCode>
        {
            ESystemPermissionCode.IdentityReadOwn,
            ESystemPermissionCode.PremiseRent,
            ESystemPermissionCode.PremiseReturn,
            ESystemPermissionCode.PremiseReadList,
            ESystemPermissionCode.PremiseReadOwnRented,
            ESystemPermissionCode.ContractSign,
            ESystemPermissionCode.ContractTerminate,
            ESystemPermissionCode.ContractCancel,
            ESystemPermissionCode.ContractReadOwn,
            ESystemPermissionCode.ContractRegulationReadOwn,
            ESystemPermissionCode.ContractViolationReadOwn,
            ESystemPermissionCode.ContractViolationPay,
            ESystemPermissionCode.ContractViolationAppeal,
            ESystemPermissionCode.InvoiceReadOwn,
            ESystemPermissionCode.InvoicePay,
            ESystemPermissionCode.ReceiptReadOwn,
            ESystemPermissionCode.TicketReadOwn,
            ESystemPermissionCode.NotificationReadOwn
        };

        // Employee xem whitelist, cập nhật trạng thái premise, xử lý hợp đồng và hóa đơn.
        // Không cấp quyền đọc toàn bộ ticket/notification; dữ liệu riêng dùng quyền own.
        public static IReadOnlySet<ESystemPermissionCode> Employee { get; } = new HashSet<ESystemPermissionCode>
        {
            ESystemPermissionCode.PremiseWhitelistRead,
            ESystemPermissionCode.PremiseStatusUpdate,
            ESystemPermissionCode.ContractReadList,
            ESystemPermissionCode.ContractCreate,
            ESystemPermissionCode.ContractUpdate,
            ESystemPermissionCode.InvoiceReadList,
            ESystemPermissionCode.InvoiceCreate,
            ESystemPermissionCode.InvoiceUpdate,
            ESystemPermissionCode.InvoiceStatusUpdate,
            ESystemPermissionCode.TicketReadOwn,
            ESystemPermissionCode.NotificationReadOwn
        };

        // Inspector chỉ đọc premise/whitelist, hợp đồng, hóa đơn, receipt; được tạo
        // ticket vi phạm. Ticket và notification chỉ xem được dữ liệu của chính mình.
        public static IReadOnlySet<ESystemPermissionCode> Inspector { get; } = new HashSet<ESystemPermissionCode>
        {
            ESystemPermissionCode.PremiseReadList,
            ESystemPermissionCode.PremiseWhitelistRead,
            ESystemPermissionCode.ContractReadList,
            ESystemPermissionCode.ContractRegulationRead,
            ESystemPermissionCode.ContractViolationRead,
            ESystemPermissionCode.InvoiceReadList,
            ESystemPermissionCode.ReceiptReadList,
            ESystemPermissionCode.TicketViolationCreate,
            ESystemPermissionCode.TicketReadOwn,
            ESystemPermissionCode.NotificationReadOwn
        };

        // Admin có toàn bộ quyền quản trị, ngoại trừ đăng ký thuê premise (chỉ Customer).
        // Dùng enum hiện tại để quyền mới được cấp mặc định cho Admin.
        public static IReadOnlySet<ESystemPermissionCode> Admin { get; } =
            System.Enum.GetValues<ESystemPermissionCode>()
                .Where(permission => permission != ESystemPermissionCode.PremiseRent)
                .ToHashSet();

        // Manager tương đương Admin ngoài các thao tác tạo/sửa/xóa định nghĩa role,
        // permission và đăng ký thuê. Quyền gán role/permission cho account vẫn có;
        // API/service phải giới hạn account đích chỉ là Inspector hoặc Employee.
        public static IReadOnlySet<ESystemPermissionCode> Manager { get; } =
            Admin.Where(permission => permission is not ESystemPermissionCode.IdentityRoleCreate
                                      and not ESystemPermissionCode.IdentityRoleUpdate
                                      and not ESystemPermissionCode.IdentityRoleRevoke
                                      and not ESystemPermissionCode.IdentityPermissionCreate
                                      and not ESystemPermissionCode.IdentityPermissionUpdate
                                      and not ESystemPermissionCode.IdentityPermissionRevoke)
                .ToHashSet();

        public static IReadOnlySet<ESystemPermissionCode> GetPermissions(ESystemRoleCode roleCode)
        {
            return roleCode switch
            {
                ESystemRoleCode.Customer => Customer,
                ESystemRoleCode.Employee => Employee,
                ESystemRoleCode.Inspector => Inspector,
                ESystemRoleCode.Manager => Manager,
                ESystemRoleCode.Admin => Admin,
                _ => throw new ArgumentOutOfRangeException(nameof(roleCode), roleCode, null)
            };
        }
    }
}

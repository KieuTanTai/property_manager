namespace Shared.Enum
{
    public static class SystemRolePermission
    {
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

        public static IReadOnlySet<ESystemPermissionCode> Admin { get; } =
            System.Enum.GetValues<ESystemPermissionCode>()
                .Where(permission => permission != ESystemPermissionCode.PremiseRent)
                .ToHashSet();

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
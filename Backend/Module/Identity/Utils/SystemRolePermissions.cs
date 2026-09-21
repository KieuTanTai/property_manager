using Identity.Utils.Enum;

namespace Identity.Utils
{
    // ! WARNING: Do not change the order of the enum values, as they are used in the database and changing the order will break existing data,
    // STRING TYPE IN DB IS USING '_' AS SEPARATOR, BECAREFUL WHEN CONVERTING TO STRING, ADD SEPARATOR '_' WHEN CONVERTING TO STRING, AND REMOVE '_' WHEN CONVERTING FROM STRING TO ENUM.
    public static class SystemRolePermission
    {
        #region Employee

        public static IReadOnlySet<ESystemPermissionCode> Employee { get; } =
            new HashSet<ESystemPermissionCode>
            {
                ESystemPermissionCode.PremiseStatusUpdate,
                ESystemPermissionCode.ContractCreate,
                ESystemPermissionCode.ContractStatusUpdate,
                ESystemPermissionCode.ContractTerminateDateUpdate,
                ESystemPermissionCode.ContractViolationStatusUpdate,
                ESystemPermissionCode.InvoiceReadList,
                ESystemPermissionCode.InvoiceStatusUpdate,
                ESystemPermissionCode.ReceiptCreate,
                ESystemPermissionCode.ReceiptReadList
            };

        #endregion

        #region Inspector

        public static IReadOnlySet<ESystemPermissionCode> Inspector { get; } =
            new HashSet<ESystemPermissionCode>
            {
                ESystemPermissionCode.ContractReadList,
                ESystemPermissionCode.ContractRegulationRead,
                ESystemPermissionCode.ContractViolationRead
            };

        #endregion

        #region Manager

        public static IReadOnlySet<ESystemPermissionCode> Manager { get; } =
            new HashSet<ESystemPermissionCode>
            {
                ESystemPermissionCode.IdentityReadList,
                ESystemPermissionCode.IdentityStatusUpdate,
                ESystemPermissionCode.IdentityCreateEmployee,
                ESystemPermissionCode.IdentityCreateInspector,
                ESystemPermissionCode.IdentityPermissionCreate,
                ESystemPermissionCode.IdentityPermissionAssign,
                ESystemPermissionCode.IdentityPermissionUpdate,
                ESystemPermissionCode.IdentityPermissionRevoke,
                ESystemPermissionCode.IdentityRoleCreate,
                ESystemPermissionCode.IdentityRoleAssign,
                ESystemPermissionCode.IdentityRoleUpdate,
                ESystemPermissionCode.IdentityRoleRevoke,
                ESystemPermissionCode.PremiseCreate,
                ESystemPermissionCode.PremiseUpdate,
                ESystemPermissionCode.PremiseBusinessCreate,
                ESystemPermissionCode.PremiseBusinessAssign,
                ESystemPermissionCode.PremiseBusinessRevoke,
                ESystemPermissionCode.PremiseWhitelistCreate,
                ESystemPermissionCode.PremiseWhitelistRevoke,
                ESystemPermissionCode.PremiseLocationCreate,
                ESystemPermissionCode.PremiseLocationUpdate,
                ESystemPermissionCode.PremiseLocationRevoke,
                ESystemPermissionCode.PremiseStatusUpdate,
                ESystemPermissionCode.PremiseMediaDeleteHard,
                ESystemPermissionCode.ContractReadList,
                ESystemPermissionCode.ContractApprove,
                ESystemPermissionCode.ContractUpdate,
                ESystemPermissionCode.ContractStatusUpdate,
                ESystemPermissionCode.ContractTerminateDateUpdate,
                ESystemPermissionCode.ContractRegulationCreate,
                ESystemPermissionCode.ContractRegulationUpdate,
                ESystemPermissionCode.ContractRegulationRevoke,
                ESystemPermissionCode.ContractViolationRead,
                ESystemPermissionCode.ContractViolationCreate,
                ESystemPermissionCode.ContractViolationUpdate,
                ESystemPermissionCode.InvoiceReadList,
                ESystemPermissionCode.InvoiceCreate,
                ESystemPermissionCode.InvoiceUpdate,
                ESystemPermissionCode.ReceiptReadList,
                ESystemPermissionCode.TicketReadList,
                ESystemPermissionCode.NotificationDeleteHard
            };

        #endregion

        #region Customer

        public static IReadOnlySet<ESystemPermissionCode> Customer { get; } =
            new HashSet<ESystemPermissionCode>
            {
                ESystemPermissionCode.PremiseRent,
                ESystemPermissionCode.PremiseReturn,
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
                ESystemPermissionCode.ReceiptReadOwn
            };

        #endregion

        #region Admin

        public static IReadOnlySet<ESystemPermissionCode> Admin { get; } =
            new HashSet<ESystemPermissionCode>();

        #endregion

        #region Helper

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

        #endregion
    }
}

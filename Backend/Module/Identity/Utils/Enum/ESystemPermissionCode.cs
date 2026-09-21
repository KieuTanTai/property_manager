namespace Identity.Utils.Enum
{
    // ! WARNING: Do not change the order of the enum values, as they are used in the database and changing the order will break existing data,
    // STRING TYPE IN DB IS USING '_' AS SEPARATOR, BECAREFUL WHEN CONVERTING TO STRING, ADD SEPARATOR '_' WHEN CONVERTING TO STRING, AND REMOVE '_' WHEN CONVERTING FROM STRING TO ENUM.
    public enum ESystemPermissionCode
    {
        #region Identity

        IdentityReadList,
        IdentityStatusUpdate,
        IdentityCreateEmployee,
        IdentityCreateInspector,
        IdentityPermissionCreate,
        IdentityPermissionAssign,
        IdentityPermissionUpdate,
        IdentityPermissionRevoke,
        IdentityRoleCreate,
        IdentityRoleAssign,
        IdentityRoleUpdate,
        IdentityRoleRevoke,

        #endregion

        #region Premise

        PremiseCreate,
        PremiseUpdate,
        PremiseBusinessCreate,
        PremiseBusinessAssign,
        PremiseBusinessRevoke,
        PremiseWhitelistCreate,
        PremiseWhitelistRevoke,
        PremiseLocationCreate,
        PremiseLocationUpdate,
        PremiseLocationRevoke,
        PremiseStatusUpdate,
        PremiseMediaDeleteHard,
        PremiseRent,
        PremiseReturn,
        PremiseReadOwnRented,

        #endregion

        #region Contract

        ContractReadList,
        ContractCreate,
        ContractApprove,
        ContractUpdate,
        ContractStatusUpdate,
        ContractTerminateDateUpdate,
        ContractRegulationRead,
        ContractRegulationCreate,
        ContractRegulationUpdate,
        ContractRegulationRevoke,
        ContractViolationRead,
        ContractViolationCreate,
        ContractViolationUpdate,
        ContractViolationStatusUpdate,
        ContractSign,
        ContractTerminate,
        ContractCancel,
        ContractReadOwn,
        ContractRegulationReadOwn,
        ContractViolationReadOwn,
        ContractViolationPay,
        ContractViolationAppeal,

        #endregion

        #region Invoice

        InvoiceReadList,
        InvoiceCreate,
        InvoiceUpdate,
        InvoiceStatusUpdate,
        InvoiceReadOwn,
        InvoicePay,

        #endregion

        #region Receipt

        ReceiptReadList,
        ReceiptCreate,
        ReceiptReadOwn,

        #endregion

        #region Ticket

        TicketReadList,
        NotificationDeleteHard

        #endregion
    }
}
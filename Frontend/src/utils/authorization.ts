export const PERMISSIONS = {
    identityReadList: "IDENTITY_READ_LIST",
    identityStatusUpdate: "IDENTITY_STATUS_UPDATE",
    identityCreateEmployee: "IDENTITY_CREATE_EMPLOYEE",
    identityCreateInspector: "IDENTITY_CREATE_INSPECTOR",
    identityPermissionAssign: "IDENTITY_PERMISSION_ASSIGN",
    identityPermissionUpdate: "IDENTITY_PERMISSION_UPDATE",
    identityRoleAssign: "IDENTITY_ROLE_ASSIGN",
    identityRoleUpdate: "IDENTITY_ROLE_UPDATE",
    premiseCreate: "PREMISE_CREATE",
    premiseUpdate: "PREMISE_UPDATE",
    premiseWhitelistRead: "PREMISE_WHITELIST_READ",
    premiseWhitelistCreate: "PREMISE_WHITELIST_CREATE",
    premiseWhitelistRevoke: "PREMISE_WHITELIST_REVOKE",
    premiseLocationCreate: "PREMISE_LOCATION_CREATE",
    premiseLocationUpdate: "PREMISE_LOCATION_UPDATE",
    premiseLocationRevoke: "PREMISE_LOCATION_REVOKE",
    premiseBusinessCreate: "PREMISE_BUSINESS_CREATE",
    premiseBusinessAssign: "PREMISE_BUSINESS_ASSIGN",
    premiseBusinessRevoke: "PREMISE_BUSINESS_REVOKE",
    premiseReadList: "PREMISE_READ_LIST",
    premiseReadOwnRented: "PREMISE_READ_OWN_RENTED",
    premiseStatusUpdate: "PREMISE_STATUS_UPDATE",
    contractReadList: "CONTRACT_READ_LIST",
    contractReadOwn: "CONTRACT_READ_OWN",
    contractCreate: "CONTRACT_CREATE",
    contractUpdate: "CONTRACT_UPDATE",
    contractRegulationRead: "CONTRACT_REGULATION_READ",
    contractRegulationReadOwn: "CONTRACT_REGULATION_READ_OWN",
    contractRegulationCreate: "CONTRACT_REGULATION_CREATE",
    contractRegulationUpdate: "CONTRACT_REGULATION_UPDATE",
    contractRegulationRevoke: "CONTRACT_REGULATION_REVOKE",
    contractViolationRead: "CONTRACT_VIOLATION_READ",
    contractViolationReadOwn: "CONTRACT_VIOLATION_READ_OWN",
    invoiceReadList: "INVOICE_READ_LIST",
    invoiceReadOwn: "INVOICE_READ_OWN",
    invoiceCreate: "INVOICE_CREATE",
    invoiceUpdate: "INVOICE_UPDATE",
    invoiceStatusUpdate: "INVOICE_STATUS_UPDATE",
    receiptCreate: "RECEIPT_CREATE",
    ticketReadList: "TICKET_READ_LIST",
    ticketReadOwn: "TICKET_READ_OWN",
    ticketViolationCreate: "TICKET_VIOLATION_CREATE",
    notificationReadOwn: "NOTIFICATION_READ_OWN",
    notificationDeleteHard: "NOTIFICATION_DELETE_HARD",
} as const

// Admin is unrestricted in the UI except for customer-only premise rental.
const ADMIN_PREVENT_PERMISSIONS = new Set(["PREMISE_RENT"])

function normalizeCode(code: string) {
    return code.toUpperCase().replace(/_/g, "")
}

export function hasPermission(permissionCodes: string[], permission: string, roleCodes: string[] = []) {
    const target = normalizeCode(permission)
    if (hasRole(roleCodes, "ADMIN")) return !ADMIN_PREVENT_PERMISSIONS.has(target)
    return permissionCodes.some((code) => normalizeCode(code) === target)
}

export function hasAnyPermission(permissionCodes: string[], ...permissions: string[]) {
    return permissions.some((permission) => hasPermission(permissionCodes, permission))
}

export function hasAnyPermissionForRole(permissionCodes: string[], roleCodes: string[], ...permissions: string[]) {
    return permissions.some((permission) => hasPermission(permissionCodes, permission, roleCodes))
}

export function hasRole(roleCodes: string[], ...roles: string[]) {
    const assigned = new Set(roleCodes.map(normalizeCode))
    return roles.some((role) => assigned.has(normalizeCode(role)))
}

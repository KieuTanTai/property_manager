import {useEffect, useState} from "react"
import {identityApi} from "@/api/identity"
import type {PermissionModel, RoleModel, UserProfileModel} from "@/types/identity"
import {dateLabel} from "@/scripts/identity"
import type {ManagerModule as Module, ManagerWorkspaceProps as Props} from "../../types/manager"
import {AccountsModule} from "./AccountsModule"
import {ContractsModule} from "./ContractsModule"
import {ContactsModule} from "./ContactsModule"
import {Dashboard} from "./Dashboard"
import {InvoicesModule} from "./InvoicesModule"
import {PremisesModule} from "./PremisesModule"
import {RegulationsModule} from "./RegulationsModule"
import {StatisticsModule} from "./StatisticsModule"
import {TicketsModule} from "./TicketsModule"
import {ViolationsModule} from "./ViolationsModule"
import {Modal} from "./shared"
import {hasAnyPermissionForRole, hasPermission, hasRole, PERMISSIONS} from "@/utils/authorization"

const NAV: {
    id: Module;
    label: string;
    icon: string;
    canView: (permissions: string[], roles: string[], management: boolean) => boolean
}[] = [
    {id: "dashboard", label: "Tổng quan", icon: "dashboard", canView: (_permissions, _roles, management) => management},
    {
        id: "statistics",
        label: "Thống kê",
        icon: "monitoring",
        canView: (_permissions, _roles, management) => management
    },
    {
        id: "accounts",
        label: "Tài khoản",
        icon: "group",
        canView: (permissions, roles) => hasPermission(permissions, PERMISSIONS.identityReadList, roles)
    },
    {
        id: "contracts",
        label: "Hợp đồng",
        icon: "description",
        canView: (permissions, roles) => hasAnyPermissionForRole(permissions, roles, PERMISSIONS.contractReadList, PERMISSIONS.contractReadOwn)
    },
    {
        id: "invoices",
        label: "Hóa đơn",
        icon: "receipt_long",
        canView: (permissions, roles) => hasAnyPermissionForRole(permissions, roles, PERMISSIONS.invoiceReadList, PERMISSIONS.invoiceReadOwn)
    },
    {
        id: "regulations",
        label: "Quy định",
        icon: "gavel",
        canView: (permissions, roles) => hasAnyPermissionForRole(permissions, roles, PERMISSIONS.contractRegulationRead, PERMISSIONS.contractRegulationReadOwn)
    },
    {
        id: "violations",
        label: "Vi phạm",
        icon: "warning",
        canView: (permissions, roles) => hasAnyPermissionForRole(permissions, roles, PERMISSIONS.contractViolationRead, PERMISSIONS.contractViolationReadOwn)
    },
    {
        id: "tickets",
        label: "Yêu cầu",
        icon: "confirmation_number",
        canView: (permissions, roles) => hasAnyPermissionForRole(permissions, roles, PERMISSIONS.ticketReadList, PERMISSIONS.ticketReadOwn, PERMISSIONS.ticketViolationCreate)
    },
    {
        id: "notifications",
        label: "Thông báo",
        icon: "notifications",
        canView: (permissions, roles) => hasPermission(permissions, PERMISSIONS.notificationReadOwn, roles)
    },
    {
        id: "premises",
        label: "Mặt bằng",
        icon: "storefront",
        canView: (permissions, roles) => hasAnyPermissionForRole(permissions, roles, PERMISSIONS.premiseReadList, PERMISSIONS.premiseReadOwnRented, PERMISSIONS.premiseStatusUpdate, PERMISSIONS.premiseWhitelistRead, PERMISSIONS.premiseCreate, PERMISSIONS.premiseUpdate)
    },
    {
        id: "contacts",
        label: "Thông tin liên hệ",
        icon: "contact_phone",
        canView: (_permissions, _roles, management) => management
    },
]

export function ManagerWorkspace({accountId, email, roleCodes, permissionCodes, onLogout}: Props) {
    const [active, setActive] = useState<Module>("dashboard")
    const [isAccountDetailsOpen, setIsAccountDetailsOpen] = useState(false)
    const [profile, setProfile] = useState<UserProfileModel | null>(null)
    const [roles, setRoles] = useState<RoleModel[]>([])
    const [permissions, setPermissions] = useState<PermissionModel[]>([])
    const [profileLoading, setProfileLoading] = useState(false)
    const [profileError, setProfileError] = useState<string | null>(null)
    const isManagementRole = hasRole(roleCodes, "ADMIN", "MANAGER")
    const can = (permission: string) => hasPermission(permissionCodes, permission, roleCodes)
    const canAny = (...permissions: string[]) => hasAnyPermissionForRole(permissionCodes, roleCodes, ...permissions)
    const nav = NAV.filter((item) => item.canView(permissionCodes, roleCodes, isManagementRole))
    const currentActive = nav.some((item) => item.id === active) ? active : nav[0]?.id
    const canReadAllTickets = can(PERMISSIONS.ticketReadList)
    const canReadOwnTickets = can(PERMISSIONS.ticketReadOwn)
    const canViewOwnContracts = can(PERMISSIONS.contractReadOwn)
    const canViewOwnViolations = can(PERMISSIONS.contractViolationReadOwn)
    const accountInitial = email.charAt(0).toUpperCase()

    useEffect(() => {
        if (!isAccountDetailsOpen) return

        let isCurrent = true
        setProfileLoading(true)
        setProfile(null)
        setRoles([])
        setPermissions([])
        setProfileError(null)
        void Promise.allSettled([
            identityApi.getProfileByAccountId(accountId),
            identityApi.getAllRolesWithPermissions(),
            identityApi.getAllPermissions(),
        ]).then(([profileResult, roleResult, permissionResult]) => {
            if (!isCurrent) return
            if (profileResult.status === "fulfilled") {
                setProfile(profileResult.value)
            } else if (!/profile was not found/i.test(profileResult.reason instanceof Error
                ? profileResult.reason.message
                : "")) {
                setProfileError(profileResult.reason instanceof Error
                    ? profileResult.reason.message
                    : "Không thể tải hồ sơ người dùng.")
            }
            if (roleResult.status === "fulfilled") {
                setRoles(roleResult.value)
            } else {
                setProfileError((current) => current ?? (roleResult.reason instanceof Error
                    ? roleResult.reason.message
                    : "Không thể tải vai trò tài khoản."))
            }
            if (permissionResult.status === "fulfilled") {
                setPermissions(permissionResult.value)
            } else {
                setProfileError((current) => current ?? (permissionResult.reason instanceof Error
                    ? permissionResult.reason.message
                    : "Không thể tải quyền tài khoản."))
            }
        }).finally(() => {
            if (isCurrent) setProfileLoading(false)
        })
        return () => {
            isCurrent = false
        }
    }, [accountId, isAccountDetailsOpen])

    const effectiveRoles = roles.filter((role) =>
        roleCodes.some((code) => code.toUpperCase() === role.roleCode.toUpperCase()))
    const effectivePermissions = permissions.filter((permission) =>
        permissionCodes.some((code) => code.toUpperCase() === permission.permissionCode.toUpperCase()))

    return (
        <div className="flex" style={{fontFamily: "Inter, sans-serif"}}>
            <aside className="flex w-56 shrink-0 flex-col" style={{background: "#0f1629", minHeight: "100vh"}}>
                <div className="border-b px-5 py-5" style={{borderColor: "rgba(255,255,255,0.06)"}}>
                    <div className="flex items-center gap-2.5">
                        <div
                            className="flex h-7 w-7 items-center justify-center rounded-lg bg-blue-500 text-xs font-bold text-white"
                            style={{fontFamily: "DM Sans"}}>M
                        </div>
                        <div>
                            <div className="text-sm font-semibold leading-tight text-white"
                                 style={{fontFamily: "DM Sans"}}>ManagerSpace
                            </div>
                            <div className="text-xs" style={{color: "rgba(255,255,255,0.35)"}}>Quản lý mặt bằng</div>
                        </div>
                    </div>
                </div>

                <nav className="flex-1 overflow-y-auto px-3 py-4">
                    <div className="space-y-0.5">
                        {nav.map((item) => (
                            <button
                                key={item.id}
                                onClick={() => setActive(item.id)}
                                className={`flex w-full items-center gap-3 rounded-lg px-3 py-2 text-sm transition-all ${currentActive === item.id ? "bg-blue-600 text-white" : "hover:bg-white/5"}`}
                                style={{color: currentActive === item.id ? "#fff" : "rgba(255,255,255,0.55)"}}
                            >
                                <span className="material-symbols-outlined w-4 text-center text-[18px] leading-none"
                                      aria-hidden="true">{item.icon}</span>
                                <span style={{fontFamily: "DM Sans"}}>{item.label}</span>
                            </button>
                        ))}
                    </div>
                </nav>

                <div className="border-t px-3 py-4" style={{borderColor: "rgba(255,255,255,0.06)"}}>
                    <div className="flex items-center gap-3 px-2">
                        <button type="button" onClick={() => setIsAccountDetailsOpen(true)}
                                aria-label="Xem tài khoản của tôi"
                                title="Tài khoản của tôi"
                                className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-linear-to-br from-blue-400 to-violet-500 text-xs font-bold text-white ring-offset-2 transition hover:ring-2 hover:ring-blue-400 focus:outline-none focus:ring-2 focus:ring-blue-400">{accountInitial}</button>
                        <div className="min-w-0 flex-1">
                            <div className="truncate text-xs font-medium text-white">{email}</div>
                            <div className="truncate text-xs"
                                 style={{color: "rgba(255,255,255,0.35)"}}>{roleCodes.join(", ") || "Tài khoản"}</div>
                        </div>
                        <button onClick={onLogout}
                                className="rounded-lg px-2 py-1.5 text-xs text-slate-400 transition hover:bg-white/10 hover:text-white"
                                title="Đăng xuất">Thoát
                        </button>
                    </div>
                </div>
            </aside>

            <main className="flex min-w-0 flex-1 flex-col">
                <header
                    className="flex shrink-0 items-center justify-between border-b border-gray-100 bg-white px-8 py-3">
                    <div className="text-xs text-gray-400">{nav.find((item) => item.id === currentActive)?.label}</div>
                    <div className="flex items-center gap-4">
                        <div className="text-xs text-gray-400"
                             style={{fontFamily: "JetBrains Mono, monospace"}}>{new Intl.DateTimeFormat("vi-VN").format(new Date())}</div>
                        <div className="h-2 w-2 rounded-full bg-emerald-400" title="Hệ thống hoạt động bình thường"/>
                    </div>
                </header>
                <div className="flex-1 overflow-y-auto p-8">
                    {currentActive === "dashboard" && <Dashboard/>}
                    {currentActive === "statistics" && <StatisticsModule/>}
                    {currentActive === "premises" &&
                        <PremisesModule roleCodes={roleCodes} permissionCodes={permissionCodes}/>}
                    {currentActive === "contracts" &&
                        <ContractsModule accountId={accountId} readOnly={!can(PERMISSIONS.contractUpdate)}
                                         canCreate={can(PERMISSIONS.contractCreate)}
                                         canUpdate={can(PERMISSIONS.contractUpdate)}
                                         ownOnly={canViewOwnContracts && !can(PERMISSIONS.contractReadList)}/>}
                    {currentActive === "invoices" &&
                        <InvoicesModule roleCodes={roleCodes} permissionCodes={permissionCodes} accountId={accountId}
                                        ownOnly={can(PERMISSIONS.invoiceReadOwn) && !can(PERMISSIONS.invoiceReadList)}/>}
                    {currentActive === "regulations" && <RegulationsModule
                        readOnly={!canAny(PERMISSIONS.contractRegulationCreate, PERMISSIONS.contractRegulationUpdate, PERMISSIONS.contractRegulationRevoke)}/>}
                    {currentActive === "violations" &&
                        <ViolationsModule accountId={accountId} readOnly={!can("CONTRACT_VIOLATION_UPDATE")}
                                          ownOnly={canViewOwnViolations && !can(PERMISSIONS.contractViolationRead)}/>}
                    {currentActive === "tickets" && <TicketsModule accountId={accountId} ownOnly={!canReadAllTickets}
                                                                   canCreateViolation={can(PERMISSIONS.ticketViolationCreate)}
                                                                   canReadOwn={canReadOwnTickets}/>}
                    {currentActive === "notifications" &&
                        <TicketsModule accountId={accountId} customerMode ownOnly={!isManagementRole}
                                       canReadOwn={canReadOwnTickets}/>}
                    {currentActive === "accounts" &&
                        <AccountsModule currentAccountId={accountId} roleCodes={roleCodes} permissionCodes={permissionCodes}/>}
                    {currentActive === "contacts" && <ContactsModule/>}
                </div>
            </main>
            {isAccountDetailsOpen && <Modal title="Tài khoản của tôi" onClose={() => setIsAccountDetailsOpen(false)} wide>
                <div className="grid gap-4 xl:grid-cols-2">
                    <section className="rounded-lg border border-slate-100 p-4">
                        <h2 className="mb-3 text-xs font-semibold uppercase tracking-wide text-slate-500">Thông tin tài khoản</h2>
                        <dl className="space-y-3 text-sm">
                            <div><dt className="text-xs text-slate-500">Account ID</dt>
                                <dd className="mt-1 break-all font-mono text-xs text-slate-800">{accountId}</dd></div>
                            <div><dt className="text-xs text-slate-500">Email</dt>
                                <dd className="mt-1 font-medium text-slate-900">{email}</dd></div>
                        </dl>
                    </section>
                    <section className="rounded-lg border border-slate-100 p-4">
                        <h2 className="mb-3 text-xs font-semibold uppercase tracking-wide text-slate-500">Hồ sơ người dùng</h2>
                        {profileLoading ? <p role="status" className="text-sm text-slate-500">Đang tải hồ sơ...</p>
                            : profileError ? <p role="alert" className="text-sm text-red-700">{profileError}</p>
                                : profile ? <div className="grid gap-4 sm:grid-cols-[7rem_minmax(0,1fr)]">
                                    <img src={profile.userProfileAvatarUrl || "/assets/avatar-placeholder.svg"}
                                         onError={(event) => {
                                             event.currentTarget.onerror = null
                                             event.currentTarget.src = "/assets/avatar-placeholder.svg"
                                         }}
                                         alt="Ảnh đại diện"
                                         className="h-24 w-24 rounded-full border border-slate-200 object-cover"/>
                                    <dl className="grid grid-cols-[auto_1fr] content-start gap-x-4 gap-y-2 text-sm">
                                        <dt className="text-slate-500">Mã hồ sơ</dt>
                                        <dd className="break-all">{profile.userProfileId}</dd>
                                        <dt className="text-slate-500">Họ tên</dt>
                                        <dd>{[profile.userProfileLastName, profile.userProfileFirstName].filter(Boolean).join(" ") || "—"}</dd>
                                        <dt className="text-slate-500">Ngày sinh</dt>
                                        <dd>{profile.userProfileDateOfBirth ? dateLabel(profile.userProfileDateOfBirth) : "—"}</dd>
                                        <dt className="text-slate-500">Giới tính</dt>
                                        <dd>{profile.userProfileGender === "Male" ? "Nam" : profile.userProfileGender === "Female" ? "Nữ" : "Chưa xác định"}</dd>
                                        <dt className="text-slate-500">Điện thoại</dt>
                                        <dd>{profile.userProfilePhoneNumber || "—"}</dd>
                                        <dt className="text-slate-500">Địa chỉ</dt>
                                        <dd className="wrap-break-word">{profile.userProfileAddress || "—"}</dd>
                                    </dl>
                                </div> : <p className="text-sm text-slate-500">Tài khoản chưa có hồ sơ người dùng.</p>}
                    </section>
                    <section className="rounded-lg border border-slate-100 p-4">
                        <h2 className="mb-3 text-xs font-semibold uppercase tracking-wide text-slate-500">Vai trò</h2>
                        <div className="space-y-2">
                            {effectiveRoles.map((role) => <div key={role.roleId}
                                className="flex items-center justify-between gap-3 rounded-md bg-slate-50 p-2 text-sm">
                                <span className="font-medium text-slate-800">{role.roleName}</span>
                                <span className="text-xs text-slate-500">{role.roleCode}</span>
                            </div>)}
                            {effectiveRoles.length === 0 && roleCodes.map((code) => <div key={code}
                                className="rounded-md bg-slate-50 p-2 text-sm text-slate-800">{code}</div>)}
                            {roleCodes.length === 0 && <p className="text-sm text-slate-500">Chưa được gán vai trò.</p>}
                        </div>
                    </section>
                    <section className="rounded-lg border border-slate-100 p-4">
                        <h2 className="mb-3 text-xs font-semibold uppercase tracking-wide text-slate-500">Quyền hiệu lực</h2>
                        <div className="max-h-56 space-y-2 overflow-y-auto">
                            {effectivePermissions.map((permission) => <div key={permission.permissionId}
                                className="rounded-md bg-slate-50 p-2">
                                <p className="text-sm font-medium text-slate-800">{permission.permissionName}</p>
                                <p className="mt-0.5 text-xs text-slate-500">{permission.permissionCode}</p>
                            </div>)}
                            {effectivePermissions.length === 0 && permissionCodes.map((code) => <div key={code}
                                className="rounded-md bg-slate-50 p-2 text-xs text-slate-800">{code}</div>)}
                            {permissionCodes.length === 0 && <p className="text-sm text-slate-500">Tài khoản chưa có quyền nào.</p>}
                        </div>
                    </section>
                </div>
                <div className="mt-4 flex justify-end">
                    <button type="button" onClick={() => setIsAccountDetailsOpen(false)}
                            className="rounded-lg border border-slate-200 px-3 py-2 text-sm font-medium text-slate-700 hover:bg-slate-50">Đóng</button>
                </div>
            </Modal>}
        </div>
    )
}

import type {SubmitEvent} from "react"
import {useCallback, useEffect, useState} from "react"
import {identityApi} from "@/api/identity.ts"
import {mediaApi} from "@/api/media.ts"
import type {AccountModel, PermissionModel, RoleModel, UserProfileModel} from "@/types/identity"
import {BaseTable, FormField, inputCls, Modal, ProcessingForm, selectCls} from "./shared"
import {accountName, dateLabel} from "@/scripts/identity.ts"
import {APP_SETTINGS} from "@/config/appSettings"

const buttonCls = "rounded-lg px-3 py-2 text-sm font-medium transition-colors disabled:cursor-not-allowed disabled:opacity-50"
const fallbackAvatarUrl = "https://lh3.googleusercontent.com/d/11TJu1RDc0clWSfvF5LpkkLlnO-flF7ey?authuser=0"

function avatarDisplayUrl(url: string | null | undefined): string {
    if (!url) return fallbackAvatarUrl
    const driveFileId = url.match(/\/d\/([^/?#]+)/)?.[1]
    return driveFileId ? `/api/media/${encodeURIComponent(driveFileId)}/content` : url
}

function AvatarUploader({file, currentUrl, disabled, onChange}: {file: File | null; currentUrl: string; disabled: boolean; onChange: (file: File | null) => void}) {
    const [previewUrl, setPreviewUrl] = useState("")
    useEffect(() => {
        if (!file) { setPreviewUrl(""); return }
        const url = URL.createObjectURL(file)
        setPreviewUrl(url)
        return () => URL.revokeObjectURL(url)
    }, [file])
    return <div className="mb-5 flex flex-col items-center gap-3">
        <img src={previewUrl || avatarDisplayUrl(currentUrl)} onError={(event) => { event.currentTarget.onerror = null; event.currentTarget.src = avatarDisplayUrl(fallbackAvatarUrl) }} alt="Ảnh đại diện"
             className="h-28 w-28 rounded-full border border-slate-200 object-cover shadow-sm" />
        <label className={`${buttonCls} cursor-pointer border border-blue-200 bg-blue-50 text-blue-700 hover:bg-blue-100`}>
            Chọn ảnh đại diện
            <input type="file" accept="image/*" className="sr-only" disabled={disabled} onChange={(event) => onChange(event.target.files?.[0] ?? null)} />
        </label>
        {file && <button type="button" disabled={disabled} className="text-xs text-slate-500 underline disabled:opacity-50" onClick={() => onChange(null)}>Bỏ ảnh mới chọn</button>}
    </div>
}
type ProfileDraft = {
    identityCode: string
    firstName: string
    lastName: string
    dateOfBirth: string
    gender: NonNullable<UserProfileModel["userProfileGender"]>
    phoneNumber: string
    address: string
    avatarUrl: string
}

const emptyProfileDraft: ProfileDraft = {
    identityCode: "",
    firstName: "",
    lastName: "",
    dateOfBirth: "",
    gender: "Unspecified",
    phoneNumber: "",
    address: "",
    avatarUrl: "",
}

function toProfileDraft(profile: UserProfileModel | null): ProfileDraft {
    return profile ? {
        identityCode: profile.userProfileId,
        firstName: profile.userProfileFirstName ?? "",
        lastName: profile.userProfileLastName ?? "",
        dateOfBirth: profile.userProfileDateOfBirth?.slice(0, 10) ?? "",
        gender: profile.userProfileGender ?? "Unspecified",
        phoneNumber: profile.userProfilePhoneNumber ?? "",
        address: profile.userProfileAddress ?? "",
        avatarUrl: profile.userProfileAvatarUrl ?? "",
    } : emptyProfileDraft
}

export function AccountsModule() {
    const [accounts, setAccounts] = useState<AccountModel[]>([])
    const [roles, setRoles] = useState<RoleModel[]>([])
    const [permissions, setPermissions] = useState<PermissionModel[]>([])
    const [status, setStatus] = useState<"all" | "active" | "inactive">("active")
    const [search, setSearch] = useState("")
    const [roleFilter, setRoleFilter] = useState("all")
    const [selectedAccount, setSelectedAccount] = useState<AccountModel | null>(null)
    const [profileDraft, setProfileDraft] = useState<ProfileDraft>(emptyProfileDraft)
    const [isEditingProfile, setIsEditingProfile] = useState(false)
    const [profileSaving, setProfileSaving] = useState(false)
    const [profileError, setProfileError] = useState<string | null>(null)
    const [assignedRoleIds, setAssignedRoleIds] = useState<string[]>([])
    const [assignedPermissionIds, setAssignedPermissionIds] = useState<string[]>([])
    const [isEditingAccountRoles, setIsEditingAccountRoles] = useState(false)
    const [isEditingAccountPermissions, setIsEditingAccountPermissions] = useState(false)
    const [avatarFile, setAvatarFile] = useState<File | null>(null)
    const [selectedRoleId, setSelectedRoleId] = useState("")
    const [permissionIds, setPermissionIds] = useState<string[]>([])
    const [showCreate, setShowCreate] = useState(false)
    const [createForm, setCreateForm] = useState({email: "", password: "", roleId: ""})
    const [loading, setLoading] = useState(true)
    const [saving, setSaving] = useState(false)
    const [error, setError] = useState<string | null>(null)
    const [notice, setNotice] = useState<string | null>(null)

    const loadPage = useCallback(async () => {
        setLoading(true)
        setError(null)
        try {
            const [roleList, permissionList] = await Promise.all([
                identityApi.getAllRolesWithPermissions(),
                identityApi.getAllPermissions(),
            ])
            setRoles(roleList)
            setPermissions(permissionList)

            const accountItems: AccountModel[] = []
            let cursor: string | null = null
            do {
                const page = await identityApi.getAccountsPageByStatus(
                    cursor,
                    APP_SETTINGS.pagination.maxPageLoaded * APP_SETTINGS.pagination.defaultPageSize,
                    status,
                )
                accountItems.push(...page.items)
                cursor = page.isHasMore ? page.nextCursor : null
            } while (cursor)

            setAccounts(accountItems.map((account) => ({
                ...account,
                roles: roleList.filter((role) => account.roleCodes.includes(role.roleCode)),
                additionalPermissions: permissionList.filter((permission) => account.permissionCodes.includes(permission.permissionCode)),
            })))
            setSelectedRoleId((current) => current || roleList[0]?.roleId || "")
            setCreateForm((current) => ({...current, roleId: current.roleId || roleList[0]?.roleId || ""}))
        } catch (cause) {
            const message = cause instanceof Error ? cause.message : "Không tải được dữ liệu Identity."
            if (message === "Không tìm thấy dữ liệu.") {
                setAccounts([])
                try {
                    const [roleList, permissionList] = await Promise.all([identityApi.getAllRolesWithPermissions(), identityApi.getAllPermissions()])
                    setRoles(roleList)
                    setPermissions(permissionList)
                } catch (roleCause) {
                    setError(roleCause instanceof Error ? roleCause.message : message)
                }
            } else {
                setError(message)
            }
        } finally {
            setLoading(false)
        }
    }, [status])

    useEffect(() => {
        void loadPage()
    }, [loadPage])

    const selectedRole = roles.find((role) => role.roleId === selectedRoleId) ?? null
    useEffect(() => {
        if (!selectedRole) {
            setPermissionIds([])
            return
        }
        let cancelled = false
        setPermissionIds([])
        void identityApi.getRoleWithPermissionsByCode(selectedRole.roleCode).then((role) => {
            if (cancelled) return
            setPermissionIds(role.permissions.map((permission) => permission.permissionId))
            setRoles((current) => current.map((item) => item.roleId === role.roleId ? role : item))
        }).catch((cause) => {
            if (!cancelled) setError(cause instanceof Error ? cause.message : "Không tải được quyền của vai trò.")
        })
        return () => {
            cancelled = true
        }
    }, [selectedRoleId])

    const visibleAccounts = accounts.filter((account) => {
        const matchesSearch = !search || `${accountName(account)} ${account.accountEmail}`.toLocaleLowerCase("vi").includes(search.toLocaleLowerCase("vi"))
        const matchesRole = roleFilter === "all" || account.roles.some((role) => role.roleId === roleFilter)
        return matchesSearch && matchesRole
    })

    const runAction = async (action: () => Promise<unknown>, successMessage: string) => {
        setSaving(true)
        setError(null)
        setNotice(null)
        try {
            await action()
            setNotice(successMessage)
            await loadPage()
            return true
        } catch (cause) {
            setError(cause instanceof Error ? cause.message : "Thao tác Identity thất bại.")
            return false
        } finally {
            setSaving(false)
        }
    }

    const saveAccountRoles = async () => {
        if (!selectedAccount) return
        const additions = assignedRoleIds.filter((id) => !selectedAccount.roles.some((role) => role.roleId === id))
        if (!additions.length) { setIsEditingAccountRoles(false); return }
        const succeeded = await runAction(() => identityApi.addRolesToAccount(selectedAccount.accountId, additions), "Đã thêm vai trò cho tài khoản.")
        if (succeeded) {
            const nextRoles = roles.filter((role) => assignedRoleIds.includes(role.roleId))
            setSelectedAccount((current) => current ? {...current, roles: nextRoles} : current)
            setAccounts((current) => current.map((account) => account.accountId === selectedAccount.accountId ? {...account, roles: nextRoles} : account))
            setIsEditingAccountRoles(false)
        }
    }

    const saveAccountPermissions = async () => {
        if (!selectedAccount) return
        const additions = assignedPermissionIds.filter((id) => !selectedAccount.additionalPermissions.some((permission) => permission.permissionId === id))
        if (!additions.length) { setIsEditingAccountPermissions(false); return }
        const succeeded = await runAction(() => identityApi.addPermissionsToAccount(selectedAccount.accountId, additions), "Đã thêm quyền bổ sung cho tài khoản.")
        if (succeeded) {
            const nextPermissions = permissions.filter((permission) => assignedPermissionIds.includes(permission.permissionId))
            setSelectedAccount((current) => current ? {...current, additionalPermissions: nextPermissions} : current)
            setAccounts((current) => current.map((account) => account.accountId === selectedAccount.accountId ? {...account, additionalPermissions: nextPermissions} : account))
            setIsEditingAccountPermissions(false)
        }
    }

    const saveRolePermissions = () => {
        if (!selectedRole) return
        void runAction(
            () => identityApi.updateRolePermissions(selectedRole, permissionIds),
            `Đã cập nhật quyền của vai trò ${selectedRole.roleName}.`,
        )
    }

    const createAccount = async (event: SubmitEvent<HTMLFormElement>) => {
        event.preventDefault()
        const role = roles.find((item) => item.roleId === createForm.roleId)
        if (!role) return
        setSaving(true)
        setError(null)
        setNotice(null)
        try {
            const account = await identityApi.registerAccount(createForm.email, createForm.password)
            try {
                await identityApi.assignRolesToAccount(account.accountId, [role.roleId])
            } catch (cause) {
                throw new Error(`Tài khoản đã tạo với role mặc định nhưng chưa gán được ${role.roleName}: ${cause instanceof Error ? cause.message : "lỗi không xác định"}`)
            }
            setShowCreate(false)
            setCreateForm({email: "", password: "", roleId: role.roleId})
            setNotice("Đã tạo tài khoản và gán vai trò.")
            setStatus("all")
            setSearch("")
            setRoleFilter("all")
            if (status === "all") await loadPage()
        } catch (cause) {
            setError(cause instanceof Error ? cause.message : "Không tạo được tài khoản.")
        } finally {
            setSaving(false)
        }
    }

    const openAccount = (account: AccountModel) => {
        setSelectedAccount(account)
        setAssignedRoleIds(account.roles.map((role) => role.roleId))
        setAssignedPermissionIds(account.additionalPermissions.map((permission) => permission.permissionId))
        setIsEditingAccountRoles(false)
        setIsEditingAccountPermissions(false)
        setAvatarFile(null)
        setProfileDraft(toProfileDraft(account.userProfile))
        setIsEditingProfile(false)
        setProfileError(null)
        setError(null)
        setNotice(null)
        void identityApi.getProfileByAccountId(account.accountId).then((profile) => {
            setSelectedAccount((current) => current?.accountId === account.accountId
                ? {...current, userProfile: profile}
                : current)
            setAccounts((current) => current.map((item) => item.accountId === account.accountId
                ? {...item, userProfile: profile}
                : item))
            setProfileDraft(toProfileDraft(profile))
        }).catch(() => {
            // Tài khoản có thể chưa có hồ sơ.
        })
    }

    const saveAccountProfile = (profile: UserProfileModel) => {
        setSelectedAccount((current) => current?.accountId === profile.userProfileAccountId
            ? {...current, userProfile: profile}
            : current)
        setAccounts((current) => current.map((item) => item.accountId === profile.userProfileAccountId
            ? {...item, userProfile: profile}
            : item))
        setProfileDraft(toProfileDraft(profile))
        setAvatarFile(null)
        setIsEditingProfile(false)
    }

    const createAccountProfile = async (event: SubmitEvent<HTMLFormElement>) => {
        event.preventDefault()
        if (!selectedAccount || !/^\d{12}$/.test(profileDraft.identityCode)) return
        setProfileSaving(true)
        setProfileError(null)
        try {
            const profile = await identityApi.createProfile(selectedAccount.accountId, profileDraft.identityCode)
            saveAccountProfile(profile)
            setNotice("Đã tạo hồ sơ người dùng.")
        } catch (cause) {
            setProfileError(cause instanceof Error ? cause.message : "Không tạo được hồ sơ người dùng.")
        } finally {
            setProfileSaving(false)
        }
    }

    const updateAccountProfile = async (event: SubmitEvent<HTMLFormElement>) => {
        event.preventDefault()
        if (!selectedAccount?.userProfile) return
        setProfileSaving(true)
        setProfileError(null)
        try {
            const avatarUrl = avatarFile ? await mediaApi.uploadImage(avatarFile, "avatar") : profileDraft.avatarUrl.trim() || null
            const profile = await identityApi.updateProfile({
                ...selectedAccount.userProfile,
                userProfileFirstName: profileDraft.firstName.trim() || null,
                userProfileLastName: profileDraft.lastName.trim() || null,
                userProfileDateOfBirth: profileDraft.dateOfBirth
                    ? new Date(`${profileDraft.dateOfBirth}T00:00:00`).toISOString()
                    : null,
                userProfileGender: profileDraft.gender,
                userProfilePhoneNumber: profileDraft.phoneNumber.trim() || null,
                userProfileAddress: profileDraft.address.trim() || null,
                userProfileAvatarUrl: avatarUrl,
            })
            saveAccountProfile(profile)
            setAvatarFile(null)
            setNotice("Đã cập nhật hồ sơ người dùng.")
        } catch (cause) {
            setProfileError(cause instanceof Error ? cause.message : "Không cập nhật được hồ sơ người dùng.")
        } finally {
            setProfileSaving(false)
        }
    }

    return (
        <div className="-m-8 flex min-h-[calc(100vh-49px)] flex-col bg-[#f8fafc]">
            <header className="flex min-h-18 items-center justify-between border-b border-[#e2e8f0] bg-white px-8">
                <div>
                    <h1 className="text-lg font-bold text-[#0f172a]">Quản lý Định danh &amp; Tài khoản</h1>
                    <p className="mt-1 text-xs text-[#64748b]">Dữ liệu trực tiếp từ Identity API · tài khoản, vai trò và
                        quyền</p>
                </div>
                <div className="flex items-center gap-3">
                    <input className={`${inputCls} w-64`} placeholder="Tìm tên hoặc email" value={search}
                           onChange={(event) => setSearch(event.target.value)}/>
                    <button className={`${buttonCls} bg-[#0f52ff] text-white hover:bg-[#0a42dd]`}
                            onClick={() => setShowCreate(true)}>Tạo tài khoản
                    </button>
                </div>
            </header>

            <main className="flex flex-1 flex-col gap-5 p-8">
                {error && <div role="alert"
                               className="flex items-start justify-between gap-4 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
                    <span>{error}</span>
                    <button className="font-semibold" onClick={() => setError(null)}>Đóng</button>
                </div>}
                {notice && <div role="status"
                                className="rounded-lg border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700">{notice}</div>}

                <section className="flex flex-wrap items-center justify-between gap-3">
                    <div className="flex items-center gap-3">
                        <label className="text-sm text-slate-600" htmlFor="account-status">Trạng thái</label>
                        <select id="account-status" className={selectCls} value={status}
                                onChange={(event) => setStatus(event.target.value as typeof status)}>
                            <option value="active">Đang hoạt động</option>
                            <option value="inactive">Đã vô hiệu hóa</option>
                            <option value="all">Tất cả</option>
                        </select>
                        <label className="text-sm text-slate-600" htmlFor="account-role">Vai trò</label>
                        <select id="account-role" className={selectCls} value={roleFilter}
                                onChange={(event) => setRoleFilter(event.target.value)}>
                            <option value="all">Tất cả vai trò</option>
                            {roles.map((role) => <option key={role.roleId}
                                                         value={role.roleId}>{role.roleName}</option>)}
                        </select>
                    </div>
                    <button className={`${buttonCls} border border-slate-200 bg-white text-slate-700 hover:bg-slate-50`}
                            onClick={() => void loadPage()} disabled={loading}>Làm mới
                    </button>
                </section>

                <div className="grid items-start gap-5 xl:grid-cols-[minmax(0,1fr)_380px]">
                    <section className="overflow-hidden rounded-2xl border border-[#e2e8f0] bg-white shadow-sm">
                        <div className="flex items-center justify-between border-b border-slate-100 px-5 py-4">
                            <div><h2 className="font-semibold text-slate-900">Danh sách tài khoản</h2><p
                                className="mt-1 text-xs text-slate-500">{visibleAccounts.length} tài khoản phù hợp</p></div>
                            <span className="rounded-md bg-slate-100 px-2.5 py-1 text-xs text-slate-600">Identity AccountModel</span>
                        </div>
                        <BaseTable
                            rows={visibleAccounts}
                            getRowKey={(account) => account.accountId}
                            loading={loading}
                            emptyMessage="Không có tài khoản phù hợp."
                            tableClassName="w-full min-w-190 text-left text-sm"
                            paginationResetKey={`${status}:${search}:${roleFilter}`}
                            onRowClick={openAccount}
                            rowClassName={() => "hover:bg-blue-50/50"}
                            columns={[
                                {key: "account", header: "Tài khoản", headerClassName: "px-5 py-3 text-left font-semibold", cellClassName: "px-5 py-3.5", render: (account) => <><span className="block font-medium text-slate-900">{accountName(account)}</span><span className="mt-0.5 block text-xs text-slate-500">{account.accountEmail}</span></>},
                                {key: "roles", header: "Vai trò", headerClassName: "px-5 py-3 text-left font-semibold", cellClassName: "px-5 py-3.5", render: (account) => <div className="flex flex-wrap gap-1.5">{account.roles.length ? account.roles.map((role) => <span key={role.roleId} className="rounded-md bg-blue-50 px-2 py-1 text-xs font-medium text-blue-700">{role.roleName}</span>) : <span className="text-xs text-slate-400">Chưa gán role</span>}</div>},
                                {key: "created", header: "Ngày tạo", headerClassName: "px-5 py-3 text-left font-semibold", cellClassName: "px-5 py-3.5 text-slate-600", render: (account) => dateLabel(account.accountCreatedAt)},
                                {key: "status", header: "Trạng thái", headerClassName: "px-5 py-3 text-left font-semibold", cellClassName: "px-5 py-3.5", render: (account) => <span className={`rounded-md px-2 py-1 text-xs font-semibold ${account.accountIsActive ? "bg-emerald-50 text-emerald-700" : "bg-red-50 text-red-700"}`}>{account.accountIsActive ? "Hoạt động" : "Vô hiệu hóa"}</span>},
                            ]}
                        />
                    </section>

                    <section className="rounded-2xl border border-[#e2e8f0] bg-white p-5 shadow-sm">
                        <div className="mb-4"><h2 className="font-semibold text-slate-900">Quyền của vai trò</h2><p
                            className="mt-1 text-xs text-slate-500">Cập nhật qua AuthorizationController</p></div>
                        <FormField label="Vai trò">
                            <select className={selectCls} value={selectedRoleId} disabled={saving}
                                    onChange={(event) => setSelectedRoleId(event.target.value)}>
                                {roles.map((role) => <option key={role.roleId}
                                                             value={role.roleId}>{role.roleName} ({role.roleCode})</option>)}
                            </select>
                        </FormField>
                        {selectedRole ? <>
                            <div className="mb-3 flex items-center justify-between"><p
                                className="text-xs font-semibold uppercase tracking-wide text-slate-500">Permission</p>
                                <span className="text-xs text-slate-500">{permissionIds.length} đã chọn</span></div>
                            <div
                                className="max-h-97.5 space-y-1 overflow-y-auto rounded-lg border border-slate-100 p-2">
                                {permissions.map((permission) => <label key={permission.permissionId}
                                className="flex cursor-pointer items-start gap-2 rounded-md p-2 hover:bg-slate-50"><input
                                    type="checkbox" className="mt-0.5 accent-blue-600" disabled={saving}
                                    checked={permissionIds.includes(permission.permissionId)}
                                    onChange={(event) => setPermissionIds((ids) => event.target.checked ? [...ids, permission.permissionId] : ids.filter((id) => id !== permission.permissionId))}/><span><span
                                    className="block text-xs font-medium text-slate-800">{permission.permissionName}</span><span
                                    className="text-[11px] text-slate-500">{permission.permissionCode}</span></span></label>)}
                                {permissions.length === 0 &&
                                    <p className="p-3 text-xs text-slate-500">Backend chưa trả về permission.</p>}
                            </div>
                            <button className={`${buttonCls} mt-4 w-full bg-[#0f52ff] text-white hover:bg-[#0a42dd]`}
                                    disabled={saving || permissionIds.length === 0} onClick={saveRolePermissions}>Lưu
                                quyền vai trò
                            </button>
                        </> : <p className="text-sm text-slate-500">Chưa có vai trò từ Backend.</p>}
                    </section>
                </div>
            </main>

            {selectedAccount &&
                <Modal title="Chi tiết tài khoản" onClose={() => setSelectedAccount(null)} wide>
                    <div className="grid gap-5 xl:grid-cols-[minmax(0,1fr)_minmax(0,1fr)]">
                        <div className="space-y-3">
                            <div className="rounded-lg border border-slate-100 p-3">
                                <div className="mb-2 flex items-center justify-between"><p className="text-xs font-semibold uppercase tracking-wide text-slate-500">Vai trò tài khoản</p>
                                    {!isEditingAccountRoles && <button type="button" className={`${buttonCls} border border-blue-200 text-blue-700 hover:bg-blue-50`} onClick={() => { setAssignedRoleIds(selectedAccount.roles.map((role) => role.roleId)); setIsEditingAccountRoles(true) }}>Chỉnh sửa</button>}
                                </div>
                                <p className="mb-2 text-xs text-slate-500">Chọn thêm vai trò cho tài khoản.</p>
                                <div className="max-h-64 space-y-1 overflow-y-auto rounded-lg border border-slate-100 p-2">
                                    {roles.map((role) => <label key={role.roleId}
                                        className={`flex items-center gap-2 rounded-md p-2 ${isEditingAccountRoles && !selectedAccount.roles.some((item) => item.roleId === role.roleId) ? "cursor-pointer hover:bg-slate-50" : "cursor-default opacity-70"}`}><input
                                        type="checkbox" className="accent-blue-600" disabled={saving || !isEditingAccountRoles || selectedAccount.roles.some((item) => item.roleId === role.roleId)}
                                        checked={assignedRoleIds.includes(role.roleId)}
                                        onChange={(event) => setAssignedRoleIds((ids) => event.target.checked ? [...ids, role.roleId] : ids.filter((id) => id !== role.roleId))}/><span
                                        className="text-sm text-slate-800">{role.roleName}</span><span
                                        className="ml-auto text-xs text-slate-400">{role.roleCode}</span></label>)}
                                </div>
                                {isEditingAccountRoles && <div className="mt-3 flex gap-2"><button className={`${buttonCls} flex-1 bg-blue-600 text-white hover:bg-blue-700`} disabled={saving} onClick={() => void saveAccountRoles()}>Lưu vai trò</button><button className={`${buttonCls} border border-slate-200 text-slate-700`} disabled={saving} onClick={() => { setAssignedRoleIds(selectedAccount.roles.map((role) => role.roleId)); setIsEditingAccountRoles(false) }}>Hủy</button></div>}
                            </div>
                            <div className="rounded-lg border border-slate-100 p-3">
                                <div className="mb-2 flex items-center justify-between"><p className="text-xs font-semibold uppercase tracking-wide text-slate-500">Quyền bổ sung</p>
                                    {!isEditingAccountPermissions && <button type="button" className={`${buttonCls} border border-blue-200 text-blue-700 hover:bg-blue-50`} onClick={() => { setAssignedPermissionIds(selectedAccount.additionalPermissions.map((permission) => permission.permissionId)); setIsEditingAccountPermissions(true) }}>Chỉnh sửa</button>}
                                </div>
                                <p className="mb-2 text-xs text-slate-500">Gán permission trực tiếp cho tài khoản, ngoài quyền từ vai trò.</p>
                                <div className="max-h-64 space-y-1 overflow-y-auto rounded-lg border border-slate-100 p-2">
                                    {permissions.map((permission) => <label key={permission.permissionId}
                                        className={`flex items-start gap-2 rounded-md p-2 ${isEditingAccountPermissions && !selectedAccount.additionalPermissions.some((item) => item.permissionId === permission.permissionId) ? "cursor-pointer hover:bg-slate-50" : "cursor-default opacity-70"}`}><input
                                        type="checkbox" className="mt-0.5 accent-blue-600" disabled={saving || !isEditingAccountPermissions || selectedAccount.additionalPermissions.some((item) => item.permissionId === permission.permissionId)}
                                        checked={assignedPermissionIds.includes(permission.permissionId)}
                                        onChange={(event) => setAssignedPermissionIds((ids) => event.target.checked ? [...ids, permission.permissionId] : ids.filter((id) => id !== permission.permissionId))}/><span><span className="block text-xs font-medium text-slate-800">{permission.permissionName}</span><span className="text-[11px] text-slate-500">{permission.permissionCode}</span></span></label>)}
                                    {!permissions.length && <p className="p-2 text-xs text-slate-500">Chưa có permission.</p>}
                                </div>
                                {isEditingAccountPermissions && <div className="mt-3 flex gap-2"><button className={`${buttonCls} flex-1 bg-blue-600 text-white hover:bg-blue-700`} disabled={saving} onClick={() => void saveAccountPermissions()}>Lưu quyền bổ sung</button><button className={`${buttonCls} border border-slate-200 text-slate-700`} disabled={saving} onClick={() => { setAssignedPermissionIds(selectedAccount.additionalPermissions.map((permission) => permission.permissionId)); setIsEditingAccountPermissions(false) }}>Hủy</button></div>}
                            </div>
                        </div>
                        <div className="space-y-3">
                        <section className="rounded-lg border border-slate-100 p-4">
                            <h2 className="mb-3 text-center text-xs font-semibold uppercase tracking-wide text-slate-500">Hồ sơ người dùng</h2>
                            {selectedAccount.userProfile && isEditingProfile
                                ? <AvatarUploader file={avatarFile} currentUrl={profileDraft.avatarUrl} disabled={profileSaving} onChange={setAvatarFile}/>
                                : <div className="mb-5 flex justify-center"><img key={avatarDisplayUrl(selectedAccount.userProfile?.userProfileAvatarUrl)} src={avatarDisplayUrl(selectedAccount.userProfile?.userProfileAvatarUrl)} onError={(event) => { event.currentTarget.onerror = null; event.currentTarget.src = avatarDisplayUrl(fallbackAvatarUrl) }} alt="Ảnh đại diện" className="h-28 w-28 rounded-full border border-slate-200 object-cover shadow-sm" /></div>}
                            {profileError && <p role="alert" className="mb-3 rounded-md bg-red-50 p-2 text-xs text-red-700">{profileError}</p>}
                            {!selectedAccount.userProfile ? <ProcessingForm processing={profileSaving} onSubmit={createAccountProfile}>
                                <p className="mb-3 text-sm text-slate-600">Tài khoản chưa có hồ sơ. Nhập mã định danh 12 chữ số để tạo hồ sơ.</p>
                                <FormField label="Mã định danh"><input className={inputCls} required inputMode="numeric"
                                    pattern="[0-9]{12}" maxLength={12} minLength={12} value={profileDraft.identityCode}
                                    onChange={(event) => setProfileDraft((current) => ({...current, identityCode: event.target.value.replace(/\D/g, "").slice(0, 12)}))}/></FormField>
                                <button type="submit" className={`${buttonCls} bg-blue-600 text-white hover:bg-blue-700`}
                                    disabled={profileSaving || !/^\d{12}$/.test(profileDraft.identityCode)}>
                                    {profileSaving ? "Đang tạo..." : "Tạo hồ sơ"}
                                </button>
                            </ProcessingForm> : isEditingProfile ? <ProcessingForm processing={profileSaving} onSubmit={updateAccountProfile}>
                                <div className="grid gap-x-3 sm:grid-cols-2">
                                    <FormField label="Mã định danh"><input className={inputCls} value={profileDraft.identityCode} readOnly/></FormField>
                                    <FormField label="Giới tính"><select className={selectCls} value={profileDraft.gender}
                                        onChange={(event) => setProfileDraft((current) => ({...current, gender: event.target.value as ProfileDraft["gender"]}))}>
                                        <option value="Unspecified">Chưa xác định</option><option value="Male">Nam</option><option value="Female">Nữ</option>
                                    </select></FormField>
                                    <FormField label="Họ"><input className={inputCls} maxLength={30} value={profileDraft.lastName}
                                        onChange={(event) => setProfileDraft((current) => ({...current, lastName: event.target.value}))}/></FormField>
                                    <FormField label="Tên"><input className={inputCls} maxLength={30} value={profileDraft.firstName}
                                        onChange={(event) => setProfileDraft((current) => ({...current, firstName: event.target.value}))}/></FormField>
                                    <FormField label="Ngày sinh"><input className={inputCls} type="date" value={profileDraft.dateOfBirth}
                                        onChange={(event) => setProfileDraft((current) => ({...current, dateOfBirth: event.target.value}))}/></FormField>
                                    <FormField label="Điện thoại"><input className={inputCls} maxLength={10} value={profileDraft.phoneNumber}
                                        onChange={(event) => setProfileDraft((current) => ({...current, phoneNumber: event.target.value}))}/></FormField>
                                    <FormField label="Địa chỉ"><input className={inputCls} maxLength={255} value={profileDraft.address}
                                        onChange={(event) => setProfileDraft((current) => ({...current, address: event.target.value}))}/></FormField>
                                </div>
                                <div className="flex justify-end gap-2">
                                    <button type="button" className={`${buttonCls} border border-slate-200 text-slate-700`}
                                        disabled={profileSaving} onClick={() => { setProfileDraft(toProfileDraft(selectedAccount.userProfile)); setIsEditingProfile(false) }}>Hủy</button>
                                    <button type="submit" className={`${buttonCls} bg-blue-600 text-white hover:bg-blue-700`}
                                        disabled={profileSaving}>{profileSaving ? "Đang lưu..." : "Lưu hồ sơ"}</button>
                                </div>
                            </ProcessingForm> : <>
                                <dl className="grid grid-cols-[auto_1fr] gap-x-4 gap-y-3 text-sm">
                                    <dt className="text-slate-500">Mã định danh</dt><dd className="break-all">{selectedAccount.userProfile.userProfileId}</dd>
                                    <dt className="text-slate-500">Họ và tên</dt><dd>{accountName(selectedAccount) || "—"}</dd>
                                    <dt className="text-slate-500">Ngày sinh</dt><dd>{selectedAccount.userProfile.userProfileDateOfBirth ? dateLabel(selectedAccount.userProfile.userProfileDateOfBirth) : "—"}</dd>
                                    <dt className="text-slate-500">Giới tính</dt><dd>{selectedAccount.userProfile.userProfileGender === "Male" ? "Nam" : selectedAccount.userProfile.userProfileGender === "Female" ? "Nữ" : "Chưa xác định"}</dd>
                                    <dt className="text-slate-500">Điện thoại</dt><dd>{selectedAccount.userProfile.userProfilePhoneNumber || "—"}</dd>
                                    <dt className="text-slate-500">Địa chỉ</dt><dd>{selectedAccount.userProfile.userProfileAddress || "—"}</dd>
                                </dl>
                            </>}
                            {selectedAccount.userProfile && !isEditingProfile && <button type="button"
                                className={`${buttonCls} mt-5 w-full bg-blue-600 text-white hover:bg-blue-700`}
                                onClick={() => { setProfileDraft(toProfileDraft(selectedAccount.userProfile)); setAvatarFile(null); setIsEditingProfile(true) }}>
                                Chỉnh sửa hồ sơ
                            </button>}
                        </section>
                        <div className="space-y-3">
                            <div className="rounded-lg bg-slate-50 p-3"><p
                                className="text-xs text-slate-500">AccountId</p><p
                                className="mt-1 break-all font-mono text-xs text-slate-800">{selectedAccount.accountId}</p>
                            </div>
                            <div className="rounded-lg bg-slate-50 p-3"><p className="text-xs text-slate-500">Email</p>
                                <p className="mt-1 text-sm font-medium text-slate-900">{selectedAccount.accountEmail}</p>
                            </div>
                            <div className="grid grid-cols-2 gap-3">
                                <div className="rounded-lg bg-slate-50 p-3"><p className="text-xs text-slate-500">Trạng
                                    thái</p><p
                                    className="mt-1 text-sm font-medium">{selectedAccount.accountIsActive ? "Hoạt động" : "Vô hiệu hóa"}</p>
                                </div>
                                <div className="rounded-lg bg-slate-50 p-3"><p className="text-xs text-slate-500">Ngày
                                    tạo</p><p
                                    className="mt-1 text-sm font-medium">{dateLabel(selectedAccount.accountCreatedAt)}</p>
                                </div>
                            </div>
                            <div className="mt-5 flex gap-2 border-t border-slate-100 pt-4">
                                {selectedAccount.accountIsActive ? <button
                                    className={`${buttonCls} border border-red-200 text-red-700 hover:bg-red-50`}
                                    disabled={saving}
                                    onClick={() => void runAction(() => identityApi.deactivateAccountById(selectedAccount.accountId), "Đã vô hiệu hóa tài khoản.").then((succeeded) => {
                                        if (succeeded) setSelectedAccount(null)
                                    })}>Vô hiệu hóa</button> : <button
                                    className={`${buttonCls} border border-emerald-200 text-emerald-700 hover:bg-emerald-50`}
                                    disabled={saving}
                                    onClick={() => void runAction(() => identityApi.activateAccountByEmail(selectedAccount.accountEmail), "Đã kích hoạt tài khoản.").then((succeeded) => {
                                        if (succeeded) setSelectedAccount(null)
                                    })}>Kích hoạt</button>}
                                <button
                                    className={`${buttonCls} ml-auto border border-slate-200 text-slate-700 hover:bg-slate-50`}
                                    onClick={() => setSelectedAccount(null)}>Đóng
                                </button>
                            </div>
                        </div>
                        </div>
                    </div>
                </Modal>}

            {showCreate && <Modal title="Tạo tài khoản Identity" onClose={() => setShowCreate(false)}>
                <ProcessingForm processing={saving} onSubmit={createAccount}>
                    <FormField label="Email"><input className={inputCls} type="email" required maxLength={255}
                                                    value={createForm.email} onChange={(event) => setCreateForm({
                        ...createForm,
                        email: event.target.value
                    })}/></FormField>
                    <FormField label="Mật khẩu"><input className={inputCls} type="password" required
                                                       value={createForm.password} onChange={(event) => setCreateForm({
                        ...createForm,
                        password: event.target.value
                    })}/><span className="mt-1 block text-xs text-slate-500">Theo AccountRules cấu hình tại Identity Backend.</span></FormField>
                    <FormField label="Vai trò"><select className={selectCls} required value={createForm.roleId}
                                                       onChange={(event) => setCreateForm({
                                                           ...createForm,
                                                           roleId: event.target.value
                                                       })}>{roles.map((role) => <option key={role.roleId}
                                                                                        value={role.roleId}>{role.roleName}</option>)}</select></FormField>
                    <p className="mb-4 rounded-lg bg-amber-50 p-3 text-xs text-amber-800">Backend tạo tài khoản bằng
                        endpoint register và khởi tạo hồ sơ riêng. Họ tên/mã định danh sẽ bổ sung qua API hồ sơ khi
                        luồng đó được bật.</p>
                    <div className="flex justify-end gap-2">
                        <button type="button" className={`${buttonCls} border border-slate-200 text-slate-700`}
                                onClick={() => setShowCreate(false)}>Hủy
                        </button>
                        <button type="submit" className={`${buttonCls} bg-blue-600 text-white`}
                                disabled={saving || !roles.length}>{saving ? "Đang tạo..." : "Tạo và gán vai trò"}</button>
                    </div>
                </ProcessingForm>
            </Modal>}
        </div>
    )
}

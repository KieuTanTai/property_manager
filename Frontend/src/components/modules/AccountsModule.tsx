import { useCallback, useEffect, useState } from "react"
import type { SubmitEvent } from "react"
import { identityApi } from "@/api/identity.ts"
import type { AccountModel, PermissionModel, RoleModel } from "@/types/identity"
import { FormField, Modal, inputCls, selectCls } from "./shared"
import { accountName, dateLabel } from "@/scripts/identity.ts"
import { APP_SETTINGS } from "@/config/appSettings"

const buttonCls = "rounded-lg px-3 py-2 text-sm font-medium transition-colors disabled:cursor-not-allowed disabled:opacity-50"

export function AccountsModule() {
  const [accounts, setAccounts] = useState<AccountModel[]>([])
  const [roles, setRoles] = useState<RoleModel[]>([])
  const [permissions, setPermissions] = useState<PermissionModel[]>([])
  const [status, setStatus] = useState<"all" | "active" | "inactive">("active")
  const [cursorStack, setCursorStack] = useState<Array<string | null>>([null])
  const [pageIndex, setPageIndex] = useState(0)
  const [nextCursor, setNextCursor] = useState<string | null>(null)
  const [hasMore, setHasMore] = useState(false)
  const [search, setSearch] = useState("")
  const [roleFilter, setRoleFilter] = useState("all")
  const [selectedAccount, setSelectedAccount] = useState<AccountModel | null>(null)
  const [assignedRoleIds, setAssignedRoleIds] = useState<string[]>([])
  const [selectedRoleId, setSelectedRoleId] = useState("")
  const [permissionIds, setPermissionIds] = useState<string[]>([])
  const [showCreate, setShowCreate] = useState(false)
  const [createForm, setCreateForm] = useState({ email: "", password: "", roleId: "" })
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [notice, setNotice] = useState<string | null>(null)

  const loadPage = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const [page, roleList, permissionList] = await Promise.all([
        identityApi.accounts(cursorStack[pageIndex] ?? null, APP_SETTINGS.pagination.defaultPageSize, status),
        identityApi.roles(),
        identityApi.permissions(),
      ])
      setAccounts(page.items.map((account) => ({
        ...account,
        roles: roleList.filter((role) => account.roleCodes.includes(role.roleCode)),
        additionalPermissions: permissionList.filter((permission) => account.permissionCodes.includes(permission.permissionCode)),
      })))
      setNextCursor(page.nextCursor)
      setHasMore(page.isHasMore)
      setRoles(roleList)
      setPermissions(permissionList)
      setSelectedRoleId((current) => current || roleList[0]?.roleId || "")
      setCreateForm((current) => ({ ...current, roleId: current.roleId || roleList[0]?.roleId || "" }))
    } catch (cause) {
      const message = cause instanceof Error ? cause.message : "Không tải được dữ liệu Identity."
      if (message === "Không tìm thấy dữ liệu.") {
        setAccounts([])
        setNextCursor(null)
        setHasMore(false)
        try {
          const [roleList, permissionList] = await Promise.all([identityApi.roles(), identityApi.permissions()])
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
  }, [cursorStack, pageIndex, status])

  useEffect(() => { void loadPage() }, [loadPage])

  const selectedRole = roles.find((role) => role.roleId === selectedRoleId) ?? null
  useEffect(() => {
    if (!selectedRole) {
      setPermissionIds([])
      return
    }
    let cancelled = false
    setPermissionIds([])
    void identityApi.roleWithPermissions(selectedRole.roleCode).then((role) => {
      if (cancelled) return
      setPermissionIds(role.permissions.map((permission) => permission.permissionId))
      setRoles((current) => current.map((item) => item.roleId === role.roleId ? role : item))
    }).catch((cause) => {
      if (!cancelled) setError(cause instanceof Error ? cause.message : "Không tải được quyền của vai trò.")
    })
    return () => { cancelled = true }
  }, [selectedRoleId])

  const visibleAccounts = accounts.filter((account) => {
    const matchesSearch = !search || `${accountName(account)} ${account.accountEmail}`.toLocaleLowerCase("vi").includes(search.toLocaleLowerCase("vi"))
    const matchesRole = roleFilter === "all" || account.roles.some((role) => role.roleId === roleFilter)
    return matchesSearch && matchesRole
  })

  const resetPages = (nextStatus = status) => {
    setStatus(nextStatus)
    setCursorStack([null])
    setPageIndex(0)
  }

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

  const saveAccountRoles = () => {
    if (!selectedAccount || assignedRoleIds.length === 0) return
    void runAction(
      () => identityApi.updateAccountRoles(selectedAccount.accountId, assignedRoleIds),
      "Đã cập nhật vai trò tài khoản.",
    ).then((succeeded) => { if (succeeded) setSelectedAccount(null) })
  }

  const saveRolePermissions = () => {
    if (!selectedRole) return
    void runAction(
      () => identityApi.updateRole(selectedRole, permissionIds),
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
      const account = await identityApi.register(createForm.email, createForm.password)
      try {
        await identityApi.updateAccountRoles(account.accountId, [role.roleId])
      } catch (cause) {
        throw new Error(`Tài khoản đã tạo với role mặc định nhưng chưa gán được ${role.roleName}: ${cause instanceof Error ? cause.message : "lỗi không xác định"}`)
      }
      setShowCreate(false)
      setCreateForm({ email: "", password: "", roleId: role.roleId })
      setNotice("Đã tạo tài khoản và gán vai trò.")
      resetPages("all")
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : "Không tạo được tài khoản.")
    } finally {
      setSaving(false)
    }
  }

  const openAccount = (account: AccountModel) => {
    setSelectedAccount(account)
    setAssignedRoleIds(account.roles.map((role) => role.roleId))
    void identityApi.profileByAccountId(account.accountId).then((profile) => {
      setSelectedAccount((current) => current?.accountId === account.accountId
        ? { ...current, userProfile: profile }
        : current)
    }).catch(() => {
      // Tài khoản có thể chưa có hồ sơ.
    })
  }

  return (
    <div className="-m-8 flex min-h-[calc(100vh-49px)] flex-col bg-[#f8fafc]">
      <header className="flex min-h-18 items-center justify-between border-b border-[#e2e8f0] bg-white px-8">
        <div>
          <h1 className="text-lg font-bold text-[#0f172a]">Quản lý Định danh &amp; Tài khoản</h1>
          <p className="mt-1 text-xs text-[#64748b]">Dữ liệu trực tiếp từ Identity API · tài khoản, vai trò và quyền</p>
        </div>
        <div className="flex items-center gap-3">
          <input className={`${inputCls} w-64`} placeholder="Tìm tên hoặc email" value={search} onChange={(event) => setSearch(event.target.value)} />
          <button className={`${buttonCls} bg-[#0f52ff] text-white hover:bg-[#0a42dd]`} onClick={() => setShowCreate(true)}>Tạo tài khoản</button>
        </div>
      </header>

      <main className="flex flex-1 flex-col gap-5 p-8">
        {error && <div role="alert" className="flex items-start justify-between gap-4 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700"><span>{error}</span><button className="font-semibold" onClick={() => setError(null)}>Đóng</button></div>}
        {notice && <div role="status" className="rounded-lg border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700">{notice}</div>}

        <section className="flex flex-wrap items-center justify-between gap-3">
          <div className="flex items-center gap-3">
            <label className="text-sm text-slate-600" htmlFor="account-status">Trạng thái</label>
            <select id="account-status" className={selectCls} value={status} onChange={(event) => resetPages(event.target.value as typeof status)}>
              <option value="active">Đang hoạt động</option>
              <option value="inactive">Đã vô hiệu hóa</option>
              <option value="all">Tất cả</option>
            </select>
            <label className="text-sm text-slate-600" htmlFor="account-role">Vai trò</label>
            <select id="account-role" className={selectCls} value={roleFilter} onChange={(event) => setRoleFilter(event.target.value)}>
              <option value="all">Tất cả vai trò</option>
              {roles.map((role) => <option key={role.roleId} value={role.roleId}>{role.roleName}</option>)}
            </select>
          </div>
          <button className={`${buttonCls} border border-slate-200 bg-white text-slate-700 hover:bg-slate-50`} onClick={() => void loadPage()} disabled={loading}>Làm mới</button>
        </section>

        <div className="grid items-start gap-5 xl:grid-cols-[minmax(0,1fr)_380px]">
          <section className="overflow-hidden rounded-2xl border border-[#e2e8f0] bg-white shadow-sm">
            <div className="flex items-center justify-between border-b border-slate-100 px-5 py-4">
              <div><h2 className="font-semibold text-slate-900">Danh sách tài khoản</h2><p className="mt-1 text-xs text-slate-500">Trang {pageIndex + 1} · {visibleAccounts.length} tài khoản trong trang</p></div>
              <span className="rounded-md bg-slate-100 px-2.5 py-1 text-xs text-slate-600">Identity AccountModel</span>
            </div>
            <div className="overflow-x-auto">
              <table className="w-full min-w-190 text-left text-sm">
                <thead className="bg-slate-50 text-xs uppercase tracking-wide text-slate-500"><tr><th className="px-5 py-3 font-semibold">Tài khoản</th><th className="px-5 py-3 font-semibold">Vai trò</th><th className="px-5 py-3 font-semibold">Ngày tạo</th><th className="px-5 py-3 font-semibold">Trạng thái</th><th className="px-5 py-3" /></tr></thead>
                <tbody className="divide-y divide-slate-100">
                  {loading ? <tr><td colSpan={5} className="px-5 py-10 text-center text-slate-500">Đang tải dữ liệu...</td></tr> : visibleAccounts.map((account) => (
                    <tr key={account.accountId} className="hover:bg-blue-50/50">
                      <td className="px-5 py-3.5"><button className="text-left" onClick={() => openAccount(account)}><span className="block font-medium text-slate-900">{accountName(account)}</span><span className="mt-0.5 block text-xs text-slate-500">{account.accountEmail}</span></button></td>
                      <td className="px-5 py-3.5"><div className="flex flex-wrap gap-1.5">{account.roles.length ? account.roles.map((role) => <span key={role.roleId} className="rounded-md bg-blue-50 px-2 py-1 text-xs font-medium text-blue-700">{role.roleName}</span>) : <span className="text-xs text-slate-400">Chưa gán role</span>}</div></td>
                      <td className="px-5 py-3.5 text-slate-600">{dateLabel(account.accountCreatedAt)}</td>
                      <td className="px-5 py-3.5"><span className={`rounded-md px-2 py-1 text-xs font-semibold ${account.accountIsActive ? "bg-emerald-50 text-emerald-700" : "bg-red-50 text-red-700"}`}>{account.accountIsActive ? "Hoạt động" : "Vô hiệu hóa"}</span></td>
                      <td className="px-5 py-3.5 text-right"><button className="text-xs font-semibold text-blue-700 hover:underline" onClick={() => openAccount(account)}>Chi tiết</button></td>
                    </tr>
                  ))}
                  {!loading && visibleAccounts.length === 0 && <tr><td colSpan={5} className="px-5 py-10 text-center text-slate-500">Không có tài khoản phù hợp.</td></tr>}
                </tbody>
              </table>
            </div>
            <div className="flex items-center justify-between border-t border-slate-100 px-5 py-3">
              <span className="text-xs text-slate-500">Phân trang theo cursor của Backend</span>
              <div className="flex gap-2">
                <button className={`${buttonCls} border border-slate-200 bg-white text-slate-700`} disabled={pageIndex === 0 || loading} onClick={() => setPageIndex((index) => index - 1)}>Trước</button>
                <button className={`${buttonCls} border border-slate-200 bg-white text-slate-700`} disabled={!hasMore || !nextCursor || loading} onClick={() => { setCursorStack((stack) => [...stack.slice(0, pageIndex + 1), nextCursor]); setPageIndex((index) => index + 1) }}>Sau</button>
              </div>
            </div>
          </section>

          <section className="rounded-2xl border border-[#e2e8f0] bg-white p-5 shadow-sm">
            <div className="mb-4"><h2 className="font-semibold text-slate-900">Quyền của vai trò</h2><p className="mt-1 text-xs text-slate-500">Cập nhật qua AuthorizationController</p></div>
            <FormField label="Vai trò">
              <select className={selectCls} value={selectedRoleId} onChange={(event) => setSelectedRoleId(event.target.value)}>
                {roles.map((role) => <option key={role.roleId} value={role.roleId}>{role.roleName} ({role.roleCode})</option>)}
              </select>
            </FormField>
            {selectedRole ? <>
              <div className="mb-3 flex items-center justify-between"><p className="text-xs font-semibold uppercase tracking-wide text-slate-500">Permission</p><span className="text-xs text-slate-500">{permissionIds.length} đã chọn</span></div>
              <div className="max-h-97.5 space-y-1 overflow-y-auto rounded-lg border border-slate-100 p-2">
                {permissions.map((permission) => <label key={permission.permissionId} className="flex cursor-pointer items-start gap-2 rounded-md p-2 hover:bg-slate-50"><input type="checkbox" className="mt-0.5 accent-blue-600" checked={permissionIds.includes(permission.permissionId)} onChange={(event) => setPermissionIds((ids) => event.target.checked ? [...ids, permission.permissionId] : ids.filter((id) => id !== permission.permissionId))} /><span><span className="block text-xs font-medium text-slate-800">{permission.permissionName}</span><span className="text-[11px] text-slate-500">{permission.permissionCode}</span></span></label>)}
                {permissions.length === 0 && <p className="p-3 text-xs text-slate-500">Backend chưa trả về permission.</p>}
              </div>
              <button className={`${buttonCls} mt-4 w-full bg-[#0f52ff] text-white hover:bg-[#0a42dd]`} disabled={saving || permissionIds.length === 0} onClick={saveRolePermissions}>Lưu quyền vai trò</button>
            </> : <p className="text-sm text-slate-500">Chưa có vai trò từ Backend.</p>}
          </section>
        </div>
      </main>

      {selectedAccount && <Modal title="Chi tiết tài khoản Identity" onClose={() => setSelectedAccount(null)} wide>
        <div className="grid gap-5 lg:grid-cols-2">
          <div className="space-y-3">
            <div className="rounded-lg bg-slate-50 p-3"><p className="text-xs text-slate-500">AccountId</p><p className="mt-1 break-all font-mono text-xs text-slate-800">{selectedAccount.accountId}</p></div>
            <div className="rounded-lg bg-slate-50 p-3"><p className="text-xs text-slate-500">Email</p><p className="mt-1 text-sm font-medium text-slate-900">{selectedAccount.accountEmail}</p></div>
            <div className="grid grid-cols-2 gap-3"><div className="rounded-lg bg-slate-50 p-3"><p className="text-xs text-slate-500">Trạng thái</p><p className="mt-1 text-sm font-medium">{selectedAccount.accountIsActive ? "Hoạt động" : "Vô hiệu hóa"}</p></div><div className="rounded-lg bg-slate-50 p-3"><p className="text-xs text-slate-500">Ngày tạo</p><p className="mt-1 text-sm font-medium">{dateLabel(selectedAccount.accountCreatedAt)}</p></div></div>
            <div className="rounded-lg border border-slate-100 p-3"><p className="mb-2 text-xs font-semibold text-slate-600">UserProfile</p>{selectedAccount.userProfile ? <dl className="grid grid-cols-2 gap-2 text-xs"><dt className="text-slate-500">Mã định danh</dt><dd>{selectedAccount.userProfile.userProfileId}</dd><dt className="text-slate-500">Họ tên</dt><dd>{accountName(selectedAccount)}</dd><dt className="text-slate-500">Điện thoại</dt><dd>{selectedAccount.userProfile.userProfilePhoneNumber || "—"}</dd></dl> : <p className="text-xs text-slate-500">Tài khoản chưa có hồ sơ.</p>}</div>
          </div>
          <div>
            <p className="mb-2 text-xs font-semibold uppercase tracking-wide text-slate-500">Vai trò được gán</p>
            <div className="max-h-64 space-y-1 overflow-y-auto rounded-lg border border-slate-100 p-2">
              {roles.map((role) => <label key={role.roleId} className="flex cursor-pointer items-center gap-2 rounded-md p-2 hover:bg-slate-50"><input type="checkbox" className="accent-blue-600" checked={assignedRoleIds.includes(role.roleId)} onChange={(event) => setAssignedRoleIds((ids) => event.target.checked ? [...ids, role.roleId] : ids.filter((id) => id !== role.roleId))} /><span className="text-sm text-slate-800">{role.roleName}</span><span className="ml-auto text-xs text-slate-400">{role.roleCode}</span></label>)}
            </div>
            <button className={`${buttonCls} mt-3 w-full bg-blue-600 text-white hover:bg-blue-700`} disabled={saving || assignedRoleIds.length === 0} onClick={saveAccountRoles}>Lưu vai trò tài khoản</button>
            <div className="mt-5 flex gap-2 border-t border-slate-100 pt-4">
              {selectedAccount.accountIsActive ? <button className={`${buttonCls} border border-red-200 text-red-700 hover:bg-red-50`} disabled={saving} onClick={() => void runAction(() => identityApi.deactivateAccount(selectedAccount.accountId), "Đã vô hiệu hóa tài khoản.").then((succeeded) => { if (succeeded) setSelectedAccount(null) })}>Vô hiệu hóa</button> : <button className={`${buttonCls} border border-emerald-200 text-emerald-700 hover:bg-emerald-50`} disabled={saving} onClick={() => void runAction(() => identityApi.activateAccount(selectedAccount.accountEmail), "Đã kích hoạt tài khoản.").then((succeeded) => { if (succeeded) setSelectedAccount(null) })}>Kích hoạt</button>}
              <button className={`${buttonCls} ml-auto border border-slate-200 text-slate-700 hover:bg-slate-50`} onClick={() => setSelectedAccount(null)}>Đóng</button>
            </div>
          </div>
        </div>
      </Modal>}

      {showCreate && <Modal title="Tạo tài khoản Identity" onClose={() => setShowCreate(false)}>
        <form onSubmit={(event) => void createAccount(event)}>
          <FormField label="Email"><input className={inputCls} type="email" required maxLength={255} value={createForm.email} onChange={(event) => setCreateForm({ ...createForm, email: event.target.value })} /></FormField>
          <FormField label="Mật khẩu"><input className={inputCls} type="password" required value={createForm.password} onChange={(event) => setCreateForm({ ...createForm, password: event.target.value })} /><span className="mt-1 block text-xs text-slate-500">Theo AccountRules cấu hình tại Identity Backend.</span></FormField>
          <FormField label="Vai trò"><select className={selectCls} required value={createForm.roleId} onChange={(event) => setCreateForm({ ...createForm, roleId: event.target.value })}>{roles.map((role) => <option key={role.roleId} value={role.roleId}>{role.roleName}</option>)}</select></FormField>
          <p className="mb-4 rounded-lg bg-amber-50 p-3 text-xs text-amber-800">Backend tạo tài khoản bằng endpoint register và khởi tạo hồ sơ riêng. Họ tên/mã định danh sẽ bổ sung qua API hồ sơ khi luồng đó được bật.</p>
          <div className="flex justify-end gap-2"><button type="button" className={`${buttonCls} border border-slate-200 text-slate-700`} onClick={() => setShowCreate(false)}>Hủy</button><button type="submit" className={`${buttonCls} bg-blue-600 text-white`} disabled={saving || !roles.length}>{saving ? "Đang tạo..." : "Tạo và gán vai trò"}</button></div>
        </form>
      </Modal>}
    </div>
  )
}

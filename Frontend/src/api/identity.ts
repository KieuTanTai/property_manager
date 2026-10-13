export type UserProfileModel = {
  userProfileId: string
  userProfileAccountId: string
  userProfileFirstName: string | null
  userProfileLastName: string | null
  userProfileDateOfBirth: string | null
  userProfileGender: "Unspecified" | "Male" | "Female"
  userProfilePhoneNumber: string | null
  userProfileAddress: string | null
  userProfileAvatarUrl: string | null
}

export type PermissionModel = {
  permissionId: string
  permissionCode: string
  permissionName: string
  permissionDescription: string | null
  permissionIsActive: boolean
  permissionCreatedAt: string
  permissionUpdatedAt: string
}

export type RoleModel = {
  roleId: string
  roleCode: string
  roleName: string
  roleDescription: string | null
  roleIsActive: boolean
  roleCreatedAt: string
  roleUpdatedAt: string
  permissions: PermissionModel[]
}

export type AccountModel = {
  accountId: string
  accountEmail: string
  accountIsActive: boolean
  accountCreatedAt: string
  accountUpdatedAt: string
  roles: RoleModel[]
  additionalPermissions: PermissionModel[]
  userProfile: UserProfileModel | null
}

export type CursorPage<T> = {
  items: T[]
  nextCursor: string | null
  isHasMore: boolean
}

export type RecordAuthResponse = {
  accountId: string
  email: string
  isActive: boolean
  roleCodes: string[]
  permissionCodes: string[]
  accountCreatedAt: string
  accountUpdatedAt: string
}

export type RecordLoginResponse = {
  account: RecordAuthResponse
  profile: UserProfileModel | null
  claims: { type: string; value: string }[]
}

const apiBase = (import.meta.env.VITE_IDENTITY_API_BASE_URL || "/api").replace(/\/$/, "")

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  let response: Response
  try {
    response = await fetch(`${apiBase}${path}`, {
      ...init,
      headers: {
        ...(init?.body ? { "Content-Type": "application/json" } : {}),
        ...init?.headers,
      },
    })
  } catch {
    throw new Error(`Không thể kết nối Identity API tại ${apiBase}. Kiểm tra backend, HTTPS và cấu hình CORS.`)
  }

  if (!response.ok) {
    const detail = await response.text()
    if (response.status === 404 && /no accounts found/i.test(detail)) {
      return { items: [], nextCursor: null, isHasMore: false } as T
    }
    if (response.status === 404 && /no (roles|permissions) found/i.test(detail)) {
      return [] as T
    }
    throw new Error(detail || `Identity API trả về HTTP ${response.status}.`)
  }
  if (response.status === 204) return undefined as T
  return response.json() as Promise<T>
}

export const identityApi = {
  accounts(cursor: string | null, pageSize: number, status: "all" | "active" | "inactive") {
    const query = new URLSearchParams({ pageSize: String(pageSize) })
    if (cursor) query.set("cursor", cursor)
    if (status === "all") query.set("isGetProfile", "true")
    if (status !== "all") query.set("isActive", String(status === "active"))
    const path = status === "all" ? "/Account/analytics" : "/Account/status"
    return request<CursorPage<AccountModel>>(`${path}?${query}`)
  },
  roles() {
    return request<RoleModel[]>("/Authorization/roles")
  },
  permissions() {
    return request<PermissionModel[]>("/Authorization/permissions")
  },
  login(email: string, password: string) {
    return request<RecordLoginResponse>("/Account/login", {
      method: "POST",
      body: JSON.stringify({ email, password }),
    })
  },
  register(email: string, password: string) {
    return request<RecordAuthResponse>("/Account/register", {
      method: "POST",
      body: JSON.stringify({ email, password }),
    })
  },
  updateAccountRoles(accountId: string, roleIds: string[]) {
    return request<number>(`/Authorization/accounts/${accountId}/roles`, {
      method: "PUT",
      body: JSON.stringify({ ids: roleIds }),
    })
  },
  updateRole(role: RoleModel, permissionIds: string[]) {
    return request<number>(`/Authorization/roles/${role.roleId}`, {
      method: "PUT",
      body: JSON.stringify({
        name: role.roleName,
        description: role.roleDescription,
        isActive: role.roleIsActive,
        permissionIds,
      }),
    })
  },
  activateAccount(email: string) {
    return request<number>("/Account/active", {
      method: "PUT",
      body: JSON.stringify(email),
    })
  },
  deactivateAccount(accountId: string) {
    return request<number>(`/Account/delete/${accountId}`, { method: "DELETE" })
  },
}

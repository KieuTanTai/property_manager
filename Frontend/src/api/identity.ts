import type {
    AccountModel,
    CursorPage,
    PermissionModel,
    RecordAuthResponse,
    RecordLoginResponse,
    RoleModel,
    UserProfileModel,
} from "@/types/identity"

export type {
    AccountModel,
    CursorPage,
    PermissionModel,
    RecordAuthResponse,
    RecordLoginResponse,
    RoleModel,
    UserProfileModel,
} from "@/types/identity"

type ApiProfile = {
    identityCode: string
    accountId: string
    firstName: string | null
    lastName: string | null
    dateOfBirth: string | null
    gender: UserProfileModel["userProfileGender"]
    phoneNumber: string | null
    address: string | null
    avatarUrl: string | null
}
type ApiAccount = Omit<AccountModel, "userProfile"> & { userProfile: ApiProfile | null }

// Vite proxies /api/* to the Identity ASP.NET service in development.
const apiBase = (import.meta.env.VITE_IDENTITY_API_BASE_URL || "/api").replace(/\/$/, "")

async function request<T>(path: string, init?: RequestInit): Promise<T> {
    let response: Response
    try {
        const headers = new Headers(init?.headers)
        if (init?.body && !headers.has("Content-Type")) headers.set("Content-Type", "application/json")
        response = await fetch(`${apiBase}${path}`, {...init, headers})
    } catch {
        throw new Error(`Không thể kết nối Identity API tại ${apiBase}. Kiểm tra Backend C# có chạy không.`)
    }

    if (!response.ok) {
        const body = await response.text()
        let detail = body
        try {
            const problem = JSON.parse(body) as { detail?: string; message?: string; title?: string }
            detail = problem.detail || problem.message || problem.title || body
        } catch {
            // ASP.NET có thể trả lỗi dạng plain text.
        }
        throw new Error(detail || `Identity API trả về HTTP ${response.status}.`)
    }
    if (response.status === 204) return undefined as T
    return await response.json() as Promise<T>
}

function accountPagePath(cursor: string | null, pageSize: number, status: "all" | "active" | "inactive") {
    const path = status === "all" ? "/account/analytics" : "/account/status"
    const query = new URLSearchParams({pageSize: String(pageSize), isGetProfile: "true"})
    if (cursor) query.set("cursor", cursor)
    if (status !== "all") query.set("isActive", String(status === "active"))
    return `${path}?${query}`
}

export const identityApi = {
    async accounts(cursor: string | null, pageSize: number, status: "all" | "active" | "inactive") {
        try {
            const page = await request<CursorPage<ApiAccount>>(accountPagePath(cursor, pageSize, status))
            return {
                ...page, items: page.items.map((account) => ({
                    ...account,
                    userProfile: account.userProfile ? toProfileModel(account.userProfile) : null,
                    roleCodes: account.roles?.map((role) => role.roleCode) ?? [],
                    permissionCodes: account.additionalPermissions?.map((permission) => permission.permissionCode) ?? [],
                }))
            }
        } catch (error) {
            // ASP.NET reports an empty page as 404.
            if (error instanceof Error && /no accounts found/i.test(error.message)) {
                return {items: [], nextCursor: null, isHasMore: false}
            }
            throw error
        }
    },
    roles() {
        return request<RoleModel[]>("/authorization/roles")
    },
    roleWithPermissions(roleCode: string) {
        return request<RoleModel>(`/authorization/roles/${encodeURIComponent(roleCode)}`)
    },
    permissions() {
        return request<PermissionModel[]>("/authorization/permissions")
    },
    async profileByAccountId(accountId: string) {
        const query = new URLSearchParams({accountId})
        const profile = await request<ApiProfile>(`/profile/profile?${query}`)
        return toProfileModel(profile)
    },
    async login(email: string, password: string): Promise<RecordLoginResponse> {
        const result = await request<RecordLoginResponse & { profile: ApiProfile | null }>("/account/login", {
            method: "POST", body: JSON.stringify({email, password}),
        })
        return {...result, profile: result.profile ? toProfileModel(result.profile) : null}
    },
    register(email: string, password: string): Promise<RecordAuthResponse> {
        return request("/account/register", {method: "POST", body: JSON.stringify({email, password})})
    },
    updateAccountRoles(accountId: string, roleIds: string[]) {
        return request<number>(`/authorization/accounts/${encodeURIComponent(accountId)}/roles`, {
            method: "PUT", body: JSON.stringify({ids: roleIds}),
        })
    },
    updateRole(role: RoleModel, permissionIds: string[]) {
        return request<number>(`/authorization/roles/${encodeURIComponent(role.roleId)}`, {
            method: "PUT",
            body: JSON.stringify({
                name: role.roleName,
                description: role.roleDescription,
                isActive: role.roleIsActive,
                permissionIds
            }),
        })
    },
    activateAccount(email: string) {
        return request<number>("/account/active", {method: "PUT", body: JSON.stringify(email)})
    },
    deactivateAccount(accountId: string) {
        return request<number>(`/account/delete/${encodeURIComponent(accountId)}`, {method: "DELETE"})
    },
}

function toProfileModel(profile: ApiProfile): UserProfileModel {
    return {
        userProfileId: profile.identityCode,
        userProfileAccountId: profile.accountId,
        userProfileFirstName: profile.firstName,
        userProfileLastName: profile.lastName,
        userProfileDateOfBirth: profile.dateOfBirth,
        userProfileGender: profile.gender,
        userProfilePhoneNumber: profile.phoneNumber,
        userProfileAddress: profile.address,
        userProfileAvatarUrl: profile.avatarUrl,
    }
}

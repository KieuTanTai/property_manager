export type UserProfileModel = {
  userProfileId: string
  userProfileAccountId: string
  userProfileFirstName: string | null
  userProfileLastName: string | null
  userProfileDateOfBirth: string | null
  userProfileGender: "Unspecified" | "Male" | "Female" | null
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
  permissionCreatedAt: string | null
  permissionUpdatedAt: string | null
}

export type RoleModel = {
  roleId: string
  roleCode: string
  roleName: string
  roleDescription: string | null
  roleIsActive: boolean
  roleCreatedAt: string | null
  roleUpdatedAt: string | null
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
  roleCodes: string[]
  permissionCodes: string[]
}

export type CursorPage<T> = { items: T[]; nextCursor: string | null; isHasMore: boolean }
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

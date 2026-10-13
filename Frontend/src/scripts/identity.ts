import type { AccountModel } from "../api/identity"

export const accountName = (account: AccountModel) => {
  const profile = account.userProfile
  return [profile?.userProfileFirstName, profile?.userProfileLastName]
    .filter(Boolean)
    .join(" ") || account.accountEmail
}

export const dateLabel = (value: string) =>
  new Intl.DateTimeFormat("vi-VN", { dateStyle: "medium" }).format(new Date(value))

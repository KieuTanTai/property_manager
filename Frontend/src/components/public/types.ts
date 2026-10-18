export interface PublicHomeProps {
    onManager: () => void
}

export interface LoginModalProps {
    onClose: () => void
    onLogin: (account: { accountId: string; email: string; roleCodes: string[] }) => void
}

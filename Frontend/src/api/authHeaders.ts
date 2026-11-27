const SESSION_KEY = "managerspace.session"

export function createApiHeaders(headers?: HeadersInit, includeAccessToken = true): Headers {
    const requestHeaders = new Headers(headers)

    if (!includeAccessToken || typeof window === "undefined") {
        return requestHeaders
    }

    try {
        const session = JSON.parse(sessionStorage.getItem(SESSION_KEY) ?? "null") as {
            accessToken?: string
        } | null
        if (session?.accessToken) {
            requestHeaders.set("Authorization", `Bearer ${session.accessToken}`)
        }
    } catch {
        // A missing or invalid local session leaves public requests anonymous.
    }

    return requestHeaders
}

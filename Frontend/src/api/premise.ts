import {PremiseModel} from "@/components/premises/types.ts";


const apiBase = (import.meta.env.VITE_IDENTITY_API_BASE_URL || "/api").replace(/\/$/, "")

async function request<T>(path: string, init?: RequestInit): Promise<T> {
    let response: Response
    try {
        const headers = new Headers(init?.headers)
        if (init?.body && !headers.has("Content-Type")) headers.set("Content-Type", "application/json")
        response = await fetch(`${apiBase}${path}`, { ...init, headers })
    } catch {
        throw new Error(`Không thể kết nối Premise API tại ${apiBase}. Kiểm tra Backend C# có chạy không.`)
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
        throw new Error(detail || `Premise API trả về HTTP ${response.status}.`)
    }
    if (response.status === 204) return undefined as T
    return response.json() as Promise<T>
}

export const premiseApi = {
    async getPremise()
    {
        return request<PremiseModel[]>("/api/PremiseController");
    }
}
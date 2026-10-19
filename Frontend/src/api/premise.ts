import type {
  LocationModel,
  PremiseMediaModel,
  PremiseModel,
} from "@/components/premises/types"

const apiBase = (import.meta.env.VITE_IDENTITY_API_BASE_URL || "/api").replace(/\/$/, "")

export type PremiseRequest = {
  premiseName: string
  premiseLocationId: string
  premiseStatus: "Available" | "Reserved" | "Rented" | "Maintenance"
  premisePosition: number
  premiseFloor: number
  premiseArea: string
  premiseDescription: string | null
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  let response: Response
  try {
    const headers = new Headers(init?.headers)
    if (init?.body && !headers.has("Content-Type")) headers.set("Content-Type", "application/json")
    response = await fetch(`${apiBase}${path}`, { ...init, headers })
  } catch {
    throw new Error(`Không thể kết nối Premise API tại ${apiBase}. Kiểm tra backend và HTTPS.`)
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
    if (response.status === 404 && /no (premises|locations) found/i.test(detail)) {
      return [] as T
    }
    throw new Error(detail || `Premise API trả về HTTP ${response.status}.`)
  }
  if (response.status === 204) return undefined as T
  return response.json() as Promise<T>
}

export const premiseApi = {
  getPremise() {
    return request<PremiseModel[]>("/premise")
  },
  getLocations() {
    return request<LocationModel[]>("/location")
  },
  getImages(premiseId: string) {
    return request<PremiseMediaModel[]>(`/Premise/${encodeURIComponent(premiseId)}/images`)
  },
  createPremise(body: PremiseRequest) {
    return request<PremiseModel>("/Premise", { method: "POST", body: JSON.stringify(body) })
  },
  updatePremise(premiseId: string, body: PremiseRequest) {
    return request<PremiseModel>(`/Premise/${encodeURIComponent(premiseId)}`, {
      method: "PUT",
      body: JSON.stringify(body),
    })
  },
  deletePremise(premiseId: string) {
    return request<void>(`/Premise/${encodeURIComponent(premiseId)}`, { method: "DELETE" })
  },
  addImage(premiseId: string, imageUrl: string) {
    return request<PremiseMediaModel>(`/Premise/${encodeURIComponent(premiseId)}/images`, {
      method: "POST",
      body: JSON.stringify({ imageUrl }),
    })
  },
  createLocation(locationAddress: string) {
    return request<LocationModel>("/Location", {
      method: "POST",
      body: JSON.stringify({ locationAddress }),
    })
  },
  updateLocation(locationId: string, locationAddress: string) {
    return request<LocationModel>(`/Location/${encodeURIComponent(locationId)}`, {
      method: "PUT",
      body: JSON.stringify({ locationAddress }),
    })
  },
  deleteLocation(locationId: string) {
    return request<void>(`/Location/${encodeURIComponent(locationId)}`, { method: "DELETE" })
  },
}

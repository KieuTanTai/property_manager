import {
  type BusinessTypeModel,
  type LocationModel,
  type PremiseMediaModel,
  type PremiseModel,
  type ProductResponse,
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

export type ProductRequest = {
  productName: string
  productDescription: string | null
}

export type PublicPremise = {
  id: string
  name: string
  floor: number
  position: number
  area: string
  status: "available" | "reserved" | "rented" | "maintenance"
  locationId: string
  location: string
  description: string
  media: { id: number; url: string }[]
  openingHours?: string
  floorArea?: string
  leasableArea?: string
  storeCount?: string
  parkingCapacity?: string
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  let response: Response
  try {
    const headers = new Headers(init?.headers)
    if (init?.body && !headers.has("Content-Type")) headers.set("Content-Type", "application/json")
    response = await fetch(`${apiBase}${path}`, { ...init, headers })
    console.log("response: ");
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
    if (response.status === 404 && /no (premises|locations|products) found/i.test(detail)) {
      return [] as T
    }
    throw new Error(detail || `Premise API trả về HTTP ${response.status}.`)
  }
  if (response.status === 204) return undefined as T
  return response.json() as Promise<T>
}

export const premiseApi = {
  async getPublicPremises(): Promise<PublicPremise[]> {
    const [premises, locations] = await Promise.all([
      request<Array<{
        premiseId: string
        premiseName: string
        premiseLocationId: string
        premiseStatus: "Available" | "Reserved" | "Rented" | "Maintenance"
        premisePosition: number
        premiseFloor: number
        premiseArea: string
        premiseDescription: string | null
      }>>("/Premise"),
      request<LocationModel[]>("/Location"),
    ])
    const locationById = new Map(locations.map((location) => [location.locationId, location.locationAddress]))
    const mediaByPremise = await Promise.all(premises.map(async (premise) => {
      try {
        const images = await request<PremiseMediaModel[]>(`/Premise/${encodeURIComponent(premise.premiseId)}/images`)
        return [premise.premiseId, images.map((image) => ({id: image.imageId, url: image.imageUrl}))] as const
      } catch {
        return [premise.premiseId, []] as const
      }
    }))
    const mediaMap = new Map(mediaByPremise)
    return premises.map((premise) => ({
      id: premise.premiseId,
      name: premise.premiseName,
      floor: premise.premiseFloor,
      position: premise.premisePosition,
      area: premise.premiseArea,
      status: premise.premiseStatus.toLowerCase() as PublicPremise["status"],
      locationId: premise.premiseLocationId,
      location: locationById.get(premise.premiseLocationId) ?? "",
      description: premise.premiseDescription ?? "",
      media: mediaMap.get(premise.premiseId) ?? [],
    }))
  },
  getPremise() {
    return request<PremiseModel[]>("/Premise")
  },
  getLocations() {
    return request<LocationModel[]>("/Location")
  },
  getBusinessTypes() {
    return request<BusinessTypeModel[]>("/BusinessType")
  },
  getProducts() {
    return request<ProductResponse[]>("/Product")
  },
  getProductsByBusinessType(businessTypeId: string) {
    return request<ProductResponse[]>(
      `/Product/by-business-type/${encodeURIComponent(businessTypeId)}`,
    )
  },
  createProduct(body: ProductRequest) {
    return request<ProductResponse>("/Product", {
      method: "POST",
      body: JSON.stringify(body),
    })
  },
  createProductForBusinessType(body: ProductRequest & { businessTypeId: string }) {
    return request<ProductResponse>("/Product/with-business-type", {
      method: "POST",
      body: JSON.stringify(body),
    })
  },
  updateProduct(productId: string, body: ProductRequest) {
    return request<ProductResponse>(`/Product/${encodeURIComponent(productId)}`, {
      method: "PUT",
      body: JSON.stringify(body),
    })
  },
  addProductBusinessType(productId: string, businessTypeId: string) {
    return request<void>(`/Product/${encodeURIComponent(productId)}/business-types`, {
      method: "POST",
      body: JSON.stringify({ businessTypeId }),
    })
  },
  removeProductBusinessType(productId: string, businessTypeId: string) {
    return request<void>(
      `/Product/${encodeURIComponent(productId)}/business-types/${encodeURIComponent(businessTypeId)}`,
      { method: "DELETE" },
    )
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
  deleteImage(premiseId: string, imageId: number) {
    return request<void>(
      `/Premise/${encodeURIComponent(premiseId)}/images/${imageId}`,
      { method: "DELETE" },
    )
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

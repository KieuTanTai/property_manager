export type PremiseStatus = "available" | "reserved" | "rented" | "maintenance"
export type PremisesSubTab = "locations" | "business-types" | "premises"

// Backend Premise models are returned with ASP.NET's camel-cased property names.
// Keep these wire types separate from the presentation types below until this
// module exposes its own API controllers/response DTOs.
export type EPremiseStatus = "Available" | "Reserved" | "Rented" | "Maintenance"

export interface PremiseModel {
  premiseId: string
  premiseName: string
  premiseLocationId: string
  premiseStatus: EPremiseStatus
  premisePosition: number
  premiseFloor: number
  premiseArea: string
  premiseDescription: string | null
  premiseCreatedAt: string
  premiseUpdatedAt: string
  premiseLocation: LocationModel | null
  premiseMedia: PremiseMediaModel[]
  premiseBusinessTypes: BusinessTypeModel[]
}

export interface PremiseMediaModel {
  premiseMediaId: number
  premiseMediaPremiseId: string
  premiseMediaImageUrl: string
  premiseMediaCreatedAt: string
  premiseMediaUpdatedAt: string
}

export interface LocationModel {
  locationId: string
  locationAddress: string
  locationCreatedAt: string
  locationUpdatedAt: string
}

export interface BusinessTypeModel {
  businessTypeId: string
  businessTypeName: string
  businessTypeDescription: string | null
  businessTypeIsActive: boolean
  businessTypeCreatedAt: string
  businessTypeUpdatedAt: string
  premises: PremiseModel[]
  whitelistProducts: WhitelistProductModel[]
}

export interface WhitelistProductModel {
  whitelistProductId: string
  whitelistProductName: string
  whitelistProductDescription: string | null
  whitelistProductCreatedAt: string
  whitelistProductUpdatedAt: string
}

export interface PremiseBusinessTypeModel {
  premiseId: string
  businessTypeId: string
  assignedAt: string
}

export interface ProductBusinessTypeModel {
  productId: string
  businessTypeId: string
  assignedAt: string
}

export interface RentedPremiseModel {
  contractId: string
  premiseId: string
}

export interface Premise {
  id: string
  name: string
  floor: number
  position: number
  area: string
  status: PremiseStatus
  locationId: string
  location: string
  description: string
  website?: string
  hotline?: string
  email?: string
  openingHours?: string
  landArea?: string
  floorArea?: string
  leasableArea?: string
  storeCount?: string
  parkingCapacity?: string
  isMockData?: boolean
}

export interface PremiseMedia {
  premiseMediaId: number
  premiseMediaPremiseId: string
  premiseMediaImageUrl: string
  premiseMediaCreatedAt: string
  premiseMediaUpdatedAt: string
}

export interface Location {
  locationId: string
  address: string
  createdAt: string
  updatedAt: string
}

export interface BusinessType {
  businessTypeId: string
  name: string
  description: string
  active: boolean
  createdAt: string
  updatedAt: string
}

export interface WhitelistProduct {
  productId: string
  businessTypeId: string
  name: string
  description: string
}

export interface LocationsTabProps {
  locations: Location[]
  onEdit: (location: Location) => void
  onDelete: (location: Location) => void
}

export interface BusinessTypesTabProps {
  businessTypes: BusinessType[]
  onView: (businessType: BusinessType) => void
}

export interface PremisesTabProps {
  premises: Premise[]
  premiseMedia: PremiseMedia[]
  filteredPremises: Premise[]
  filter: PremiseStatus | "all"
  onFilterChange: (filter: PremiseStatus | "all") => void
  onView: (premise: Premise) => void
}

export interface PremisesSubTabsProps {
  activeTab: PremisesSubTab
  locationCount: number
  businessTypeCount: number
  premiseCount: number
  onChange: (tab: PremisesSubTab) => void
}

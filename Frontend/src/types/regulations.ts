export interface Regulation {
  regulationId: string
  regulationName: string
  regulationDescription: string | null
  regulationFineAmount: number | null
  regulationIsActive: boolean
  regulationCreatedAt: string
  regulationUpdatedAt: string
}

export interface RegulationForm {
  name: string
  description: string
  fineAmount: string
  active: boolean
}



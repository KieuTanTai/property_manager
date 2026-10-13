import type { ContractStatus } from "../api/contracts"

export interface ContractForm {
  accountId: string
  deposit: string
  rentalPrice: string
  premiseReturnDate: string
  status: ContractStatus
  terminationDate: string
  regulationIds: string[]
}

export interface ContractsModuleProps {
  accountId?: string
  readOnly?: boolean
}

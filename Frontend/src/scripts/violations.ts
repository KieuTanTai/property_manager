import type { ContractViolationModel } from "../api/contracts"
import type { Violation } from "../types/modules"

export const mapContractViolationToView = (item: ContractViolationModel, tenant: string): Violation => ({
  id: item.violationId,
  contractId: item.contractId,
  tenant,
  content: item.violationContent,
  penaltyAmount: item.violationPenaltyAmount ?? 0,
  date: item.violationDate.slice(0, 10),
  dueDate: item.violationDueDate.slice(0, 10),
  resolved: item.violationIsResolved,
})

export const filterViolationsForAccount = (
  violations: Violation[],
  contracts: Array<{ contractId: string; contractAccountId: string }>,
  accountId?: string,
  readOnly = false,
) => readOnly
  ? violations.filter((violation) => contracts.some((contract) =>
    contract.contractId === violation.contractId && contract.contractAccountId === accountId,
  ))
  : violations

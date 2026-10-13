import { MOCK_VIOLATION_RECORDS } from "./contracts"
import { mapContractViolationToView } from "../scripts/violations"
import type { Violation } from "../types/modules"

const tenantByContract: Record<string, string> = {
  "0195d7f0-1000-7111-8111-100000000001": "Nguyễn Văn A",
  "0195d7f0-1001-7222-8222-200000000002": "Trần Thị B",
  "0195d7f0-1002-7333-8333-300000000003": "Lê Văn C",
  "0195d7f0-1003-7444-8444-400000000004": "Phạm Thị D",
}

export const getMockTenant = (contractId: string) => tenantByContract[contractId] ?? "Khách thuê"

export const MOCK_VIOLATIONS: Violation[] = MOCK_VIOLATION_RECORDS.map((record) =>
  mapContractViolationToView(record, getMockTenant(record.contractId)),
)

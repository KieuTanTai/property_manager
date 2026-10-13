import type { ContractModel, ContractViolationModel, RegulationModel } from "../api/contracts"

export const MOCK_REGULATIONS: RegulationModel[] = [
  { regulationId: "0195c9a1-1000-7000-8000-000000000001", regulationName: "Sử dụng mặt bằng đúng mục đích", regulationDescription: "Không tự ý thay đổi ngành nghề kinh doanh.", regulationFineAmount: 3000000, regulationIsActive: true, regulationCreatedAt: "2025-01-10T08:00:00Z", regulationUpdatedAt: "2025-01-10T08:00:00Z" },
  { regulationId: "0195c9a1-1001-7000-8000-000000000002", regulationName: "Thanh toán đúng hạn", regulationDescription: "Thanh toán tiền thuê trước ngày 05 hằng tháng.", regulationFineAmount: 1000000, regulationIsActive: true, regulationCreatedAt: "2025-01-12T08:00:00Z", regulationUpdatedAt: "2025-02-01T08:00:00Z" },
  { regulationId: "0195c9a1-1002-7000-8000-000000000003", regulationName: "Đảm bảo an toàn PCCC", regulationDescription: "Tuân thủ yêu cầu phòng cháy chữa cháy của tòa nhà.", regulationFineAmount: 5000000, regulationIsActive: true, regulationCreatedAt: "2025-02-02T08:00:00Z", regulationUpdatedAt: "2025-02-02T08:00:00Z" },
]
const mockContractIds = [
  "0195d7f0-1000-7111-8111-100000000001",
  "0195d7f0-1001-7222-8222-200000000002",
  "0195d7f0-1002-7333-8333-300000000003",
  "0195d7f0-1003-7444-8444-400000000004",
]
const mockViolationContents = [
  "Thanh toán tiền thuê mặt bằng trễ hạn.", "Sử dụng thiết bị âm thanh vượt mức cho phép sau 22:00.",
  "Để hàng hóa lấn chiếm hành lang chung.", "Không phân loại rác theo quy định của khu vực.",
  "Tự ý thay đổi bố trí mặt bằng khi chưa được chấp thuận.", "Chưa bổ sung hồ sơ an toàn phòng cháy chữa cháy.",
  "Đóng/mở cửa hàng không đúng khung giờ đăng ký.", "Để vật dụng chắn lối thoát hiểm.",
]
const mockViolationPenalties = [200000, 300000, 500000, 750000, 1000000, 1500000, 2000000]
export const MOCK_VIOLATION_RECORDS: ContractViolationModel[] = Array.from({ length: 50 }, (_, index) => {
  const date = new Date(Date.UTC(2025, index % 12, (index % 27) + 1))
  const dueDate = new Date(date)
  dueDate.setUTCDate(dueDate.getUTCDate() + 7)
  const id = (index + 1).toString(16).padStart(12, "0")
  return {
    violationId: `0195e8b0-${(index + 1).toString(16).padStart(4, "0")}-7000-8000-${id}`,
    contractId: mockContractIds[index % mockContractIds.length],
    violationContent: mockViolationContents[index % mockViolationContents.length],
    violationPenaltyAmount: mockViolationPenalties[index % mockViolationPenalties.length],
    violationDate: date.toISOString(),
    violationDueDate: dueDate.toISOString(),
    violationIsResolved: index % 4 === 0,
  }
})
export const MOCK_CONTRACTS: ContractModel[] = [
  { contractId: mockContractIds[0], contractAccountId: "0195d6e0-0001-7111-8111-100000000002", contractDeposit: 15000000, contractRentalPrice: 7500000, contractPremiseReturnDate: "2026-12-31T00:00:00Z", contractStatus: "Signed", contractTerminationDate: null, contractCreatedAt: "2025-12-15T09:00:00Z", contractUpdatedAt: "2025-12-20T14:30:00Z", contractViolations: MOCK_VIOLATION_RECORDS.filter((item) => item.contractId === mockContractIds[0]), contractInvoices: [], contractRegulations: [MOCK_REGULATIONS[0], MOCK_REGULATIONS[1]] },
  { contractId: mockContractIds[1], contractAccountId: "0195d6e0-0002-7222-8222-200000000003", contractDeposit: 24000000, contractRentalPrice: 12000000, contractPremiseReturnDate: "2027-01-31T00:00:00Z", contractStatus: "PendingApproval", contractTerminationDate: null, contractCreatedAt: "2026-01-05T08:45:00Z", contractUpdatedAt: "2026-01-05T08:45:00Z", contractViolations: MOCK_VIOLATION_RECORDS.filter((item) => item.contractId === mockContractIds[1]), contractInvoices: [], contractRegulations: [MOCK_REGULATIONS[2]] },
  { contractId: mockContractIds[2], contractAccountId: "0195d6e0-0003-7333-8333-300000000004", contractDeposit: 18000000, contractRentalPrice: 9000000, contractPremiseReturnDate: "2026-06-30T00:00:00Z", contractStatus: "PendingSignature", contractTerminationDate: null, contractCreatedAt: "2026-01-12T11:15:00Z", contractUpdatedAt: "2026-01-15T10:00:00Z", contractViolations: MOCK_VIOLATION_RECORDS.filter((item) => item.contractId === mockContractIds[2]), contractInvoices: [], contractRegulations: [MOCK_REGULATIONS[0], MOCK_REGULATIONS[2]] },
  { contractId: mockContractIds[3], contractAccountId: "0195d6e0-0004-7444-8444-400000000005", contractDeposit: 11400000, contractRentalPrice: 5700000, contractPremiseReturnDate: "2026-09-30T00:00:00Z", contractStatus: "Terminated", contractTerminationDate: "2026-02-15T00:00:00Z", contractCreatedAt: "2025-09-20T13:00:00Z", contractUpdatedAt: "2026-02-15T09:20:00Z", contractViolations: MOCK_VIOLATION_RECORDS.filter((item) => item.contractId === mockContractIds[3]), contractInvoices: [], contractRegulations: [MOCK_REGULATIONS[1]] },
]

import type { Regulation } from "../types/regulations"

export const MOCK_REGULATIONS: Regulation[] = [
  {
    regulationId: "0195c8f4-ad30-7111-8111-100000000001",
    regulationName: "Giữ gìn vệ sinh chung",
    regulationDescription:
      "Không xả rác tại hành lang, lối đi và khu vực sinh hoạt chung.",
    regulationFineAmount: 200000,
    regulationIsActive: true,
    regulationCreatedAt: "2024-01-08T08:30:00",
    regulationUpdatedAt: "2024-01-08T08:30:00",
  },
  {
    regulationId: "0195c8f4-ad31-7222-8222-200000000002",
    regulationName: "Quy định tiếng ồn",
    regulationDescription:
      "Không gây tiếng ồn ảnh hưởng đến các mặt bằng khác sau 22:00.",
    regulationFineAmount: 500000,
    regulationIsActive: true,
    regulationCreatedAt: "2024-01-10T09:15:00",
    regulationUpdatedAt: "2024-06-15T11:20:00",
  },
  {
    regulationId: "0195c8f4-ad32-7333-8333-300000000003",
    regulationName: "Cải tạo mặt bằng",
    regulationDescription:
      "Mọi hoạt động sửa chữa, cải tạo phải được ban quản lý chấp thuận trước.",
    regulationFineAmount: null,
    regulationIsActive: false,
    regulationCreatedAt: "2024-02-05T13:45:00",
    regulationUpdatedAt: "2024-09-01T16:10:00",
  },
]

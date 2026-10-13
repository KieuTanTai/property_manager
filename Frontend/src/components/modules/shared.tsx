import type { ReactNode } from "react"
import type {
  BusinessType,
  Location,
  Premise,
  PremiseMedia,
  PremiseStatus,
  WhitelistProduct,
} from "../premises/types"

// ─── Types ───────────────────────────────────────────────────────────────────

export type ContractStatus = "pending_approval" | "pending_signature" | "canceled" | "expired" | "signed" | "terminated"
export type InvoiceStatus = "unpaid" | "paid" | "overdue"
export type TicketType = "review" | "complaint" | "violation" | "feedback"

export interface Contract {
  id: string
  accountEmail: string
  tenant: string
  premiseName: string
  deposit: number
  rentalPrice: number
  returnDate: string
  status: ContractStatus
  createdAt: string
}

export interface Invoice {
  id: string
  contractId: string
  tenant: string
  premiseName: string
  totalAmount: number
  paymentDate: string
  dueDate: string
  status: InvoiceStatus
}

export interface Violation {
  id: string
  contractId: string
  tenant: string
  content: string
  penaltyAmount: number
  date: string
  dueDate: string
  resolved: boolean
}

export interface Ticket {
  id: string
  accountEmail: string
  content: string
  type: TicketType
  resolved: boolean
  createdAt: string
}

export interface Account {
  id: string
  email: string
  role: string
  active: boolean
  createdAt: string
  name: string
}

// ─── Seed Data ────────────────────────────────────────────────────────────────

export const LOCATIONS: Location[] = [
  {
    locationId: "0195c8f1-7a00-7a31-8b20-4e9f8c1a1001",
    address: "123 Nguyễn Huệ, Q.1, TP.HCM",
    createdAt: "2024-01-05T08:30:00",
    updatedAt: "2024-01-05T08:30:00",
  },
  {
    locationId: "0195c8f1-7a01-7b42-9c31-5fa09d2b2002",
    address: "45 Lê Lợi, Q.1, TP.HCM",
    createdAt: "2024-02-12T10:15:00",
    updatedAt: "2024-08-20T14:05:00",
  },
  {
    locationId: "0195c8f1-7a02-7c53-ad42-60b1ae3c3003",
    address: "78 Đinh Tiên Hoàng, Q.Bình Thạnh",
    createdAt: "2024-03-18T09:45:00",
    updatedAt: "2024-03-18T09:45:00",
  },
]

export const BUSINESS_TYPES: BusinessType[] = [
  {
    businessTypeId: "0195c8f2-8b10-71a1-8c21-4f9a7d1b1001",
    name: "Thời trang",
    description: "Quần áo, giày dép, phụ kiện và trang sức thời trang.",
    active: true,
    createdAt: "2024-01-10T08:30:00",
    updatedAt: "2024-06-15T11:20:00",
  },
  {
    businessTypeId: "0195c8f2-8b11-72b2-9d32-50ab8e2c2002",
    name: "Gia dụng",
    description: "Đồ dùng gia đình, nhà bếp và thiết bị sinh hoạt.",
    active: true,
    createdAt: "2024-01-12T09:15:00",
    updatedAt: "2024-01-12T09:15:00",
  },
  {
    businessTypeId: "0195c8f2-8b12-73c3-ae43-61bc9f3d3003",
    name: "Điện tử",
    description: "Thiết bị điện tử, phụ kiện công nghệ và viễn thông.",
    active: true,
    createdAt: "2024-02-05T13:45:00",
    updatedAt: "2024-08-10T15:30:00",
  },
  {
    businessTypeId: "0195c8f2-8b13-74d4-bf54-72cda04e4004",
    name: "Văn phòng phẩm",
    description: "Dụng cụ học tập, văn phòng và sản phẩm giấy.",
    active: false,
    createdAt: "2024-03-08T10:00:00",
    updatedAt: "2024-09-01T16:10:00",
  },
]

export const WHITELIST_PRODUCTS: WhitelistProduct[] = [
  {
    productId: "0195c8f3-9c20-7111-8111-100000000001",
    businessTypeId: BUSINESS_TYPES[0].businessTypeId,
    name: "Giày",
    description: "Các loại giày thời trang, giày thể thao và giày công sở.",
  },
  {
    productId: "0195c8f3-9c21-7222-8222-200000000002",
    businessTypeId: BUSINESS_TYPES[0].businessTypeId,
    name: "Dép",
    description: "Dép thời trang và dép sử dụng hằng ngày.",
  },
  {
    productId: "0195c8f3-9c22-7333-8333-300000000003",
    businessTypeId: BUSINESS_TYPES[0].businessTypeId,
    name: "Quần áo",
    description: "Quần áo may sẵn dành cho mọi độ tuổi.",
  },
  {
    productId: "0195c8f3-9c23-7444-8444-400000000004",
    businessTypeId: BUSINESS_TYPES[0].businessTypeId,
    name: "Trang sức",
    description: "Trang sức và phụ kiện thời trang không thuộc nhóm hàng cấm.",
  },
  {
    productId: "0195c8f3-9c24-7555-8555-500000000005",
    businessTypeId: BUSINESS_TYPES[0].businessTypeId,
    name: "Túi xách",
    description: "Túi xách, ví và phụ kiện đi kèm.",
  },
  {
    productId: "0195c8f3-9c25-7666-8666-600000000006",
    businessTypeId: BUSINESS_TYPES[1].businessTypeId,
    name: "Đồ dùng nhà bếp",
    description: "Dụng cụ nấu ăn và vật dụng sử dụng trong nhà bếp.",
  },
  {
    productId: "0195c8f3-9c26-7777-8777-700000000007",
    businessTypeId: BUSINESS_TYPES[1].businessTypeId,
    name: "Đồ trang trí",
    description: "Sản phẩm trang trí nội thất và không gian sống.",
  },
  {
    productId: "0195c8f3-9c27-7888-8888-800000000008",
    businessTypeId: BUSINESS_TYPES[2].businessTypeId,
    name: "Điện thoại",
    description: "Điện thoại di động chính hãng và thiết bị liên quan.",
  },
  {
    productId: "0195c8f3-9c28-7999-8999-900000000009",
    businessTypeId: BUSINESS_TYPES[2].businessTypeId,
    name: "Máy tính",
    description: "Máy tính để bàn, máy tính xách tay và linh kiện.",
  },
  {
    productId: "0195c8f3-9c29-7aaa-8aaa-a00000000010",
    businessTypeId: BUSINESS_TYPES[3].businessTypeId,
    name: "Bút viết",
    description: "Các loại bút phục vụ học tập và văn phòng.",
  },
  {
    productId: "0195c8f3-9c2a-7bbb-8bbb-b00000000011",
    businessTypeId: BUSINESS_TYPES[3].businessTypeId,
    name: "Sổ tay",
    description: "Sổ ghi chép và sổ kế hoạch cá nhân.",
  },
]

export const PREMISES: Premise[] = [
  {
    id: "0195c8f6-cd50-7111-8111-100000000001",
    name: "A01",
    floor: 1,
    position: 1,
    area: "45m²",
    status: "rented",
    locationId: LOCATIONS[0].locationId,
    location: LOCATIONS[0].address,
    description: "Mặt bằng góc, view đường lớn",
  },
  {
    id: "0195c8f6-cd51-7222-8222-200000000002",
    name: "A02",
    floor: 1,
    position: 2,
    area: "38m²",
    status: "available",
    locationId: LOCATIONS[0].locationId,
    location: LOCATIONS[0].address,
    description: "Mặt bằng thông thoáng",
  },
  {
    id: "0195c8f6-cd52-7333-8333-300000000003",
    name: "B01",
    floor: 2,
    position: 1,
    area: "60m²",
    status: "reserved",
    locationId: LOCATIONS[1].locationId,
    location: LOCATIONS[1].address,
    description: "Tầng 2, có thang máy",
  },
  {
    id: "0195c8f6-cd55-7444-8444-400000000004",
    name: "B02",
    floor: 2,
    position: 2,
    area: "55m²",
    status: "maintenance",
    locationId: LOCATIONS[1].locationId,
    location: LOCATIONS[1].address,
    description: "Đang sơn sửa lại",
  },
  {
    id: "0195c8f6-cd53-7444-8444-400000000004",
    name: "C01",
    floor: 3,
    position: 1,
    area: "80m²",
    status: "rented",
    locationId: LOCATIONS[2].locationId,
    location: LOCATIONS[2].address,
    description: "Phù hợp văn phòng",
  },
  {
    id: "0195c8f6-cd54-7555-8555-500000000005",
    name: "C02",
    floor: 3,
    position: 2,
    area: "72m²",
    status: "available",
    locationId: LOCATIONS[2].locationId,
    location: LOCATIONS[2].address,
    description: "Tầng cao, view đẹp",
  },
]

export const PREMISE_MEDIA: PremiseMedia[] = [
  {
    premiseMediaId: 1,
    premiseMediaPremiseId: PREMISES[0].id,
    premiseMediaImageUrl:
      "https://images.unsplash.com/photo-1765081598525-626148ec885c?crop=entropy&cs=tinysrgb&fit=crop&fm=jpg&q=80&w=1080",
    premiseMediaCreatedAt: "2026-01-05T08:30:00",
    premiseMediaUpdatedAt: "2026-01-05T08:30:00",
  },
  {
    premiseMediaId: 2,
    premiseMediaPremiseId: PREMISES[0].id,
    premiseMediaImageUrl:
      "https://images.unsplash.com/photo-1781967651630-e680c7bad016?crop=entropy&cs=tinysrgb&fit=crop&fm=jpg&q=80&w=1080",
    premiseMediaCreatedAt: "2026-01-05T08:35:00",
    premiseMediaUpdatedAt: "2026-01-05T08:35:00",
  },
  {
    premiseMediaId: 3,
    premiseMediaPremiseId: PREMISES[1].id,
    premiseMediaImageUrl:
      "https://images.unsplash.com/photo-1759323321196-2813db509285?crop=entropy&cs=tinysrgb&fit=crop&fm=jpg&q=80&w=1080",
    premiseMediaCreatedAt: "2026-01-08T09:15:00",
    premiseMediaUpdatedAt: "2026-01-08T09:15:00",
  },
  {
    premiseMediaId: 4,
    premiseMediaPremiseId: PREMISES[1].id,
    premiseMediaImageUrl:
      "https://images.unsplash.com/photo-1788347102930-60ac65a980a8?crop=entropy&cs=tinysrgb&fit=crop&fm=jpg&q=80&w=1080",
    premiseMediaCreatedAt: "2026-01-08T09:20:00",
    premiseMediaUpdatedAt: "2026-01-08T09:20:00",
  },
  {
    premiseMediaId: 5,
    premiseMediaPremiseId: PREMISES[2].id,
    premiseMediaImageUrl:
      "https://images.unsplash.com/photo-1761333482894-700fc6aebd47?crop=entropy&cs=tinysrgb&fit=crop&fm=jpg&q=80&w=1080",
    premiseMediaCreatedAt: "2026-01-12T13:00:00",
    premiseMediaUpdatedAt: "2026-01-12T13:00:00",
  },
  {
    premiseMediaId: 6,
    premiseMediaPremiseId: PREMISES[3].id,
    premiseMediaImageUrl:
      "https://images.unsplash.com/photo-1778513597815-4cd41539a2a4?crop=entropy&cs=tinysrgb&fit=crop&fm=jpg&q=80&w=1080",
    premiseMediaCreatedAt: "2026-01-15T10:00:00",
    premiseMediaUpdatedAt: "2026-01-15T10:00:00",
  },
  {
    premiseMediaId: 7,
    premiseMediaPremiseId: PREMISES[4].id,
    premiseMediaImageUrl:
      "https://images.unsplash.com/photo-1790427606600-8ebd7d33ae36?crop=entropy&cs=tinysrgb&fit=crop&fm=jpg&q=80&w=1080",
    premiseMediaCreatedAt: "2026-01-18T14:20:00",
    premiseMediaUpdatedAt: "2026-01-18T14:20:00",
  },
  {
    premiseMediaId: 8,
    premiseMediaPremiseId: PREMISES[5].id,
    premiseMediaImageUrl:
      "https://images.unsplash.com/photo-1763120692175-13310322c6aa?crop=entropy&cs=tinysrgb&fit=crop&fm=jpg&q=80&w=1080",
    premiseMediaCreatedAt: "2026-01-20T11:10:00",
    premiseMediaUpdatedAt: "2026-01-20T11:10:00",
  },
  {
    premiseMediaId: 9,
    premiseMediaPremiseId: PREMISES[0].id,
    premiseMediaImageUrl:
      "https://images.unsplash.com/photo-1784790435603-58b273bd7803?crop=entropy&cs=tinysrgb&fit=crop&fm=jpg&q=80&w=1080",
    premiseMediaCreatedAt: "2026-01-05T08:40:00",
    premiseMediaUpdatedAt: "2026-01-05T08:40:00",
  },
  {
    premiseMediaId: 10,
    premiseMediaPremiseId: PREMISES[0].id,
    premiseMediaImageUrl:
      "https://images.unsplash.com/photo-1790257186039-96dc5874db1a?crop=entropy&cs=tinysrgb&fit=crop&fm=jpg&q=80&w=1080",
    premiseMediaCreatedAt: "2026-01-05T08:45:00",
    premiseMediaUpdatedAt: "2026-01-05T08:45:00",
  },
  {
    premiseMediaId: 11,
    premiseMediaPremiseId: PREMISES[0].id,
    premiseMediaImageUrl:
      "https://images.unsplash.com/photo-1790427606600-8ebd7d33ae36?crop=entropy&cs=tinysrgb&fit=crop&fm=jpg&q=80&w=1080",
    premiseMediaCreatedAt: "2026-01-05T08:50:00",
    premiseMediaUpdatedAt: "2026-01-05T08:50:00",
  },
  {
    premiseMediaId: 12,
    premiseMediaPremiseId: PREMISES[0].id,
    premiseMediaImageUrl:
      "https://images.unsplash.com/photo-1784646583945-a414d8590235?crop=entropy&cs=tinysrgb&fit=crop&fm=jpg&q=80&w=1080",
    premiseMediaCreatedAt: "2026-01-05T08:55:00",
    premiseMediaUpdatedAt: "2026-01-05T08:55:00",
  },
  {
    premiseMediaId: 13,
    premiseMediaPremiseId: PREMISES[0].id,
    premiseMediaImageUrl:
      "https://images.unsplash.com/photo-1760072513403-d70003481414?crop=entropy&cs=tinysrgb&fit=crop&fm=jpg&q=80&w=1080",
    premiseMediaCreatedAt: "2026-01-05T09:00:00",
    premiseMediaUpdatedAt: "2026-01-05T09:00:00",
  },
]

export const CONTRACTS: Contract[] = [
  {
    id: "CTR-2024-001",
    accountEmail: "nguyen.van.a@gmail.com",
    tenant: "Nguyễn Văn A",
    premiseName: "A01",
    deposit: 15000000,
    rentalPrice: 7500000,
    returnDate: "2025-12-31",
    status: "signed",
    createdAt: "2024-01-15",
  },
  {
    id: "CTR-2024-002",
    accountEmail: "tran.thi.b@gmail.com",
    tenant: "Trần Thị B",
    premiseName: "C01",
    deposit: 24000000,
    rentalPrice: 12000000,
    returnDate: "2025-06-30",
    status: "signed",
    createdAt: "2024-03-01",
  },
  {
    id: "CTR-2024-003",
    accountEmail: "le.van.c@company.vn",
    tenant: "Lê Văn C",
    premiseName: "B01",
    deposit: 18000000,
    rentalPrice: 9000000,
    returnDate: "2024-12-31",
    status: "pending_approval",
    createdAt: "2024-09-10",
  },
  {
    id: "CTR-2024-004",
    accountEmail: "pham.thi.d@shop.vn",
    tenant: "Phạm Thị D",
    premiseName: "A02",
    deposit: 11400000,
    rentalPrice: 5700000,
    returnDate: "2025-09-30",
    status: "pending_signature",
    createdAt: "2024-09-20",
  },
  {
    id: "CTR-2023-005",
    accountEmail: "hoang.van.e@gmail.com",
    tenant: "Hoàng Văn E",
    premiseName: "B02",
    deposit: 16500000,
    rentalPrice: 8250000,
    returnDate: "2024-03-31",
    status: "expired",
    createdAt: "2023-04-01",
  },
]

export const INVOICES: Invoice[] = [
  {
    id: "INV-2024-001",
    contractId: "CTR-2024-001",
    tenant: "Nguyễn Văn A",
    premiseName: "A01",
    totalAmount: 8200000,
    paymentDate: "2024-09-01",
    dueDate: "2024-09-30",
    status: "paid",
  },
  {
    id: "INV-2024-002",
    contractId: "CTR-2024-002",
    tenant: "Trần Thị B",
    premiseName: "C01",
    totalAmount: 13100000,
    paymentDate: "2024-09-01",
    dueDate: "2024-09-30",
    status: "unpaid",
  },
  {
    id: "INV-2024-003",
    contractId: "CTR-2024-001",
    tenant: "Nguyễn Văn A",
    premiseName: "A01",
    totalAmount: 8050000,
    paymentDate: "2024-08-01",
    dueDate: "2024-08-31",
    status: "paid",
  },
  {
    id: "INV-2024-004",
    contractId: "CTR-2024-002",
    tenant: "Trần Thị B",
    premiseName: "C01",
    totalAmount: 13400000,
    paymentDate: "2024-08-01",
    dueDate: "2024-08-31",
    status: "overdue",
  },
  {
    id: "INV-2024-005",
    contractId: "CTR-2024-003",
    tenant: "Lê Văn C",
    premiseName: "B01",
    totalAmount: 10200000,
    paymentDate: "2024-09-15",
    dueDate: "2024-10-15",
    status: "unpaid",
  },
]

export const VIOLATIONS: Violation[] = [
  {
    id: "VIO-001",
    contractId: "CTR-2024-001",
    tenant: "Nguyễn Văn A",
    content: "Sử dụng loa nhạc quá âm lượng cho phép sau 22:00",
    penaltyAmount: 500000,
    date: "2024-09-05",
    dueDate: "2024-09-12",
    resolved: false,
  },
  {
    id: "VIO-002",
    contractId: "CTR-2024-002",
    tenant: "Trần Thị B",
    content: "Để rác không đúng nơi quy định",
    penaltyAmount: 200000,
    date: "2024-08-20",
    dueDate: "2024-08-27",
    resolved: true,
  },
  {
    id: "VIO-003",
    contractId: "CTR-2024-001",
    tenant: "Nguyễn Văn A",
    content: "Tự ý sửa chữa, cải tạo mặt bằng khi chưa được phép",
    penaltyAmount: 2000000,
    date: "2024-09-15",
    dueDate: "2024-09-22",
    resolved: false,
  },
]

export const TICKETS: Ticket[] = [
  {
    id: "TKT-001",
    accountEmail: "nguyen.van.a@gmail.com",
    content: "Điều hòa không khí tầng 1 bị hỏng, cần sửa gấp",
    type: "complaint",
    resolved: false,
    createdAt: "2024-09-22",
  },
  {
    id: "TKT-002",
    accountEmail: "tran.thi.b@gmail.com",
    content: "Phản ánh về dịch vụ vệ sinh hành lang chưa tốt",
    type: "feedback",
    resolved: true,
    createdAt: "2024-09-18",
  },
  {
    id: "TKT-003",
    accountEmail: "le.van.c@company.vn",
    content: "Hàng xóm vi phạm giờ giấc, gây ồn ào",
    type: "violation",
    resolved: false,
    createdAt: "2024-09-20",
  },
  {
    id: "TKT-004",
    accountEmail: "nguyen.van.a@gmail.com",
    content: "Đánh giá 5 sao cho ban quản lý, rất chuyên nghiệp!",
    type: "review",
    resolved: true,
    createdAt: "2024-09-10",
  },
]

export const ACCOUNTS: Account[] = [
  {
    id: "ACC-001",
    email: "admin@company.vn",
    name: "Admin Hệ Thống",
    role: "SUPER_ADMIN",
    active: true,
    createdAt: "2024-01-01",
  },
  {
    id: "ACC-002",
    email: "manager@company.vn",
    name: "Trần Quản Lý",
    role: "MANAGER",
    active: true,
    createdAt: "2024-01-05",
  },
  {
    id: "ACC-003",
    email: "nguyen.van.a@gmail.com",
    name: "Nguyễn Văn A",
    role: "TENANT",
    active: true,
    createdAt: "2024-01-15",
  },
  {
    id: "ACC-004",
    email: "tran.thi.b@gmail.com",
    name: "Trần Thị B",
    role: "TENANT",
    active: true,
    createdAt: "2024-03-01",
  },
  {
    id: "ACC-005",
    email: "le.van.c@company.vn",
    name: "Lê Văn C",
    role: "TENANT",
    active: false,
    createdAt: "2024-09-10",
  },
  {
    id: "ACC-006",
    email: "staff01@company.vn",
    name: "Nhân Viên 01",
    role: "STAFF",
    active: true,
    createdAt: "2024-02-15",
  },
]

// ─── Badge helpers ─────────────────────────────────────────────────────────

export function PremiseStatusBadge({ status }: { status: PremiseStatus }) {
  const map: Record<PremiseStatus, { label: string; cls: string }> = {
    available: {
      label: "Còn trống",
      cls: "bg-emerald-100 text-emerald-700 border border-emerald-200",
    },
    rented: {
      label: "Đang thuê",
      cls: "bg-blue-100 text-blue-700 border border-blue-200",
    },
    reserved: {
      label: "Đã đặt cọc",
      cls: "bg-amber-100 text-amber-700 border border-amber-200",
    },
    maintenance: {
      label: "Bảo trì",
      cls: "bg-red-100 text-red-700 border border-red-200",
    },
  }
  const { label, cls } = map[status]
  return (
    <span
      className={`inline-flex items-center px-2 py-0.5 rounded text-xs font-medium ${cls}`}
    >
      {label}
    </span>
  )
}

export function ContractStatusBadge({ status }: { status: ContractStatus }) {
  const map: Record<ContractStatus, { label: string; cls: string }> = {
    pending_approval: {
      label: "Chờ duyệt",
      cls: "bg-amber-100 text-amber-700 border border-amber-200",
    },
    pending_signature: {
      label: "Chờ ký",
      cls: "bg-sky-100 text-sky-700 border border-sky-200",
    },
    signed: {
      label: "Đã ký",
      cls: "bg-emerald-100 text-emerald-700 border border-emerald-200",
    },
    canceled: {
      label: "Đã hủy",
      cls: "bg-gray-100 text-gray-500 border border-gray-200",
    },
    expired: {
      label: "Hết hạn",
      cls: "bg-orange-100 text-orange-700 border border-orange-200",
    },
    terminated: {
      label: "Đã chấm dứt",
      cls: "bg-red-100 text-red-700 border border-red-200",
    },
  }
  const { label, cls } = map[status]
  return (
    <span
      className={`inline-flex items-center px-2 py-0.5 rounded text-xs font-medium ${cls}`}
    >
      {label}
    </span>
  )
}

export function InvoiceStatusBadge({ status }: { status: InvoiceStatus }) {
  const map: Record<InvoiceStatus, { label: string; cls: string }> = {
    paid: {
      label: "Đã thanh toán",
      cls: "bg-emerald-100 text-emerald-700 border border-emerald-200",
    },
    unpaid: {
      label: "Chưa thanh toán",
      cls: "bg-amber-100 text-amber-700 border border-amber-200",
    },
    overdue: {
      label: "Quá hạn",
      cls: "bg-red-100 text-red-700 border border-red-200",
    },
  }
  const { label, cls } = map[status]
  return (
    <span
      className={`inline-flex items-center px-2 py-0.5 rounded text-xs font-medium ${cls}`}
    >
      {label}
    </span>
  )
}

export function TicketTypeBadge({ type }: { type: TicketType }) {
  const map: Record<TicketType, { label: string; cls: string }> = {
    review: {
      label: "Đánh giá",
      cls: "bg-violet-100 text-violet-700 border border-violet-200",
    },
    complaint: {
      label: "Khiếu nại",
      cls: "bg-red-100 text-red-700 border border-red-200",
    },
    violation: {
      label: "Tố cáo vi phạm",
      cls: "bg-orange-100 text-orange-700 border border-orange-200",
    },
    feedback: {
      label: "Phản hồi",
      cls: "bg-sky-100 text-sky-700 border border-sky-200",
    },
  }
  const { label, cls } = map[type]
  return (
    <span
      className={`inline-flex items-center px-2 py-0.5 rounded text-xs font-medium ${cls}`}
    >
      {label}
    </span>
  )
}

export const fmt = (n: number) =>
  new Intl.NumberFormat("vi-VN").format(n) + " ₫"

// ─── Modal ─────────────────────────────────────────────────────────────────

export function Modal({
  title,
  onClose,
  children,
  wide = false,
}: {
  title: string
  onClose: () => void
  children: ReactNode
  wide?: boolean
}) {
  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center"
      style={{ background: "rgba(10,14,26,0.6)" }}
    >
      <div
        className={`bg-white rounded-xl shadow-2xl w-full ${
          wide ? "max-w-6xl" : "max-w-lg"
        } mx-4 overflow-hidden`}
      >
        <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100">
          <h3
            className="text-base font-semibold text-gray-900"
            style={{ fontFamily: "DM Sans" }}
          >
            {title}
          </h3>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600 transition-colors text-xl leading-none"
          >
            &times;
          </button>
        </div>
        <div className="px-6 py-5">{children}</div>
      </div>
    </div>
  )
}

export function FormField({
  label,
  children,
}: {
  label: string
  children: ReactNode
}) {
  return (
    <div className="mb-4">
      <label className="block text-xs font-medium text-gray-500 mb-1 uppercase tracking-wide">
        {label}
      </label>
      {children}
    </div>
  )
}

export const inputCls =
  "w-full border border-gray-200 rounded-lg px-3 py-2 text-sm text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition"
export const selectCls =
  "w-full border border-gray-200 rounded-lg px-3 py-2 text-sm text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white"

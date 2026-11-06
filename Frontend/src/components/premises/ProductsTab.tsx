import type {ProductsTabProps as Props} from "./types"
import {formatDateTime} from "../../scripts/format"
import {BaseTable} from "../modules/shared"

export function ProductsTab({
    products,
    onEdit,
    onDelete,
    canEdit = true,
    canDelete = true,
}: Props) {
    return (
        <BaseTable
            rows={products}
            getRowKey={(product) => product.productId}
            emptyMessage="Chưa có sản phẩm. Hãy thêm sản phẩm vào danh mục."
            rowClassName={() => "hover:bg-gray-50/60 transition-colors"}
            columns={[
                {
                    key: "name",
                    header: "Tên sản phẩm",
                    cellClassName: "px-4 py-3 font-semibold text-gray-900",
                    render: (product) => product.productName,
                },
                {
                    key: "description",
                    header: "Mô tả",
                    cellClassName: "max-w-96 truncate px-4 py-3 text-sm text-gray-500",
                    render: (product) => product.productDescription || "—",
                },
                {
                    key: "created",
                    header: "Ngày tạo",
                    cellClassName: "whitespace-nowrap px-4 py-3 text-xs text-gray-500",
                    render: (product) => formatDateTime(product.productCreatedAt),
                },
                {
                    key: "updated",
                    header: "Ngày cập nhật",
                    cellClassName: "whitespace-nowrap px-4 py-3 text-xs text-gray-500",
                    render: (product) => formatDateTime(product.productUpdatedAt),
                },
                {
                    key: "actions",
                    header: "Thao tác",
                    headerClassName: "px-4 py-3 text-right font-medium",
                    cellClassName: "px-4 py-3 text-right",
                    render: (product) => (
                        <>
                            {canEdit && (
                                <button
                                    type="button"
                                    onClick={() => onEdit(product)}
                                    className="rounded px-2 py-1 text-xs text-blue-600 transition-colors hover:bg-blue-50 hover:text-blue-800"
                                >
                                    Sửa
                                </button>
                            )}
                            {canDelete && (
                                <button
                                    type="button"
                                    onClick={() => onDelete(product)}
                                    className="rounded px-2 py-1 text-xs text-red-600 transition-colors hover:bg-red-50 hover:text-red-800"
                                >
                                    Xóa
                                </button>
                            )}
                        </>
                    ),
                },
            ]}
        />
    )
}

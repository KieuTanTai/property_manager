import { useEffect, useState } from "react"
import {
  FormField,
  Modal,
  PremiseStatusBadge,
  inputCls,
  premiseStatusSurface,
  selectCls
} from "./shared"
import { LOCATIONS, PREMISES, BUSINESS_TYPES, PREMISE_MEDIA, WHITELIST_PRODUCTS } from "@/mocks/shared.ts"
import type { BusinessType, Location, WhitelistProduct } from "@/types/modules.ts"
import type {
  Premise,
  PremiseMedia,
  PremisesSubTab,
  PremiseStatus,
} from "../premises/types"

import { createUuidV7 } from "@/scripts/ids.ts"
import { formatDateTime } from "@/scripts/format"

import { BusinessTypesTab } from "../premises/BusinessTypesTab"
import { LocationsTab } from "../premises/LocationsTab"
import { PremisesSubTabs } from "../premises/PremisesSubTabs"
import { PremisesTab } from "../premises/PremisesTab"

// ─── Premises Module ────────────────────────────────────────────────────────

export function PremisesModule() {
  const [premises, setPremises] = useState<Premise[]>(PREMISES)
  const [premiseMedia] = useState<PremiseMedia[]>(PREMISE_MEDIA)
  const [locations, setLocations] = useState<Location[]>(LOCATIONS)
  const [businessTypes, setBusinessTypes] =
    useState<BusinessType[]>(BUSINESS_TYPES)
  const [whitelistProducts, setWhitelistProducts] =
    useState<WhitelistProduct[]>(WHITELIST_PRODUCTS)
  const [activeSubTab, setActiveSubTab] = useState<PremisesSubTab>("locations")
  const [filter, setFilter] = useState<PremiseStatus | "all">("all")
  const [modal, setModal] = useState<"add" | "view" | null>(null)
  const [premiseIsEditing, setPremiseIsEditing] = useState(false)
  const [locationModal, setLocationModal] = useState<"add" | "edit" | null>(
    null,
  )
  const [businessModal, setBusinessModal] = useState<"add" | "view" | null>(
    null,
  )
  const [businessIsEditing, setBusinessIsEditing] = useState(false)
  const [selected, setSelected] = useState<Premise | null>(null)
  const [activeMediaIndex, setActiveMediaIndex] = useState(0)
  const [isImageFullscreen, setIsImageFullscreen] = useState(false)
  const [selectedLocation, setSelectedLocation] = useState<Location | null>(
    null,
  )
  const [selectedBusinessType, setSelectedBusinessType] =
    useState<BusinessType | null>(null)
  const [locationAddress, setLocationAddress] = useState("")
  const [productName, setProductName] = useState("")
  const [productDescription, setProductDescription] = useState("")
  const [businessForm, setBusinessForm] = useState({
    name: "",
    description: "",
    active: true,
  })
  const [form, setForm] = useState({
    name: "",
    floor: "1",
    position: "1",
    area: "",
    status: "available" as PremiseStatus,
    locationId: "",
    description: "",
  })

  const filtered =
    filter === "all" ? premises : premises.filter((p) => p.status === filter)
  const selectedPremiseMedia = selected
    ? premiseMedia.filter((media) => media.premiseMediaPremiseId === selected.id)
    : []

  useEffect(() => {
    if (!isImageFullscreen) return

    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === "Escape") setIsImageFullscreen(false)
    }

    window.addEventListener("keydown", handleKeyDown)
    return () => window.removeEventListener("keydown", handleKeyDown)
  }, [isImageFullscreen])

  const openAdd = () => {
    if (!locations.length) {
      alert("Vui lòng tạo địa chỉ trước khi thêm mặt bằng.")
      setActiveSubTab("locations")
      return
    }
    setForm({
      name: "",
      floor: "1",
      position: "1",
      area: "",
      status: "available",
      locationId: locations[0].locationId,
      description: "",
    })
    setModal("add")
  }

  const openView = (p: Premise) => {
    setSelected(p)
    setActiveMediaIndex(0)
    setPremiseIsEditing(false)
    setModal("view")
  }

  const startEditingPremise = () => {
    if (!selected) return
    setForm({
      name: selected.name,
      floor: String(selected.floor),
      position: String(selected.position),
      area: selected.area,
      status: selected.status,
      locationId: selected.locationId,
      description: selected.description,
    })
    setPremiseIsEditing(true)
  }

  const handleAdd = () => {
    const location = locations.find(
      (item) => item.locationId === form.locationId,
    )
    if (!location) return
    const newP: Premise = {
      id: `PRE-${String(premises.length + 1).padStart(3, "0")}`,
      name: form.name,
      floor: Number(form.floor),
      position: Number(form.position),
      area: form.area,
      status: form.status,
      locationId: location.locationId,
      location: location.address,
      description: form.description,
    }
    setPremises([...premises, newP])
    setModal(null)
  }

  const handleEdit = () => {
    const location = locations.find(
      (item) => item.locationId === form.locationId,
    )
    if (!location || !selected) return
    const updated: Premise = {
      ...selected,
      ...form,
      location: location.address,
      floor: Number(form.floor),
      position: Number(form.position),
    }
    setPremises(premises.map((p) => (p.id === selected.id ? updated : p)))
    setSelected(updated)
    setPremiseIsEditing(false)
  }

  const handleSaveLocation = () => {
    const address = locationAddress.trim()
    if (!address) return
    const now = new Date().toISOString()
    if (locationModal === "edit" && selectedLocation) {
      setLocations(
        locations.map((location) =>
          location.locationId === selectedLocation.locationId
            ? { ...location, address, updatedAt: now }
            : location,
        ),
      )
      setPremises(
        premises.map((premise) =>
          premise.locationId === selectedLocation.locationId
            ? { ...premise, location: address }
            : premise,
        ),
      )
    } else {
      setLocations([
        ...locations,
        { locationId: createUuidV7(), address, createdAt: now, updatedAt: now },
      ])
    }
    setLocationModal(null)
    setSelectedLocation(null)
    setLocationAddress("")
  }

  const handleDeleteLocation = (location: Location) => {
    if (
      premises.some((premise) => premise.locationId === location.locationId)
    ) {
      alert("Không thể xóa địa chỉ đang được sử dụng bởi mặt bằng.")
      return
    }
    if (confirm("Xác nhận xóa địa chỉ này?"))
      setLocations(
        locations.filter((item) => item.locationId !== location.locationId),
      )
  }

  const openAddBusinessType = () => {
    setSelectedBusinessType(null)
    setBusinessForm({ name: "", description: "", active: true })
    setBusinessModal("add")
  }

  const startEditingBusinessType = () => {
    if (!selectedBusinessType) return
    setBusinessForm({
      name: selectedBusinessType.name,
      description: selectedBusinessType.description,
      active: selectedBusinessType.active,
    })
    setBusinessIsEditing(true)
  }

  const handleSaveBusinessType = () => {
    const name = businessForm.name.trim()
    if (!name) return
    const now = new Date().toISOString()
    if (businessModal === "view" && selectedBusinessType) {
      const updated = {
        ...selectedBusinessType,
        ...businessForm,
        name,
        updatedAt: now,
      }
      setBusinessTypes(
        businessTypes.map((item) =>
          item.businessTypeId === updated.businessTypeId ? updated : item,
        ),
      )
      setSelectedBusinessType(updated)
      setBusinessIsEditing(false)
    } else {
      setBusinessTypes([
        ...businessTypes,
        {
          businessTypeId: createUuidV7(),
          name,
          description: businessForm.description.trim(),
          active: businessForm.active,
          createdAt: now,
          updatedAt: now,
        },
      ])
      setBusinessModal(null)
    }
  }

  const handleAddWhitelistProduct = () => {
    const name = productName.trim()
    if (!name || !selectedBusinessType) return
    setWhitelistProducts([
      ...whitelistProducts,
      {
        productId: createUuidV7(),
        businessTypeId: selectedBusinessType.businessTypeId,
        name,
        description: productDescription.trim(),
      },
    ])
    setProductName("")
    setProductDescription("")
  }

  const handleDeleteWhitelistProduct = (productId: string) => {
    setWhitelistProducts(
      whitelistProducts.filter((product) => product.productId !== productId),
    )
  }



  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1
            className="text-xl font-bold text-gray-900"
            style={{ fontFamily: "DM Sans" }}
          >
            Quản lý mặt bằng
          </h1>
          <p className="text-sm text-gray-400 mt-0.5">
            {activeSubTab === "locations"
              ? `${locations.length} địa chỉ đã tạo`
              : activeSubTab === "business-types"
                ? `${businessTypes.length} loại mặt hàng`
                : `${premises.length} mặt bằng tổng cộng`}
          </p>
          <p className="mt-1 text-[11px] text-amber-700">
            Dữ liệu mẫu cục bộ · Premise Backend chưa có API controller
          </p>
        </div>
        <button
          onClick={
            activeSubTab === "locations"
              ? () => {
                  setLocationAddress("")
                  setSelectedLocation(null)
                  setLocationModal("add")
                }
              : activeSubTab === "business-types"
                ? openAddBusinessType
                : openAdd
          }
          className="bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium px-4 py-2 rounded-lg transition-colors flex items-center gap-2"
        >
          <span className="text-base leading-none">+</span>
          {activeSubTab === "locations"
            ? "Thêm địa chỉ"
            : activeSubTab === "business-types"
              ? "Thêm loại mặt hàng"
              : "Thêm mặt bằng"}
        </button>
      </div>

      <PremisesSubTabs
        activeTab={activeSubTab}
        locationCount={locations.length}
        businessTypeCount={businessTypes.length}
        premiseCount={premises.length}
        onChange={setActiveSubTab}
      />

      {activeSubTab === "locations" ? (
        <LocationsTab
          locations={locations}
          onEdit={(location) => {
            setSelectedLocation(location)
            setLocationAddress(location.address)
            setLocationModal("edit")
          }}
          onDelete={handleDeleteLocation}
        />
      ) : activeSubTab === "business-types" ? (
        <BusinessTypesTab
          businessTypes={businessTypes}
          onView={(businessType) => {
            setSelectedBusinessType(businessType)
            setProductName("")
            setProductDescription("")
            setBusinessIsEditing(false)
            setBusinessModal("view")
          }}
        />
      ) : (
        <PremisesTab
          premises={premises}
          premiseMedia={premiseMedia}
          filteredPremises={filtered}
          filter={filter}
          onFilterChange={setFilter}
          onView={openView}
        />
      )}

      {locationModal && (
        <Modal
          title={
            locationModal === "add" ? "Thêm địa chỉ mới" : "Chỉnh sửa địa chỉ"
          }
          onClose={() => setLocationModal(null)}
        >
          <FormField label="Địa chỉ">
            <textarea
              className={inputCls}
              rows={3}
              value={locationAddress}
              onChange={(event) => setLocationAddress(event.target.value)}
              placeholder="Nhập địa chỉ đầy đủ"
              autoFocus
            />
          </FormField>
          {locationModal === "add" && (
            <p className="text-xs text-gray-400 -mt-2 mb-4">
              Location ID UUID v7 sẽ được tạo tự động khi lưu.
            </p>
          )}
          <div className="flex justify-end gap-2">
            <button
              onClick={() => setLocationModal(null)}
              className="px-4 py-2 text-sm text-gray-600 border border-gray-200 rounded-lg hover:bg-gray-50"
            >
              Hủy
            </button>
            <button
              onClick={handleSaveLocation}
              disabled={!locationAddress.trim()}
              className="px-4 py-2 text-sm bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-40 disabled:cursor-not-allowed"
            >
              {locationModal === "add" ? "Tạo địa chỉ" : "Lưu thay đổi"}
            </button>
          </div>
        </Modal>
      )}

      {businessModal === "add" && (
        <Modal
          title="Thêm loại mặt hàng"
          onClose={() => setBusinessModal(null)}
        >
          <FormField label="Tên mặt hàng">
            <input
              className={inputCls}
              value={businessForm.name}
              onChange={(event) =>
                setBusinessForm({ ...businessForm, name: event.target.value })
              }
              placeholder="VD: Thời trang"
              autoFocus
            />
          </FormField>
          <FormField label="Ghi chú (không bắt buộc)">
            <textarea
              className={inputCls}
              rows={3}
              value={businessForm.description}
              onChange={(event) =>
                setBusinessForm({
                  ...businessForm,
                  description: event.target.value,
                })
              }
              placeholder="Mô tả nhóm mặt hàng được kinh doanh"
            />
          </FormField>
          <div className="flex items-center justify-between rounded-lg border border-gray-100 bg-gray-50 px-3 py-3 mb-5">
            <div>
              <p className="text-sm font-semibold text-gray-800">
                Trạng thái hoạt động
              </p>
              <p className="text-xs text-gray-400 mt-0.5">
                Cho phép sử dụng loại mặt hàng này
              </p>
            </div>
            <button
              type="button"
              role="switch"
              aria-checked={businessForm.active}
              onClick={() =>
                setBusinessForm({
                  ...businessForm,
                  active: !businessForm.active,
                })
              }
              className={`relative w-11 h-6 shrink-0 overflow-hidden rounded-full transition-colors ${
                businessForm.active ? "bg-blue-600" : "bg-gray-300"
              }`}
            >
              <span
                className={`absolute left-0.5 top-0.5 w-5 h-5 bg-white rounded-full shadow-sm transition-transform ${
                  businessForm.active ? "translate-x-5" : "translate-x-0"
                }`}
              />
            </button>
          </div>
          <div className="flex justify-end gap-2">
            <button
              onClick={() => setBusinessModal(null)}
              className="px-4 py-2 text-sm text-gray-600 border border-gray-200 rounded-lg hover:bg-gray-50"
            >
              Hủy
            </button>
            <button
              onClick={handleSaveBusinessType}
              disabled={!businessForm.name.trim()}
              className="px-4 py-2 text-sm bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-40 disabled:cursor-not-allowed"
            >
              Tạo loại mặt hàng
            </button>
          </div>
        </Modal>
      )}

      {businessModal === "view" && selectedBusinessType && (
        <Modal
          title={`Chi tiết loại mặt hàng: ${selectedBusinessType.name}`}
          onClose={() => setBusinessModal(null)}
          wide
        >
          <div className="grid grid-cols-2 gap-0 -mx-6 -my-5">
            <div className="p-6 border-r border-gray-100">
              <p className="text-xs font-semibold text-gray-400 uppercase tracking-wider mb-4">
                Thông tin Business Type
              </p>
              {businessIsEditing ? (
                <>
                  <FormField label="Tên mặt hàng">
                    <input
                      className={inputCls}
                      value={businessForm.name}
                      onChange={(event) =>
                        setBusinessForm({
                          ...businessForm,
                          name: event.target.value,
                        })
                      }
                      autoFocus
                    />
                  </FormField>
                  <FormField label="Ghi chú (không bắt buộc)">
                    <textarea
                      className={inputCls}
                      rows={4}
                      value={businessForm.description}
                      onChange={(event) =>
                        setBusinessForm({
                          ...businessForm,
                          description: event.target.value,
                        })
                      }
                    />
                  </FormField>
                  <div className="flex items-center justify-between rounded-lg border border-gray-100 bg-gray-50 px-3 py-3 mb-5">
                    <div>
                      <p className="text-sm font-semibold text-gray-800">
                        Trạng thái hoạt động
                      </p>
                      <p className="text-xs text-gray-400 mt-0.5">
                        Cho phép sử dụng loại mặt hàng này
                      </p>
                    </div>
                    <button
                      type="button"
                      role="switch"
                      aria-checked={businessForm.active}
                      onClick={() =>
                        setBusinessForm({
                          ...businessForm,
                          active: !businessForm.active,
                        })
                      }
                      className={`relative w-11 h-6 shrink-0 overflow-hidden rounded-full transition-colors ${
                        businessForm.active ? "bg-blue-600" : "bg-gray-300"
                      }`}
                    >
                      <span
                        className={`absolute left-0.5 top-0.5 w-5 h-5 bg-white rounded-full shadow-sm transition-transform ${
                          businessForm.active
                            ? "translate-x-5"
                            : "translate-x-0"
                        }`}
                      />
                    </button>
                  </div>
                  <div className="flex justify-end gap-2">
                    <button
                      onClick={() => setBusinessIsEditing(false)}
                      className="px-4 py-2 text-sm text-gray-600 border border-gray-200 rounded-lg hover:bg-gray-50"
                    >
                      Hủy
                    </button>
                    <button
                      onClick={handleSaveBusinessType}
                      disabled={!businessForm.name.trim()}
                      className="px-4 py-2 text-sm bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-40 disabled:cursor-not-allowed"
                    >
                      Lưu thay đổi
                    </button>
                  </div>
                </>
              ) : (
                <div className="space-y-3">
                  <div className="bg-gray-50 rounded-lg p-3">
                    <p className="text-xs text-gray-400 mb-1">
                      Business Type ID
                    </p>
                    <p className="text-xs font-mono font-medium text-gray-700 break-all">
                      {selectedBusinessType.businessTypeId}
                    </p>
                  </div>
                  <div className="bg-gray-50 rounded-lg p-3">
                    <p className="text-xs text-gray-400 mb-1">Tên mặt hàng</p>
                    <p className="text-sm font-semibold text-gray-900">
                      {selectedBusinessType.name}
                    </p>
                  </div>
                  <div className="bg-gray-50 rounded-lg p-3">
                    <p className="text-xs text-gray-400 mb-1">Mô tả</p>
                    <p className="text-sm text-gray-600 leading-relaxed">
                      {selectedBusinessType.description || "Chưa có mô tả"}
                    </p>
                  </div>
                  <div className="grid grid-cols-2 gap-3">
                    <div className="bg-gray-50 rounded-lg p-3">
                      <p className="text-xs text-gray-400 mb-1">Trạng thái</p>
                      <p
                        className={`text-sm font-semibold ${
                          selectedBusinessType.active
                            ? "text-emerald-600"
                            : "text-gray-500"
                        }`}
                      >
                        {selectedBusinessType.active ? "Active" : "Inactive"}
                      </p>
                    </div>
                    <div className="bg-gray-50 rounded-lg p-3">
                      <p className="text-xs text-gray-400 mb-1">Cập nhật</p>
                      <p className="text-xs font-medium text-gray-600">
                        {formatDateTime(selectedBusinessType.updatedAt)}
                      </p>
                    </div>
                  </div>
                  <div className="flex justify-end">
                    <button
                      onClick={startEditingBusinessType}
                      className="px-4 py-2 text-sm bg-blue-600 text-white rounded-lg hover:bg-blue-700"
                    >
                      Chỉnh sửa
                    </button>
                  </div>
                </div>
              )}
            </div>

            <div className="p-6">
              <div className="flex items-center justify-between mb-4">
                <div>
                  <p className="text-xs font-semibold text-gray-400 uppercase tracking-wider">
                    Sản phẩm được phép
                  </p>
                  <p className="text-xs text-gray-400 mt-1">
                    Whitelist sản phẩm thuộc loại mặt hàng này
                  </p>
                </div>
                <span className="text-xs font-mono font-semibold text-blue-600 bg-blue-50 px-2 py-1 rounded">
                  {
                    whitelistProducts.filter(
                      (product) =>
                        product.businessTypeId ===
                        selectedBusinessType.businessTypeId,
                    ).length
                  }{" "}
                  sản phẩm
                </span>
              </div>
              <div className="rounded-xl border border-gray-100 bg-gray-50 p-4 mb-4">
                <div className="grid grid-cols-2 gap-3">
                  <FormField label="Tên sản phẩm">
                    <input
                      className={inputCls}
                      value={productName}
                      onChange={(event) => setProductName(event.target.value)}
                      placeholder="VD: Giày, dép, quần áo..."
                    />
                  </FormField>
                  <FormField label="Mô tả sản phẩm">
                    <input
                      className={inputCls}
                      maxLength={255}
                      value={productDescription}
                      onChange={(event) =>
                        setProductDescription(event.target.value)
                      }
                      placeholder="Mô tả phạm vi sản phẩm được phép"
                    />
                  </FormField>
                </div>
                <div className="flex items-center justify-between -mt-1">
                  <span className="text-xs text-gray-400">
                    {productDescription.length}/255 ký tự
                  </span>
                  <button
                    onClick={handleAddWhitelistProduct}
                    disabled={!productName.trim()}
                    className="px-4 py-2 text-sm bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-40"
                  >
                    Thêm sản phẩm
                  </button>
                </div>
              </div>
              <div className="border border-gray-100 rounded-xl">
                {whitelistProducts
                  .filter(
                    (product) =>
                      product.businessTypeId ===
                      selectedBusinessType.businessTypeId,
                  )
                  .map((product, index) => (
                    <div
                      key={product.productId}
                      className="group relative flex items-center gap-3 px-4 py-3 border-b border-gray-50 last:border-0 hover:bg-blue-50/50 transition-colors"
                    >
                      <span className="w-6 text-xs font-mono text-gray-300">
                        {String(index + 1).padStart(2, "0")}
                      </span>
                      <span className="flex-1 min-w-0 text-sm font-medium text-gray-700">
                        {product.name}
                      </span>
                      <span className="hidden group-hover:block absolute z-20 right-12 bottom-full mb-1 w-72 rounded-lg bg-gray-900 px-3 py-2 text-xs font-normal text-white leading-relaxed shadow-xl pointer-events-none">
                        <span className="block text-xs font-semibold text-gray-300 mb-1">
                          Mô tả sản phẩm
                        </span>
                        <span className="block">
                          {product.description ||
                            "Chưa có mô tả cho sản phẩm này."}
                        </span>
                      </span>
                      <button
                        onClick={() =>
                          handleDeleteWhitelistProduct(product.productId)
                        }
                        className="text-xs text-gray-300 hover:text-red-500 transition-colors"
                      >
                        Xóa
                      </button>
                    </div>
                  ))}
                {!whitelistProducts.some(
                  (product) =>
                    product.businessTypeId ===
                    selectedBusinessType.businessTypeId,
                ) && (
                  <div className="px-4 py-10 text-center text-sm text-gray-400">
                    Chưa có sản phẩm nào trong whitelist.
                  </div>
                )}
              </div>
            </div>
          </div>
        </Modal>
      )}

      {/* Add Modal / Edit state inside detail Modal */}
      {(modal === "add" || (modal === "view" && premiseIsEditing)) && (
        <Modal
          title={
            modal === "add"
              ? "Thêm mặt bằng mới"
              : `Chỉnh sửa: ${selected?.name ?? ""}`
          }
          onClose={() => setModal(null)}
          panelClassName={premiseStatusSurface[form.status]}
        >
          <div className="grid grid-cols-2 gap-x-4">
            <FormField label="Tên mặt bằng">
              <input
                className={inputCls}
                value={form.name}
                onChange={(e) => setForm({ ...form, name: e.target.value })}
                placeholder="VD: A01"
              />
            </FormField>
            <FormField label="Diện tích">
              <input
                className={inputCls}
                value={form.area}
                onChange={(e) => setForm({ ...form, area: e.target.value })}
                placeholder="VD: 45m²"
              />
            </FormField>
            <FormField label="Tầng">
              <input
                className={inputCls}
                type="number"
                value={form.floor}
                onChange={(e) => setForm({ ...form, floor: e.target.value })}
              />
            </FormField>
            <FormField label="Vị trí">
              <input
                className={inputCls}
                type="number"
                value={form.position}
                onChange={(e) => setForm({ ...form, position: e.target.value })}
              />
            </FormField>
          </div>
          <FormField label="Địa chỉ đã tạo">
            <select
              className={selectCls}
              value={form.locationId}
              onChange={(e) => setForm({ ...form, locationId: e.target.value })}
            >
              {locations.map((location) => (
                <option key={location.locationId} value={location.locationId}>
                  {location.address}
                </option>
              ))}
            </select>
          </FormField>
          <FormField label="Trạng thái">
            <select
              className={selectCls}
              value={form.status}
              onChange={(e) =>
                setForm({ ...form, status: e.target.value as PremiseStatus })
              }
            >
              <option value="available">Còn trống</option>
              <option value="reserved">Đã đặt cọc</option>
              <option value="rented">Đang thuê</option>
              <option value="maintenance">Bảo trì</option>
            </select>
          </FormField>
          <FormField label="Mô tả">
            <textarea
              className={inputCls}
              rows={2}
              value={form.description}
              onChange={(e) =>
                setForm({ ...form, description: e.target.value })
              }
            />
          </FormField>
          <div className="flex justify-end gap-2 mt-2">
            <button
              onClick={
                modal === "add"
                  ? () => setModal(null)
                  : () => setPremiseIsEditing(false)
              }
              className="px-4 py-2 text-sm text-gray-600 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors"
            >
              Hủy
            </button>
            <button
              onClick={modal === "add" ? handleAdd : handleEdit}
              className="px-4 py-2 text-sm bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
            >
              {modal === "add" ? "Thêm mới" : "Lưu thay đổi"}
            </button>
          </div>
        </Modal>
      )}

      {/* View Modal */}
      {modal === "view" && selected && !premiseIsEditing && (
        <Modal
          title={`Chi tiết: ${selected.name}`}
          onClose={() => setModal(null)}
          panelClassName={premiseStatusSurface[selected.status]}
        >
          <div className="space-y-3">
            {premiseMedia.some(
              (media) => media.premiseMediaPremiseId === selected.id,
            ) && (
              <div className="-mx-6 -mt-5 mb-4">
                <div className="relative h-56 overflow-hidden bg-gray-900">
                  <img
                    src={
                      premiseMedia.filter(
                        (media) => media.premiseMediaPremiseId === selected.id,
                      )[activeMediaIndex].premiseMediaImageUrl
                    }
                    alt=""
                    aria-hidden="true"
                    className="absolute inset-0 w-full h-full object-cover object-center scale-110 blur-lg opacity-50"
                  />
                  <img
                    src={
                      premiseMedia.filter(
                        (media) => media.premiseMediaPremiseId === selected.id,
                      )[activeMediaIndex].premiseMediaImageUrl
                    }
                    alt={`${selected.name} — ảnh ${activeMediaIndex + 1}`}
                    role="button"
                    tabIndex={0}
                    onClick={() => setIsImageFullscreen(true)}
                    onKeyDown={(event) => {
                      if (event.key === "Enter" || event.key === " ") {
                        event.preventDefault()
                        setIsImageFullscreen(true)
                      }
                    }}
                    className="relative w-full h-full object-contain object-center cursor-zoom-in"
                  />
                  {premiseMedia.filter(
                    (media) => media.premiseMediaPremiseId === selected.id,
                  ).length > 1 && (
                    <>
                      <button
                        onClick={() => {
                          const count = premiseMedia.filter(
                            (media) =>
                              media.premiseMediaPremiseId === selected.id,
                          ).length
                          setActiveMediaIndex(
                            (activeMediaIndex - 1 + count) % count,
                          )
                        }}
                        aria-label="Ảnh trước"
                        className="absolute left-3 top-1/2 -translate-y-1/2 w-9 h-9 rounded-full bg-gray-950/70 text-white text-xl leading-none hover:bg-gray-950 transition-colors"
                      >
                        ‹
                      </button>
                      <button
                        onClick={() => {
                          const count = premiseMedia.filter(
                            (media) =>
                              media.premiseMediaPremiseId === selected.id,
                          ).length
                          setActiveMediaIndex((activeMediaIndex + 1) % count)
                        }}
                        aria-label="Ảnh tiếp theo"
                        className="absolute right-3 top-1/2 -translate-y-1/2 w-9 h-9 rounded-full bg-gray-950/70 text-white text-xl leading-none hover:bg-gray-950 transition-colors"
                      >
                        ›
                      </button>
                    </>
                  )}
                  <div className="absolute inset-x-0 bottom-0 h-12 bg-gradient-to-t from-gray-950/60 to-transparent" />
                  <span className="absolute right-3 bottom-3 rounded-full bg-gray-950/70 px-2.5 py-1 text-xs font-semibold text-white">
                    {activeMediaIndex + 1} /{" "}
                    {
                      premiseMedia.filter(
                        (media) => media.premiseMediaPremiseId === selected.id,
                      ).length
                    }
                  </span>
                </div>
                <div className="flex justify-center gap-1.5 mt-3 px-6">
                  {premiseMedia
                    .filter(
                      (media) => media.premiseMediaPremiseId === selected.id,
                    )
                    .map((media, index) => (
                      <button
                        key={media.premiseMediaId}
                        onClick={() => setActiveMediaIndex(index)}
                        aria-label={`Xem ảnh ${index + 1}`}
                        className={`h-1.5 rounded-full transition-all ${
                          index === activeMediaIndex
                            ? "w-6 bg-blue-600"
                            : "w-1.5 bg-gray-300 hover:bg-gray-400"
                        }`}
                      />
                    ))}
                </div>
              </div>
            )}
            <div className="grid grid-cols-2 gap-3">
              <div className="bg-white/70 rounded-lg p-3">
                <p className="text-xs text-gray-400 mb-1">ID</p>
                <p className="text-sm font-mono font-medium">{selected.id}</p>
              </div>
              <div className="bg-white/70 rounded-lg p-3">
                <p className="text-xs text-gray-400 mb-1">Diện tích</p>
                <p className="text-sm font-mono font-medium">{selected.area}</p>
              </div>
              <div className="bg-white/70 rounded-lg p-3">
                <p className="text-xs text-gray-400 mb-1">Tầng / Vị trí</p>
                <p className="text-sm font-medium">
                  Tầng {selected.floor} - {selected.position}
                </p>
              </div>
              <div className="bg-white/70 rounded-lg p-3">
                <p className="text-xs text-gray-400 mb-1">Trạng thái</p>
                <PremiseStatusBadge status={selected.status} />
              </div>
            </div>
            <div className="bg-white/70 rounded-lg p-3">
              <p className="text-xs text-gray-400 mb-1">Địa chỉ</p>
              <p className="text-sm">{selected.location}</p>
            </div>
            <div className="bg-white/70 rounded-lg p-3">
              <p className="text-xs text-gray-400 mb-1">Mô tả</p>
              <p className="text-sm text-gray-600">{selected.description}</p>
            </div>
            <div className="flex justify-end gap-2 pt-1">
              <button
                onClick={() => setModal(null)}
                className="px-4 py-2 text-sm text-gray-600 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors"
              >
                Đóng
              </button>
              <button
                onClick={startEditingPremise}
                className="px-4 py-2 text-sm bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
              >
                Chỉnh sửa
              </button>
            </div>
          </div>
        </Modal>
      )}

      {isImageFullscreen && selected && selectedPremiseMedia[activeMediaIndex] && (
        <div
          role="dialog"
          aria-modal="true"
          aria-label={`Ảnh ${activeMediaIndex + 1} của mặt bằng ${selected.name}`}
          className="fixed inset-0 z-[60] flex items-center justify-center bg-black/95 p-4"
          onClick={() => setIsImageFullscreen(false)}
        >
          <button
            type="button"
            aria-label="Đóng ảnh toàn màn hình"
            onClick={() => setIsImageFullscreen(false)}
            className="absolute right-5 top-4 z-10 flex h-11 w-11 items-center justify-center rounded-full bg-white/10 text-3xl leading-none text-white hover:bg-white/20"
          >
            &times;
          </button>
          <img
            src={selectedPremiseMedia[activeMediaIndex].premiseMediaImageUrl}
            alt={`${selected.name} — ảnh ${activeMediaIndex + 1}`}
            className="max-h-[calc(100vh-3rem)] max-w-[calc(100vw-3rem)] object-contain"
            onClick={(event) => event.stopPropagation()}
          />
          {selectedPremiseMedia.length > 1 && (
            <>
              <button
                type="button"
                aria-label="Ảnh trước"
                onClick={(event) => {
                  event.stopPropagation()
                  setActiveMediaIndex((index) =>
                    (index - 1 + selectedPremiseMedia.length) % selectedPremiseMedia.length,
                  )
                }}
                className="absolute left-4 top-1/2 flex h-12 w-12 -translate-y-1/2 items-center justify-center rounded-full bg-white/10 text-3xl text-white hover:bg-white/20"
              >
                ‹
              </button>
              <button
                type="button"
                aria-label="Ảnh tiếp theo"
                onClick={(event) => {
                  event.stopPropagation()
                  setActiveMediaIndex((index) => (index + 1) % selectedPremiseMedia.length)
                }}
                className="absolute right-4 top-1/2 flex h-12 w-12 -translate-y-1/2 items-center justify-center rounded-full bg-white/10 text-3xl text-white hover:bg-white/20"
              >
                ›
              </button>
              <span className="absolute bottom-4 left-1/2 -translate-x-1/2 rounded-full bg-white/10 px-3 py-1 text-sm text-white">
                {activeMediaIndex + 1} / {selectedPremiseMedia.length}
              </span>
            </>
          )}
        </div>
      )}
    </div>
  )
}

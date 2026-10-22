import { useEffect, useRef, useState } from "react"
import {
  FormField,
  Modal,
  PremiseStatusBadge,
  inputCls,
  premiseStatusSurface,
  selectCls
} from "./shared"
import type { BusinessType, Location, WhitelistProduct } from "@/types/modules.ts"
import type {
  BusinessTypeModel,
  Premise,
  PremiseMedia,
  PremisesSubTab,
  PremiseStatus,
  WhitelistProductBusinessType,
  WhitelistProductModel,
} from "../premises/types"

import { createUuidV7 } from "@/scripts/ids.ts"
import { formatDateTime } from "@/scripts/format"

import { BusinessTypesTab } from "../premises/BusinessTypesTab"
import { LocationsTab } from "../premises/LocationsTab"
import { PremisesSubTabs } from "../premises/PremisesSubTabs"
import { PremisesTab } from "../premises/PremisesTab"
import { ImageAttachments } from "./ImageAttachments"
import { mediaApi } from "@/api/media"
import { premiseApi, type PremiseRequest, type ProductRequest } from "@/api/premise"
import type { LocationModel, PremiseMediaModel, PremiseModel } from "../premises/types"
import { getImageDisplayUrl } from "@/utils/imageUrls"

function toPremiseStatus(status: string): PremiseStatus {
  switch (status.toLowerCase()) {
    case "available":
      return "available"
    case "reserved":
      return "reserved"
    case "rented":
      return "rented"
    case "maintenance":
      return "maintenance"
    default:
      throw new Error(`Trạng thái mặt bằng không được hỗ trợ: ${status}`)
  }
}

function toApiStatus(status: PremiseStatus): PremiseRequest["premiseStatus"] {
  switch (status) {
    case "available":
      return "Available"
    case "reserved":
      return "Reserved"
    case "rented":
      return "Rented"
    case "maintenance":
      return "Maintenance"
  }
}

function toPremise(model: PremiseModel, locations: Location[]): Premise {
  return {
    id: model.premiseId,
    name: model.premiseName,
    floor: model.premiseFloor,
    position: model.premisePosition,
    area: model.premiseArea,
    status: toPremiseStatus(model.premiseStatus),
    locationId: model.premiseLocationId,
    location:
      locations.find((location) => location.locationId === model.premiseLocationId)?.address ??
      model.premiseLocation?.locationAddress ??
      "",
    description: model.premiseDescription ?? "",
  }
}

function toPremiseMedia(model: PremiseMediaModel): PremiseMedia {
  return {
    premiseMediaId: model.imageId,
    premiseMediaPremiseId: model.premiseId,
    premiseMediaImageUrl: getImageDisplayUrl(model.imageUrl),
    premiseMediaCreatedAt: model.createdAt,
    premiseMediaUpdatedAt: model.updatedAt,
  }
}

function toLocation(model: LocationModel): Location {
  return {
    locationId: model.locationId,
    address: model.locationAddress,
    createdAt: model.locationCreatedAt,
    updatedAt: model.locationUpdatedAt,
  }
}

function toBusinessType(businessTypeModel: BusinessTypeModel): BusinessType {
  return {
    businessTypeId : businessTypeModel.businessTypeId,
    name: businessTypeModel.businessTypeName,
    description: businessTypeModel.businessTypeDescription,
    active: businessTypeModel.businessTypeIsActive,
    createdAt: businessTypeModel.businessTypeCreatedAt,
    updatedAt: businessTypeModel.businessTypeUpdatedAt
  }
}

function toWhitelistProduct(
  whitelistProductModel: WhitelistProductModel,
  businessTypeId: string,
): WhitelistProduct {
  return {
    productId: whitelistProductModel.whitelistProductId,
    businessTypeId,
    name: whitelistProductModel.whitelistProductName,
    description: whitelistProductModel.whitelistProductDescription
  }
}

function toPremiseRequest(form: {
  name: string
  floor: string
  position: string
  area: string
  status: PremiseStatus
  locationId: string
  description: string
}): PremiseRequest {
  const floor = Number(form.floor)
  const position = Number(form.position)
  if (!form.name.trim()) throw new Error("Tên mặt bằng là bắt buộc.")
  if (!form.area.trim()) throw new Error("Diện tích là bắt buộc.")
  if (!Number.isInteger(floor) || !Number.isInteger(position)) {
    throw new Error("Tầng và vị trí phải là số nguyên.")
  }
  if (form.area.length > 10) throw new Error("Diện tích không được vượt quá 10 ký tự.")
  if (form.name.trim().length > 50) throw new Error("Tên mặt bằng không được vượt quá 50 ký tự.")
  if (form.description.length > 100) throw new Error("Mô tả không được vượt quá 100 ký tự.")
  return {
    premiseName: form.name.trim(),
    premiseLocationId: form.locationId,
    premiseStatus: toApiStatus(form.status),
    premisePosition: position,
    premiseFloor: floor,
    premiseArea: form.area.trim(),
    premiseDescription: form.description.trim() || null,
  }
}

export function PremisesModule() {
  const [premises, setPremises] = useState<Premise[]>([])
  const [premiseMedia, setPremiseMedia] = useState<PremiseMedia[]>([])
  const [premiseImages, setPremiseImages] = useState<File[]>([])
  const [pendingImageDeletionIds, setPendingImageDeletionIds] = useState<number[]>([])
  const [isUploadingPremiseImages, setIsUploadingPremiseImages] = useState(false)
  const [isSavingPremise, setIsSavingPremise] = useState(false)
  const [isLoadingPremises, setIsLoadingPremises] = useState(true)
  const [premiseError, setPremiseError] = useState<string | null>(null)
  const [locations, setLocations] = useState<Location[]>([])
  const [businessTypes, setBusinessTypes] = useState<BusinessType[]>([])
  const [isLoadingBusinessTypes, setIsLoadingBusinessTypes] = useState(true)
  const [businessTypeError, setBusinessTypeError] = useState<string | null>(null)
  const [whitelistProducts, setWhitelistProducts] = useState<WhitelistProduct[]>([])
  const [isLoadingProducts, setIsLoadingProducts] = useState(false)
  const [isSavingProduct, setIsSavingProduct] = useState(false)
  const [productError, setProductError] = useState<string | null>(null)
  const [editingProductId, setEditingProductId] = useState<string | null>(null)
  const productLoadSequence = useRef(0)
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
  const [selectedPremise, setSelectedPremise] = useState<Premise | null>(null)
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
  const selectedPremiseMedia = selectedPremise
    ? premiseMedia.filter((media) => media.premiseMediaPremiseId === selectedPremise.id)
    : []
  const retainedPremiseMedia = selectedPremiseMedia.filter(
    (media) => !pendingImageDeletionIds.includes(media.premiseMediaId),
  )

  useEffect(() => {
    if (!isImageFullscreen) return

    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === "Escape") setIsImageFullscreen(false)
    }

    window.addEventListener("keydown", handleKeyDown)
    return () => window.removeEventListener("keydown", handleKeyDown)
  }, [isImageFullscreen])

  useEffect(() => {
    let isActive = true
    const load = async () => {
      setIsLoadingPremises(true)
      setPremiseError(null)
      console.log("start!");
      try {
        console.log("start try!");
        const [premiseModels, locationModels] = await Promise.all([
          premiseApi.getPremise(),
          premiseApi.getLocations(),
        ]);
        
        if (!isActive) return
        const activeLocations = locationModels
          .map(toLocation)
        setLocations(activeLocations)
        setPremises(premiseModels.map((premise) => toPremise(premise, activeLocations)))
        setPremiseMedia([])
        const imageModels: PremiseMediaModel[] = []
        let failedImageRequests = 0
        for (let offset = 0; offset < premiseModels.length; offset += 5) {
          const imageBatch = await Promise.allSettled(
            premiseModels
              .slice(offset, offset + 5)
              .map((premise) => premiseApi.getImages(premise.premiseId)),
          )
          for (const result of imageBatch) {
            if (result.status === "fulfilled") {
              imageModels.push(...result.value)
            } else {
              failedImageRequests += 1
            }
          }
        }
        if (isActive) {
          setPremiseMedia(imageModels.map(toPremiseMedia))
          if (failedImageRequests > 0) {
            setPremiseError(
              `Không tải được ảnh cho ${failedImageRequests} mặt bằng. Các ảnh tải thành công vẫn được hiển thị.`,
            )
          }
        }
      } 
      catch (error) {
        if (isActive) {
          setPremiseError(error instanceof Error ? error.message : "Không tải được dữ liệu mặt bằng.")
          console.log("load premise: " + premiseError);
        }
      } finally {
        if (isActive) setIsLoadingPremises(false)
      }
    }
    void load()
    return () => {
      isActive = false
    }
  }, [])

  useEffect(() => {
    let isActive = true

    const loadBusinessTypes = async () => {
      setIsLoadingBusinessTypes(true)
      setBusinessTypeError(null)
      try {
        const businessTypeModels = await premiseApi.getBusinessTypes()
        if (isActive) {
          setBusinessTypes(businessTypeModels.map(toBusinessType))
        }
      } catch (error) {
        if (isActive) {
          setBusinessTypeError(
            error instanceof Error
              ? error.message
              : "Không tải được danh sách loại hình kinh doanh.",
          )
        }
      } finally {
        if (isActive) setIsLoadingBusinessTypes(false)
      }
    }

    void loadBusinessTypes()
    return () => {
      isActive = false
    }
  }, [])

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
    setPremiseImages([])
    setPendingImageDeletionIds([])
    setModal("add")
  }

  const openView = (p: Premise) => {
    setSelectedPremise(p)
    setActiveMediaIndex(0)
    setPremiseImages([])
    setPendingImageDeletionIds([])
    setPremiseIsEditing(false)
    setModal("view")
  }

  const startEditingPremise = () => {
    if (!selectedPremise) return
    setForm({
      name: selectedPremise.name,
      floor: String(selectedPremise.floor),
      position: String(selectedPremise.position),
      area: selectedPremise.area,
      status: selectedPremise.status,
      locationId: selectedPremise.locationId,
      description: selectedPremise.description,
    })
    setPremiseImages([])
    setPendingImageDeletionIds([])
    setPremiseError(null)
    setPremiseIsEditing(true)
  }

  const handleAdd = async () => {
    const location = locations.find(
      (item) => item.locationId === form.locationId,
    )
    if (!location) {
      setPremiseError("Vui lòng chọn địa chỉ hợp lệ trước khi tạo mặt bằng.")
      return
    }
    setPremiseError(null)
    setIsSavingPremise(true)
    try {
      const created = await premiseApi.createPremise(toPremiseRequest(form))
      const newPremise = toPremise(created, locations)
      setPremises((current) => [...current, newPremise])
      setSelectedPremise(newPremise)
      setPremiseImages([])
      setModal(null)
      setIsUploadingPremiseImages(premiseImages.length > 0)
      for (const file of premiseImages) {
        const imageUrl = await mediaApi.uploadImage(file, "premise")
        const image = await premiseApi.addImage(created.premiseId, imageUrl)
        setPremiseMedia((current) => [...current, toPremiseMedia(image)])
      }
    } catch (error) {
      setPremiseError(error instanceof Error ? error.message : "Không lưu được mặt bằng.")
    } finally {
      setIsUploadingPremiseImages(false)
      setIsSavingPremise(false)
    }
  }

  const handleEdit = async () => {
    const location = locations.find(
      (item) => item.locationId === form.locationId,
    )
    if (!location || !selectedPremise) {
      setPremiseError("Vui lòng chọn địa chỉ hợp lệ trước khi cập nhật mặt bằng.")
      return
    }
    setPremiseError(null)
    setIsSavingPremise(true)
    try {
      const updatedModel = await premiseApi.updatePremise(selectedPremise.id, toPremiseRequest(form))
      const updated = toPremise(updatedModel, locations)
      setPremises((current) => current.map((premise) => premise.id === selectedPremise.id ? updated : premise))
      setSelectedPremise(updated)
      for (const imageId of pendingImageDeletionIds) {
        await premiseApi.deleteImage(selectedPremise.id, imageId)
        setPremiseMedia((current) =>
          current.filter((media) => media.premiseMediaId !== imageId),
        )
        setPendingImageDeletionIds((current) => current.filter((id) => id !== imageId))
      }
      setIsUploadingPremiseImages(premiseImages.length > 0)
      for (const file of premiseImages) {
        const imageUrl = await mediaApi.uploadImage(file, "premise")
        const image = await premiseApi.addImage(selectedPremise.id, imageUrl)
        setPremiseMedia((current) => [...current, toPremiseMedia(image)])
        setPremiseImages((current) => current.filter((item) => item !== file))
      }
      setPremiseIsEditing(false)
    } catch (error) {
      setPremiseError(error instanceof Error ? error.message : "Không cập nhật được mặt bằng.")
    } finally {
      setIsUploadingPremiseImages(false)
      setIsSavingPremise(false)
    }
  }

  const handleDeletePremise = async () => {
    if (!selectedPremise || !confirm(`Xác nhận xóa mặt bằng "${selectedPremise.name}"?`)) return
    setPremiseError(null)
    setIsSavingPremise(true)
    try {
      await premiseApi.deletePremise(selectedPremise.id)
      setPremises((current) => current.filter((premise) => premise.id !== selectedPremise.id))
      setPremiseMedia((current) => current.filter((media) => media.premiseMediaPremiseId !== selectedPremise.id))
      setModal(null)
      setSelectedPremise(null)
    } catch (error) {
      setPremiseError(error instanceof Error ? error.message : "Không xóa được mặt bằng.")
    } finally {
      setIsSavingPremise(false)
    }
  }

  const handleSaveLocation = async () => {
    const address = locationAddress.trim()
    if (!address) return
    setPremiseError(null)
    try {
      if (locationModal === "edit" && selectedLocation) {
        const model = await premiseApi.updateLocation(selectedLocation.locationId, address)
        const updatedLocation = toLocation(model)
        setLocations((current) => current.map((location) =>
          location.locationId === updatedLocation.locationId ? updatedLocation : location,
        ))
        setPremises((current) => current.map((premise) =>
          premise.locationId === updatedLocation.locationId
            ? { ...premise, location: updatedLocation.address }
            : premise,
        ))
      } else {
        const model = await premiseApi.createLocation(address)
        setLocations((current) => [...current, toLocation(model)])
      }
      setLocationModal(null)
      setSelectedLocation(null)
      setLocationAddress("")
    } catch (error) {
      setPremiseError(error instanceof Error ? error.message : "Không lưu được địa chỉ.")
    }
  }

  const handleDeleteLocation = (location: Location) => {
    if (
      premises.some((premise) => premise.locationId === location.locationId)
    ) {
      alert("Không thể xóa địa chỉ đang được sử dụng bởi mặt bằng.")
      return
    }
    if (!confirm("Xác nhận xóa địa chỉ này?")) return
    void premiseApi.deleteLocation(location.locationId).then(() => {
      setLocations((current) => current.filter((item) => item.locationId !== location.locationId))
    }).catch((error: unknown) => {
      setPremiseError(error instanceof Error ? error.message : "Không xóa được địa chỉ.")
    })
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

  const handleAddWhitelistProduct = async () => {
    const name = productName.trim()
    if (!name || !selectedBusinessType) return
    if (name.length > 100 || productDescription.trim().length > 255) {
      setProductError("Tên sản phẩm tối đa 100 ký tự; mô tả tối đa 255 ký tự.")
      return
    }

    const body: ProductRequest = {
      productName: name,
      productDescription: productDescription.trim() || null,
    }
    setIsSavingProduct(true)
    setProductError(null)
    try {
      if (editingProductId) {
        const updated = await premiseApi.updateProduct(editingProductId, body)
        setWhitelistProducts((current) =>
          current.map((product) =>
            product.productId === editingProductId
              ? toWhitelistProduct(updated, selectedBusinessType.businessTypeId)
              : product,
          ),
        )
      } else {
        const created = await premiseApi.createProductForBusinessType({
          ...body,
          businessTypeId: selectedBusinessType.businessTypeId,
        })
        setWhitelistProducts((current) => [
          ...current,
          toWhitelistProduct(created, selectedBusinessType.businessTypeId),
        ])
      }
      setProductName("")
      setProductDescription("")
      setEditingProductId(null)
    } catch (error) {
      setProductError(error instanceof Error ? error.message : "Không lưu được sản phẩm.")
    } finally {
      setIsSavingProduct(false)
    }
  }

  const handleEditWhitelistProduct = (product: WhitelistProduct) => {
    setEditingProductId(product.productId)
    setProductName(product.name)
    setProductDescription(product.description ?? "")
    setProductError(null)
  }

  const handleDeleteWhitelistProduct = async (productId: string) => {
    if (!selectedBusinessType) return
    setProductError(null)
    try {
      await premiseApi.removeProductBusinessType(
        productId,
        selectedBusinessType.businessTypeId,
      )
      setWhitelistProducts((current) =>
        current.filter((product) => product.productId !== productId),
      )
      if (editingProductId === productId) {
        setEditingProductId(null)
        setProductName("")
        setProductDescription("")
      }
    } catch (error) {
      setProductError(error instanceof Error ? error.message : "Không xóa được liên kết sản phẩm.")
    }
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

      {premiseError && (
        <div role="alert" className="mb-4 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {premiseError}
          <button
            type="button"
            onClick={() => setPremiseError(null)}
            className="ml-3 font-semibold underline"
          >
            Đóng
          </button>
        </div>
      )}
      {businessTypeError && (
        <div role="alert" className="mb-4 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          Không thể tải loại hình kinh doanh từ database: {businessTypeError}
          <button
            type="button"
            onClick={() => setBusinessTypeError(null)}
            className="ml-3 font-semibold underline"
          >
            Đóng
          </button>
        </div>
      )}

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
        isLoadingBusinessTypes ? (
          <div className="rounded-xl border border-gray-100 bg-white px-6 py-12 text-center text-sm text-gray-500">
            Đang tải loại hình kinh doanh...
          </div>
        ) : businessTypes.length === 0 && !businessTypeError ? (
          <div className="rounded-xl border border-gray-100 bg-white px-6 py-12 text-center text-sm text-gray-500">
            Chưa có loại hình kinh doanh trong database.
          </div>
        ) : (
          <BusinessTypesTab
            businessTypes={businessTypes}
            onView={(businessType) => {
            const loadSequence = ++productLoadSequence.current
            setSelectedBusinessType(businessType)
            setProductName("")
            setProductDescription("")
            setEditingProductId(null)
            setProductError(null)
            setIsLoadingProducts(true)
            setWhitelistProducts([])
            setBusinessIsEditing(false)
            setBusinessModal("view")
            void premiseApi.getProductsByBusinessType(businessType.businessTypeId)
              .then((products) => {
                if (loadSequence === productLoadSequence.current) {
                  setWhitelistProducts(products.map((product) =>
                    toWhitelistProduct(product, businessType.businessTypeId),
                  ))
                }
              })
              .catch((error: unknown) => {
                if (loadSequence === productLoadSequence.current) {
                  setProductError(
                    error instanceof Error ? error.message : "Không tải được sản phẩm.",
                  )
                }
              })
              .finally(() => {
                if (loadSequence === productLoadSequence.current) {
                  setIsLoadingProducts(false)
                }
              })
            }}
          />
        )
      ) : (
        isLoadingPremises ? (
          <div className="rounded-xl border border-gray-100 bg-white px-6 py-12 text-center text-sm text-gray-500">
            Đang tải mặt bằng...
          </div>
        ) : premises.length === 0 ? (
          <div className="rounded-xl border border-gray-100 bg-white px-6 py-12 text-center">
            <p className="text-sm text-gray-500">Chưa có mặt bằng nào.</p>
            <button
              type="button"
              onClick={openAdd}
              disabled={!locations.length}
              className="mt-4 rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-40"
            >
              Thêm mặt bằng
            </button>
          </div>
        ) : (
          <PremisesTab
            premises={premises}
            premiseMedia={premiseMedia}
            filteredPremises={filtered}
            filter={filter}
            onFilterChange={setFilter}
            onView={openView}
          />
        )
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
              onClick={() => void handleSaveLocation()}
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
                      maxLength={100}
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
                    disabled={!productName.trim() || isSavingProduct}
                    className="px-4 py-2 text-sm bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-40"
                  >
                    {isSavingProduct
                      ? "Đang lưu..."
                      : editingProductId
                        ? "Lưu sản phẩm"
                        : "Thêm sản phẩm"}
                  </button>
                </div>
                {editingProductId && (
                  <button
                    type="button"
                    onClick={() => {
                      setEditingProductId(null)
                      setProductName("")
                      setProductDescription("")
                    }}
                    className="mt-2 text-xs text-gray-500 hover:text-gray-700"
                  >
                    Hủy chỉnh sửa
                  </button>
                )}
              </div>
              {productError && (
                <p role="alert" className="mb-3 rounded-lg bg-red-50 px-3 py-2 text-sm text-red-700">
                  {productError}
                </p>
              )}
              <div className="border border-gray-100 rounded-xl">
                {isLoadingProducts ? (
                  <div className="px-4 py-10 text-center text-sm text-gray-400">
                    Đang tải sản phẩm...
                  </div>
                ) : whitelistProducts
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
                        type="button"
                        onClick={() => handleEditWhitelistProduct(product)}
                        className="text-xs text-blue-600 hover:text-blue-800 transition-colors"
                      >
                        Sửa
                      </button>
                      <button
                        type="button"
                        onClick={() =>
                          void handleDeleteWhitelistProduct(product.productId)
                        }
                        className="text-xs text-gray-300 hover:text-red-500 transition-colors"
                      >
                        Xóa
                      </button>
                    </div>
                  ))}
                {!isLoadingProducts && !whitelistProducts.some(
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
              : `Chỉnh sửa: ${selectedPremise?.name ?? ""}`
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
          {modal === "add" && (
            <FormField label="Ảnh mặt bằng">
              <ImageAttachments files={premiseImages} onChange={setPremiseImages} />
            </FormField>
          )}
          {modal === "view" && selectedPremise && (
            <FormField label="Ảnh mặt bằng">
              {selectedPremiseMedia.length > 0 && (
                <div className="mb-3 flex flex-wrap gap-3">
                  {selectedPremiseMedia.map((media) => {
                    const isMarkedForDeletion = pendingImageDeletionIds.includes(
                      media.premiseMediaId,
                    )
                    return (
                      <div
                        key={media.premiseMediaId}
                        className={`relative h-20 w-24 overflow-hidden rounded-lg border ${
                          isMarkedForDeletion
                            ? "border-red-300 opacity-40"
                            : "border-slate-200"
                        }`}
                      >
                        <img
                          src={media.premiseMediaImageUrl}
                          alt="Ảnh mặt bằng"
                          className="h-full w-full object-cover"
                        />
                        <button
                          type="button"
                          onClick={() => {
                            setPendingImageDeletionIds((current) =>
                              isMarkedForDeletion
                                ? current.filter((id) => id !== media.premiseMediaId)
                                : [...current, media.premiseMediaId],
                            )
                            setPremiseError(null)
                          }}
                          className="absolute right-1 top-1 rounded bg-black/70 px-2 py-1 text-xs text-white"
                        >
                          {isMarkedForDeletion ? "Hoàn tác" : "Xóa"}
                        </button>
                      </div>
                    )
                  })}
                </div>
              )}
              <ImageAttachments
                files={premiseImages}
                onChange={setPremiseImages}
                maxCount={Math.max(0, 5 - retainedPremiseMedia.length)}
              />
              <p className="mt-2 text-xs text-slate-500">
                Thay đổi ảnh sẽ được lưu cùng lúc khi bạn chọn “Lưu thay đổi”.
              </p>
            </FormField>
          )}
          {premiseError && (
            <p role="alert" className="mb-3 rounded-lg bg-red-50 px-3 py-2 text-sm text-red-700">
              {premiseError}
            </p>
          )}
          <div className="flex justify-end gap-2 mt-2">
            <button
              onClick={() => {
                if (modal === "add") {
                  setModal(null)
                  setPremiseImages([])
                } else {
                  setPremiseIsEditing(false)
                  setPremiseImages([])
                  setPendingImageDeletionIds([])
                  setPremiseError(null)
                }
              }}
              className="px-4 py-2 text-sm text-gray-600 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors"
            >
              Hủy
            </button>
            <button
              onClick={modal === "add" ? () => void handleAdd() : () => void handleEdit()}
              disabled={isSavingPremise}
              className="px-4 py-2 text-sm bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
            >
              {isSavingPremise
                ? isUploadingPremiseImages ? "Đang lưu ảnh..." : "Đang lưu..."
                : modal === "add" ? "Thêm mới" : "Lưu thay đổi"}
            </button>
          </div>
        </Modal>
      )}

      {/* View Modal */}
      {modal === "view" && selectedPremise && !premiseIsEditing && (
        <Modal
          title={`Chi tiết: ${selectedPremise.name}`}
          onClose={() => setModal(null)}
          panelClassName={premiseStatusSurface[selectedPremise.status]}
        >
          <div className="space-y-3">
            {premiseMedia.some(
              (media) => media.premiseMediaPremiseId === selectedPremise.id,
            ) && (
              <div className="-mx-6 -mt-5 mb-4">
                <div className="relative h-56 overflow-hidden bg-gray-900">
                  <img
                    src={
                      premiseMedia.filter(
                        (media) => media.premiseMediaPremiseId === selectedPremise.id,
                      )[activeMediaIndex].premiseMediaImageUrl
                    }
                    alt=""
                    aria-hidden="true"
                    className="absolute inset-0 w-full h-full object-cover object-center scale-110 blur-lg opacity-50"
                  />
                  <img
                    src={
                      premiseMedia.filter(
                        (media) => media.premiseMediaPremiseId === selectedPremise.id,
                      )[activeMediaIndex].premiseMediaImageUrl
                    }
                    alt={`${selectedPremise.name} — ảnh ${activeMediaIndex + 1}`}
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
                    (media) => media.premiseMediaPremiseId === selectedPremise.id,
                  ).length > 1 && (
                    <>
                      <button
                        onClick={() => {
                          const count = premiseMedia.filter(
                            (media) =>
                              media.premiseMediaPremiseId === selectedPremise.id,
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
                              media.premiseMediaPremiseId === selectedPremise.id,
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
                        (media) => media.premiseMediaPremiseId === selectedPremise.id,
                      ).length
                    }
                  </span>
                </div>
                <div className="flex justify-center gap-1.5 mt-3 px-6">
                  {premiseMedia
                    .filter(
                      (media) => media.premiseMediaPremiseId === selectedPremise.id,
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
                <p className="text-sm font-mono font-medium">{selectedPremise.id}</p>
              </div>
              <div className="bg-white/70 rounded-lg p-3">
                <p className="text-xs text-gray-400 mb-1">Diện tích</p>
                <p className="text-sm font-mono font-medium">{selectedPremise.area}</p>
              </div>
              <div className="bg-white/70 rounded-lg p-3">
                <p className="text-xs text-gray-400 mb-1">Tầng / Vị trí</p>
                <p className="text-sm font-medium">
                  Tầng {selectedPremise.floor} - {selectedPremise.position}
                </p>
              </div>
              <div className="bg-white/70 rounded-lg p-3">
                <p className="text-xs text-gray-400 mb-1">Trạng thái</p>
                <PremiseStatusBadge status={selectedPremise.status} />
              </div>
            </div>
            <div className="bg-white/70 rounded-lg p-3">
              <p className="text-xs text-gray-400 mb-1">Địa chỉ</p>
              <p className="text-sm">{selectedPremise.location}</p>
            </div>
            <div className="bg-white/70 rounded-lg p-3">
              <p className="text-xs text-gray-400 mb-1">Mô tả</p>
              <p className="text-sm text-gray-600">{selectedPremise.description}</p>
            </div>
            <div className="flex justify-end gap-2 pt-1">
              <button
                type="button"
                onClick={() => void handleDeletePremise()}
                disabled={isSavingPremise}
                className="mr-auto px-4 py-2 text-sm text-red-600 border border-red-200 rounded-lg hover:bg-red-50 disabled:opacity-50"
              >
                Xóa mặt bằng
              </button>
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

      {isImageFullscreen && selectedPremise && selectedPremiseMedia[activeMediaIndex] && (
        <div
          role="dialog"
          aria-modal="true"
          aria-label={`Ảnh ${activeMediaIndex + 1} của mặt bằng ${selectedPremise.name}`}
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
            alt={`${selectedPremise.name} — ảnh ${activeMediaIndex + 1}`}
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
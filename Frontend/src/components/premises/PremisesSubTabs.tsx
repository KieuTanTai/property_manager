import type { PremisesSubTabsProps as Props } from "./types"

export function PremisesSubTabs({
  activeTab,
  locationCount,
  businessTypeCount,
  premiseCount,
  onChange,
}: Props) {
  const tabs = [
    { id: "locations" as const, label: "Địa chỉ", count: locationCount },
    {
      id: "business-types" as const,
      label: "Quản lý mặt hàng",
      count: businessTypeCount,
    },
    { id: "premises" as const, label: "Mặt bằng", count: premiseCount },
  ]

  return (
    <div className="border-b border-gray-200 mb-5">
      <div className="flex gap-6">
        {tabs.map((tab) => (
          <button
            key={tab.id}
            onClick={() => onChange(tab.id)}
            className={`pb-3 text-sm font-semibold border-b-2 transition-colors ${
              activeTab === tab.id
                ? "border-blue-600 text-blue-600"
                : "border-transparent text-gray-400 hover:text-gray-700"
            }`}
          >
            {tab.label}{" "}
            <span className="ml-1 text-xs font-medium">{tab.count}</span>
          </button>
        ))}
      </div>
    </div>
  )
}

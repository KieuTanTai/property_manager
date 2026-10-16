import { useEffect, useState } from "react"
import type { ContactInfoModel, ContactInfoRequest } from "@/api/contracts.ts"
import { contractApi } from "@/api/contracts.ts"
import { FormField, Modal, inputCls } from "./shared"

const emptyForm: ContactInfoRequest = {
  contactInfoEmail: "",
  contactInfoPhone: "",
  contactInfoAddress: "",
  contactSocialLink: null,
}

export function ContactsModule() {
  const [contacts, setContacts] = useState<ContactInfoModel[]>([])
  const [selected, setSelected] = useState<ContactInfoModel | null>(null)
  const [form, setForm] = useState<ContactInfoRequest>(emptyForm)
  const [isEditing, setIsEditing] = useState(false)
  const [isAdding, setIsAdding] = useState(false)
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let active = true
    contractApi.contactInfos()
      .then((items) => {
        if (active) setContacts(items)
      })
      .catch((reason: unknown) => {
        if (active) setError(reason instanceof Error ? reason.message : "Không thể tải thông tin liên hệ.")
      })
      .finally(() => {
        if (active) setLoading(false)
      })
    return () => { active = false }
  }, [])

  const openContact = (contact: ContactInfoModel) => {
    setSelected(contact)
    setForm({
      contactInfoEmail: contact.contactInfoEmail,
      contactInfoPhone: contact.contactInfoPhone,
      contactInfoAddress: contact.contactInfoAddress,
      contactSocialLink: contact.contactSocialLink,
    })
    setIsAdding(false)
    setIsEditing(false)
  }

  const closeModal = () => {
    setSelected(null)
    setIsEditing(false)
    setIsAdding(false)
    setForm(emptyForm)
  }

  const save = async (event: React.SubmitEvent<HTMLFormElement>) => {
    event.preventDefault()
    setSaving(true)
    setError(null)
    const body = { ...form, contactSocialLink: form.contactSocialLink?.trim() || null }
    try {
      if (isAdding) {
        const created = await contractApi.createContactInfo(body)
        setContacts((current) => [...current, created])
        closeModal()
      } else if (selected) {
        const updated = await contractApi.updateContactInfo(selected.contactInfoId, body)
        setContacts((current) => current.map((item) => item.contactInfoId === updated.contactInfoId ? updated : item))
        closeModal()
      }
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : "Không thể lưu thông tin liên hệ.")
    } finally {
      setSaving(false)
    }
  }

  const remove = async (contact: ContactInfoModel) => {
    if (!window.confirm(`Xóa thông tin liên hệ ${contact.contactInfoEmail}?`)) return
    setError(null)
    try {
      await contractApi.deleteContactInfo(contact.contactInfoId)
      setContacts((current) => current.filter((item) => item.contactInfoId !== contact.contactInfoId))
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : "Không thể xóa thông tin liên hệ.")
    }
  }

  const setField = (field: keyof ContactInfoRequest, value: string) =>
    setForm((current) => ({ ...current, [field]: value }))

  const formDisabled = !isEditing && !isAdding

  return (
    <div>
      <div className="mb-6 flex items-center justify-between">
        <div>
          <h1 className="text-xl font-bold text-gray-900" style={{ fontFamily: "DM Sans" }}>Thông tin liên hệ</h1>
          <p className="mt-0.5 text-sm text-gray-400">{contacts.length} đầu mối liên hệ của chủ quản</p>
        </div>
        <button onClick={() => { setForm(emptyForm); setIsAdding(true); setIsEditing(true) }} className="flex items-center gap-2 rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-blue-700">
          <span className="text-base leading-none">+</span> Thêm liên hệ
        </button>
      </div>

      {error && <p role="alert" className="mb-4 rounded-lg border border-red-100 bg-red-50 px-4 py-3 text-sm text-red-700">{error}</p>}
      <div className="overflow-x-auto rounded-xl border border-gray-100 bg-white">
        <table className="w-full min-w-[760px] text-sm">
          <thead><tr className="border-b border-gray-100 text-left text-xs uppercase tracking-wide text-gray-400">
            <th className="px-4 py-3 font-medium">Email</th><th className="px-4 py-3 font-medium">Điện thoại</th><th className="px-4 py-3 font-medium">Địa chỉ</th><th className="px-4 py-3 font-medium">Mạng xã hội</th><th className="px-4 py-3 font-medium">Thao tác</th>
          </tr></thead>
          <tbody className="divide-y divide-gray-50">
            {loading ? <tr><td colSpan={5} className="px-4 py-10 text-center text-gray-400">Đang tải liên hệ...</td></tr> : contacts.length === 0 ? <tr><td colSpan={5} className="px-4 py-10 text-center text-gray-400">Chưa có thông tin liên hệ.</td></tr> : contacts.map((contact) => (
              <tr key={contact.contactInfoId} className="hover:bg-blue-50/40">
                <td className="px-4 py-3 font-medium text-gray-800">{contact.contactInfoEmail}</td><td className="px-4 py-3 text-gray-600">{contact.contactInfoPhone}</td><td className="max-w-sm px-4 py-3 text-gray-600">{contact.contactInfoAddress}</td>
                <td className="px-4 py-3">{contact.contactSocialLink ? <span className="group relative inline-flex"><a href={contact.contactSocialLink} target="_blank" rel="noreferrer" title={contact.contactSocialLink} className="inline-flex items-center gap-1 text-blue-600 hover:underline">Mở liên kết <span className="material-symbols-outlined text-sm" aria-hidden="true">open_in_new</span></a><span role="tooltip" className="pointer-events-none absolute left-0 top-full z-20 mt-2 hidden max-w-80 break-all rounded-lg border border-gray-200 bg-white px-3 py-2 text-xs text-gray-700 shadow-lg group-hover:block group-focus-within:block">{contact.contactSocialLink}</span></span> : <span className="text-gray-400">—</span>}</td>
                <td className="whitespace-nowrap px-4 py-3"><button onClick={() => openContact(contact)} className="mr-3 font-medium text-blue-600 hover:text-blue-800">Xem</button><button onClick={() => void remove(contact)} className="font-medium text-red-500 hover:text-red-700">Xóa</button></td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {(selected || isAdding) && <Modal title={isAdding ? "Thêm thông tin liên hệ" : "Thông tin liên hệ"} onClose={closeModal}>
        <form onSubmit={(event) => void save(event)}>
          <div className="grid gap-4">
            <FormField label="Email"><input required type="email" maxLength={255} disabled={formDisabled} value={form.contactInfoEmail} onChange={(event) => setField("contactInfoEmail", event.target.value)} className={inputCls} /></FormField>
            <FormField label="Số điện thoại"><input required maxLength={20} disabled={formDisabled} value={form.contactInfoPhone} onChange={(event) => setField("contactInfoPhone", event.target.value)} className={inputCls} /></FormField>
            <FormField label="Địa chỉ"><textarea required maxLength={255} disabled={formDisabled} rows={3} value={form.contactInfoAddress} onChange={(event) => setField("contactInfoAddress", event.target.value)} className={inputCls} /></FormField>
            <FormField label="Liên kết mạng xã hội"><input type="url" maxLength={255} disabled={formDisabled} value={form.contactSocialLink ?? ""} onChange={(event) => setField("contactSocialLink", event.target.value)} placeholder="https://..." className={inputCls} /></FormField>
          </div>
          <div className="mt-6 flex justify-end gap-2 border-t border-gray-100 pt-4">
            {isEditing || isAdding ? <>
              <button type="button" onClick={() => isAdding ? closeModal() : openContact(selected!)} className="rounded-lg border border-gray-200 px-4 py-2 text-sm text-gray-600">Hủy</button>
              <button disabled={saving} type="submit" className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white disabled:opacity-60">{saving ? "Đang lưu..." : "Lưu thay đổi"}</button>
            </> : <button type="button" onClick={() => setIsEditing(true)} className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700">Chỉnh sửa</button>}
          </div>
        </form>
      </Modal>}
    </div>
  )
}

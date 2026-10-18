import type {SubmitEvent} from "react"
import {useEffect, useState} from "react"
import {
    type NotificationModel,
    type NotificationType,
    ticketApi,
    type TicketModel,
    type TicketType
} from "../../api/ticketAndNotification"
import {FormField, inputCls, Modal, selectCls} from "./shared"
import {ImageAttachments} from "./ImageAttachments"
import {MOCK_NOTIFICATIONS, MOCK_TICKETS} from "../../mocks/tickets"

type View = "tickets" | "notifications"
const button = "rounded-lg px-3 py-2 text-sm font-medium transition-colors disabled:opacity-50"
const ticketTypes: TicketType[] = ["Review", "Complaint", "Violation", "Feedback"]
const notificationTypes: NotificationType[] = ["Violation", "Ticket", "Other"]
const ticketLabel: Record<TicketType, string> = {
    Review: "Đánh giá",
    Complaint: "Khiếu nại",
    Violation: "Tố cáo",
    Feedback: "Phản hồi"
}
const notificationLabel: Record<NotificationType, string> = {Violation: "Vi phạm", Ticket: "Yêu cầu", Other: "Khác"}

export function TicketsModule({accountId, customerMode = false}: { accountId?: string; customerMode?: boolean }) {
    const [view, setView] = useState<View>(customerMode ? "notifications" : "tickets")
    const [tickets, setTickets] = useState<TicketModel[]>(MOCK_TICKETS)
    const [notifications, setNotifications] = useState<NotificationModel[]>(() => customerMode && accountId
        ? MOCK_NOTIFICATIONS.map((item) => ({
            ...item,
            notificationRecipients: item.notificationRecipients.map((recipient) => ({...recipient, accountId})),
        }))
        : MOCK_NOTIFICATIONS)
    const [filter, setFilter] = useState<"all" | "open" | "resolved">("all")
    const [modal, setModal] = useState<View | null>(null)
    const [ticketForm, setTicketForm] = useState({accountId: "", content: "", type: "Feedback" as TicketType})
    const [ticketImages, setTicketImages] = useState<File[]>([])
    const [imageToView, setImageToView] = useState<{ urls: string[]; index: number } | null>(null)
    const [notificationForm, setNotificationForm] = useState({
        senderAccountId: "",
        recipientAccountId: "",
        content: "",
        type: "Other" as NotificationType
    })
    const loading = false
    const [saving, setSaving] = useState(false)
    const [error, setError] = useState<string | null>(null)
    const [notice, setNotice] = useState<string | null>(null)
    const loadingNotifications = false

    useEffect(() => {
        let isCurrent = true
        void ticketApi.tickets().then((items) => {
            if (isCurrent) setTickets(items)
        }).catch(() => {
            // Keep the local sample tickets available when the API is not running.
        })
        return () => {
            isCurrent = false
        }
    }, [])

    useEffect(() => {
        if (!imageToView) return
        const closeOnEscape = (event: KeyboardEvent) => {
            if (event.key === "Escape") setImageToView(null)
            if (event.key === "ArrowLeft") setImageToView((current) => current ? {
                ...current,
                index: (current.index - 1 + current.urls.length) % current.urls.length
            } : null)
            if (event.key === "ArrowRight") setImageToView((current) => current ? {
                ...current,
                index: (current.index + 1) % current.urls.length
            } : null)
        }
        window.addEventListener("keydown", closeOnEscape)
        return () => window.removeEventListener("keydown", closeOnEscape)
    }, [imageToView])

    const run = async (operation: () => Promise<unknown>, message: string) => {
        setSaving(true)
        setError(null)
        setNotice(null)
        try {
            await operation()
            setNotice(message)
            return true
        } catch (cause) {
            setError(cause instanceof Error ? cause.message : "Thao tác thất bại.")
            return false
        } finally {
            setSaving(false)
        }
    }

    const createTicket = async (event: SubmitEvent<HTMLFormElement>) => {
        event.preventDefault()
        const selectedImages = ticketImages
        const done = await run(async () => {
            if (selectedImages.length > 0) {
                const imageUrls: string[] = []
                for (const file of selectedImages) imageUrls.push(await ticketApi.uploadImage(file))
                const created = await ticketApi.createTicket({
                    accountId: ticketForm.accountId,
                    content: ticketForm.content,
                    type: ticketForm.type
                })
                await ticketApi.addTicketMedias(created.ticketId, imageUrls)
                const ticketMedias = await ticketApi.ticketMedias(created.ticketId)
                setTickets((current) => [{...created, ticketMedias}, ...current])
            } else {
                const now = new Date().toISOString()
                const sample: TicketModel = {
                    ticketId: crypto.randomUUID(),
                    ticketAccountId: ticketForm.accountId,
                    ticketContent: ticketForm.content,
                    ticketType: ticketForm.type,
                    ticketIsResolved: false,
                    ticketCreatedAt: now,
                    ticketUpdatedAt: now,
                    ticketMedias: []
                }
                setTickets((current) => [sample, ...current])
            }
        }, "Đã tạo yêu cầu.")
        if (done) {
            setTicketImages([]);
            setModal(null)
        }
    }
    const createNotification = async (event: SubmitEvent<HTMLFormElement>) => {
        event.preventDefault()
        setSaving(true)
        setError(null)
        setNotice(null)
        try {
            const now = new Date().toISOString()
            const id = crypto.randomUUID()
            const notification: NotificationModel = {
                notificationId: id,
                notificationSenderAccountId: notificationForm.senderAccountId,
                notificationType: notificationForm.type,
                notificationContent: notificationForm.content,
                notificationIsRead: false,
                notificationCreatedAt: now,
                notificationUpdatedAt: now,
                notificationRecipients: [{notificationId: id, accountId: notificationForm.recipientAccountId}]
            }
            setNotifications((current) => [notification, ...current])
            setNotice("Đã tạo thông báo và thêm người nhận.")
            setModal(null)
        } catch (cause) {
            setError(cause instanceof Error ? cause.message : "Không tạo được thông báo.")
        } finally {
            setSaving(false)
        }
    }
    const markRead = (notification: NotificationModel) => void run(async () => {
        setNotifications((current) => current.map((item) => item.notificationId === notification.notificationId ? {
            ...item,
            notificationIsRead: true,
            notificationUpdatedAt: new Date().toISOString()
        } : item))
    }, "Đã đánh dấu thông báo đã đọc.")
    const filteredTickets = tickets.filter((ticket) => filter === "all" || ticket.ticketIsResolved === (filter === "resolved"))
    const visibleNotifications = customerMode
        ? notifications.filter((item) => item.notificationRecipients.some((recipient) => recipient.accountId === accountId))
        : notifications

    return <div>
        <div className="mb-5 flex flex-wrap items-center justify-between gap-3">
            <div><h1
                className="text-xl font-bold text-slate-900">{customerMode ? "Thông báo của tôi" : "Yêu cầu &amp; Thông báo"}</h1>
                <p className="mt-1 text-sm text-slate-500">{customerMode ? `${visibleNotifications.filter((item) => !item.notificationIsRead).length} thông báo chưa đọc` : `${tickets.filter((ticket) => !ticket.ticketIsResolved).length} ticket chưa xử lý · ${notifications.filter((item) => !item.notificationIsRead).length} thông báo chưa đọc`}</p>
            </div>
            {!customerMode && <div className="flex gap-2">
                <span className="rounded-md bg-amber-50 px-3 py-2 text-xs text-amber-800">Dữ liệu mẫu</span>
                <button className={`${button} bg-blue-600 text-white hover:bg-blue-700`} onClick={() => {
                    setTicketForm({accountId: "", content: "", type: "Feedback"});
                    setTicketImages([]);
                    setModal("tickets")
                }}>Tạo ticket
                </button>
                <button className={`${button} bg-slate-900 text-white hover:bg-slate-700`} onClick={() => {
                    setNotificationForm({senderAccountId: "", recipientAccountId: "", content: "", type: "Other"});
                    setModal("notifications")
                }}>Tạo thông báo
                </button>
            </div>}
        </div>

        {error && <div role="alert"
                       className="mb-4 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">{error}</div>}
        {notice && <div role="status"
                        className="mb-4 rounded-lg border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700">{notice}</div>}

        <div className="mb-5 flex w-fit gap-1 rounded-lg bg-slate-100 p-1">
            {(customerMode ? ["notifications"] as const : ["tickets", "notifications"] as const).map((item) => <button
                key={item}
                className={`rounded-md px-3 py-1.5 text-xs font-medium ${view === item ? "bg-white text-slate-900 shadow-sm" : "text-slate-500"}`}
                onClick={() => setView(item)}>{item === "tickets" ? `Tickets (${tickets.length})` : `Thông báo (${visibleNotifications.length})`}</button>)}
        </div>

        {view === "tickets" ? <>
            <div
                className="mb-4 flex gap-1 rounded-lg bg-slate-100 p-1 w-fit">{(["all", "open", "resolved"] as const).map((item) =>
                <button key={item} onClick={() => setFilter(item)}
                        className={`rounded-md px-3 py-1.5 text-xs font-medium ${filter === item ? "bg-white text-slate-900 shadow-sm" : "text-slate-500"}`}>{item === "all" ? "Tất cả" : item === "open" ? "Chưa xử lý" : "Đã xử lý"}</button>)}</div>
            <div className="space-y-3">{loading ? <p className="py-10 text-center text-sm text-slate-500">Đang
                tải...</p> : filteredTickets.map((ticket) => <article key={ticket.ticketId}
                                                                      className="rounded-xl border border-slate-200 bg-white p-5">
                <div className="flex items-start justify-between gap-4">
                    <div className="min-w-0 flex-1">
                        <div className="mb-2 flex flex-wrap items-center gap-2"><span
                            className="rounded-md bg-blue-50 px-2 py-1 text-xs font-semibold text-blue-700">{ticketLabel[ticket.ticketType]}</span><span
                            className="font-mono text-xs text-slate-400">{ticket.ticketId}</span><span
                            className="text-xs text-slate-500">Account {ticket.ticketAccountId}</span></div>
                        <p className="whitespace-pre-wrap text-sm text-slate-800">{ticket.ticketContent}</p><p
                        className="mt-2 text-xs text-slate-400">Tạo
                        lúc {new Date(ticket.ticketCreatedAt).toLocaleString("vi-VN")}</p>
                    </div>
                    {ticket.ticketIsResolved ?
                        <span className="rounded-md bg-emerald-50 px-2 py-1 text-xs font-medium text-emerald-700">Đã xử lý</span> :
                        <button className={`${button} bg-blue-600 text-white`} disabled={saving}
                                onClick={() => void run(async () => setTickets((current) => current.map((item) => item.ticketId === ticket.ticketId ? {
                                    ...item,
                                    ticketIsResolved: true,
                                    ticketUpdatedAt: new Date().toISOString()
                                } : item)), "Đã xử lý ticket.")}>Đánh dấu xử lý</button>}</div>
                {ticket.ticketMedias.length > 0 &&
                    <div className="mt-3 flex flex-wrap gap-2">{ticket.ticketMedias.map((media, index) => <button
                        key={media.ticketMediaId} type="button" onClick={() => setImageToView({
                        urls: ticket.ticketMedias.map((item) => item.ticketMediaImageUrl),
                        index
                    })} aria-label={`Xem ảnh ${index + 1} đính kèm`}
                        className="h-20 w-20 overflow-hidden rounded-lg border border-slate-200 bg-slate-100"><img
                        src={media.ticketMediaImageUrl} alt={`Ảnh ${index + 1} đính kèm ticket`}
                        className="h-full w-full object-cover"/></button>)}</div>}
            </article>)}{!loading && filteredTickets.length === 0 &&
                <p className="rounded-xl border border-dashed border-slate-300 bg-white py-10 text-center text-sm text-slate-500">Không
                    có ticket.</p>}</div>
        </> : <div className="space-y-3">{loading || loadingNotifications ?
            <p className="py-10 text-center text-sm text-slate-500">Đang tải...</p> : visibleNotifications.map((item) =>
                <article key={item.notificationId}
                         className={`rounded-xl border bg-white p-5 ${item.notificationIsRead ? "border-slate-100" : "border-blue-200"}`}>
                    <div className="flex items-start justify-between gap-4">
                        <div className="min-w-0">
                            <div className="mb-2 flex flex-wrap items-center gap-2"><span
                                className="rounded-md bg-violet-50 px-2 py-1 text-xs font-semibold text-violet-700">{notificationLabel[item.notificationType]}</span><span
                                className="font-mono text-xs text-slate-400">{item.notificationId}</span><span
                                className="text-xs text-slate-500">Người gửi {item.notificationSenderAccountId}</span>
                            </div>
                            <p className="text-sm text-slate-800">{item.notificationContent}</p><p
                            className="mt-2 text-xs text-slate-500">{item.notificationRecipients.length} người nhận
                            · {new Date(item.notificationCreatedAt).toLocaleString("vi-VN")}</p></div>
                        {item.notificationIsRead ? <span className="text-xs text-slate-400">Đã đọc</span> :
                            <button className={`${button} bg-slate-900 text-white`} disabled={saving}
                                    onClick={() => markRead(item)}>Đánh dấu đã đọc</button>}</div>
                </article>)}{!loading && visibleNotifications.length === 0 &&
            <p className="rounded-xl border border-dashed border-slate-300 bg-white py-10 text-center text-sm text-slate-500">Không
                có thông báo.</p>}</div>}

        {modal === "tickets" && <Modal title="Tạo ticket" onClose={() => setModal(null)}>
            <form onSubmit={(event) => void createTicket(event)}>
                <FormField label="Account ID"><input className={inputCls} required placeholder="GUID tài khoản"
                                                     value={ticketForm.accountId} onChange={(event) => setTicketForm({
                    ...ticketForm,
                    accountId: event.target.value
                })}/></FormField>
                <FormField label="Loại ticket"><select className={selectCls} value={ticketForm.type}
                                                       onChange={(event) => setTicketForm({
                                                           ...ticketForm,
                                                           type: event.target.value as TicketType
                                                       })}>{ticketTypes.map((type) => <option key={type}
                                                                                              value={type}>{ticketLabel[type]}</option>)}</select></FormField>
                <FormField label="Nội dung"><textarea className={inputCls} required maxLength={255} rows={4}
                                                      value={ticketForm.content} onChange={(event) => setTicketForm({
                    ...ticketForm,
                    content: event.target.value
                })}/></FormField>
                <FormField label="Ảnh đính kèm"><ImageAttachments files={ticketImages}
                                                                  onChange={setTicketImages}/></FormField>
                <div className="flex justify-end gap-2">
                    <button type="button" className={`${button} border border-slate-200`}
                            onClick={() => setModal(null)}>Hủy
                    </button>
                    <button className={`${button} bg-blue-600 text-white`}
                            disabled={saving}>{saving ? "Đang tải ảnh..." : "Tạo ticket"}</button>
                </div>
            </form>
        </Modal>}

        {modal === "notifications" && <Modal title="Tạo thông báo" onClose={() => setModal(null)}>
            <form onSubmit={(event) => void createNotification(event)}>
                <FormField label="Sender account ID"><input className={inputCls} required placeholder="GUID người gửi"
                                                            value={notificationForm.senderAccountId}
                                                            onChange={(event) => setNotificationForm({
                                                                ...notificationForm,
                                                                senderAccountId: event.target.value
                                                            })}/></FormField>
                <FormField label="Recipient account ID"><input className={inputCls} required
                                                               placeholder="GUID người nhận"
                                                               value={notificationForm.recipientAccountId}
                                                               onChange={(event) => setNotificationForm({
                                                                   ...notificationForm,
                                                                   recipientAccountId: event.target.value
                                                               })}/></FormField>
                <FormField label="Loại thông báo"><select className={selectCls} value={notificationForm.type}
                                                          onChange={(event) => setNotificationForm({
                                                              ...notificationForm,
                                                              type: event.target.value as NotificationType
                                                          })}>{notificationTypes.map((type) => <option key={type}
                                                                                                       value={type}>{notificationLabel[type]}</option>)}</select></FormField>
                <FormField label="Nội dung"><textarea className={inputCls} required maxLength={255} rows={4}
                                                      value={notificationForm.content}
                                                      onChange={(event) => setNotificationForm({
                                                          ...notificationForm,
                                                          content: event.target.value
                                                      })}/></FormField>
                <div className="flex justify-end gap-2">
                    <button type="button" className={`${button} border border-slate-200`}
                            onClick={() => setModal(null)}>Hủy
                    </button>
                    <button className={`${button} bg-slate-900 text-white`} disabled={saving}>Tạo thông báo</button>
                </div>
            </form>
        </Modal>}
        {imageToView && <div role="dialog" aria-modal="true"
                             aria-label={`Ảnh ${imageToView.index + 1} trong ${imageToView.urls.length} ảnh đính kèm`}
                             onClick={() => setImageToView(null)}
                             className="fixed inset-0 z-[80] flex items-center justify-center bg-black/95 p-4">
            <button type="button" aria-label="Đóng ảnh" onClick={() => setImageToView(null)}
                    className="absolute right-5 top-4 z-10 flex h-11 w-11 items-center justify-center rounded-full bg-white/10 text-3xl text-white">&times;</button>
            <img src={imageToView.urls[imageToView.index]} alt={`Ảnh ${imageToView.index + 1} đính kèm ticket`}
                 onClick={(event) => event.stopPropagation()}
                 className="max-h-[calc(100vh-3rem)] max-w-[calc(100vw-3rem)] object-contain"/>
            {imageToView.urls.length > 1 && <>
                <button type="button" aria-label="Ảnh trước" onClick={(event) => {
                    event.stopPropagation();
                    setImageToView((current) => current ? {
                        ...current,
                        index: (current.index - 1 + current.urls.length) % current.urls.length
                    } : null)
                }}
                        className="absolute left-4 top-1/2 flex h-12 w-12 -translate-y-1/2 items-center justify-center rounded-full bg-white/10 text-3xl text-white">‹
                </button>
                <button type="button" aria-label="Ảnh tiếp theo" onClick={(event) => {
                    event.stopPropagation();
                    setImageToView((current) => current ? {
                        ...current,
                        index: (current.index + 1) % current.urls.length
                    } : null)
                }}
                        className="absolute right-4 top-1/2 flex h-12 w-12 -translate-y-1/2 items-center justify-center rounded-full bg-white/10 text-3xl text-white">›
                </button>
                <span
                    className="absolute bottom-4 left-1/2 -translate-x-1/2 rounded-full bg-white/10 px-3 py-1 text-sm text-white">{imageToView.index + 1} / {imageToView.urls.length}</span>
            </>}
        </div>}
    </div>
}

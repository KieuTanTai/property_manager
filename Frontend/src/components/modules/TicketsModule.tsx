import type {SubmitEvent} from "react"
import {useEffect, useState} from "react"
import {compressImageToDataUrl} from "@/utils/compressImage"
import {
    type NotificationModel,
    type NotificationType,
    ticketApi,
    type TicketModel,
    type TicketType
} from "../../api/ticketAndNotification"
import {APP_SETTINGS} from "@/config/appSettings"
import {FormField, inputCls, Modal, ProcessingForm, selectCls} from "./shared"
import {ImageAttachments} from "./ImageAttachments"

const button = "rounded-lg px-3 py-2 text-sm font-medium transition-colors disabled:opacity-50"
const ticketTypes: TicketType[] = ["Review", "Complaint", "Violation", "Feedback"]
const ticketLabel: Record<TicketType, string> = {
    Review: "Đánh giá",
    Complaint: "Khiếu nại",
    Violation: "Tố cáo",
    Feedback: "Phản hồi"
}
const notificationLabel: Record<NotificationType, string> = {Violation: "Vi phạm", Ticket: "Yêu cầu", Other: "Khác"}

export function TicketsModule({
                                  accountId,
                                  customerMode = false,
                                  ownOnly = false,
                                  canReadOwn = false,
                                  canCreateViolation = false
                              }: {
    accountId?: string;
    customerMode?: boolean;
    ownOnly?: boolean;
    canReadOwn?: boolean;
    canCreateViolation?: boolean
}) {
    const canViewTickets = !customerMode && (canReadOwn || canCreateViolation)
    const [tickets, setTickets] = useState<TicketModel[]>([])
    const [notifications, setNotifications] = useState<NotificationModel[]>([])
    const [filter, setFilter] = useState<"all" | "open" | "resolved">("all")
    const [ticketTypeFilter, setTicketTypeFilter] = useState<TicketType | "all">("all")
    const [notificationFilter, setNotificationFilter] = useState<"all" | "unread" | "read">("all")
    const [notificationTypeFilter, setNotificationTypeFilter] = useState<NotificationType | "all">("all")
    const [ticketPage, setTicketPage] = useState(1)
    const [isEditingTicketPage, setIsEditingTicketPage] = useState(false)
    const [ticketPageInput, setTicketPageInput] = useState("1")
    const [notificationPage, setNotificationPage] = useState(1)
    const [isEditingNotificationPage, setIsEditingNotificationPage] = useState(false)
    const [notificationPageInput, setNotificationPageInput] = useState("1")
    const [ticketReload, setTicketReload] = useState(0)
    const [notificationReload, setNotificationReload] = useState(0)
    const [modal, setModal] = useState<"tickets" | null>(null)
    const [ticketForm, setTicketForm] = useState({accountId: "", content: "", type: "Feedback" as TicketType})
    const [ticketImages, setTicketImages] = useState<File[]>([])
    const [imageToView, setImageToView] = useState<{ urls: string[]; index: number } | null>(null)
    const [loadingTickets, setLoadingTickets] = useState(true)
    const [saving, setSaving] = useState(false)
    const [error, setError] = useState<string | null>(null)
    const [ticketLoadError, setTicketLoadError] = useState<string | null>(null)
    const [notificationLoadError, setNotificationLoadError] = useState<string | null>(null)
    const [notice, setNotice] = useState<string | null>(null)
    const [loadingNotifications, setLoadingNotifications] = useState(customerMode)

    useEffect(() => {
        if (customerMode || !canViewTickets) {
            setLoadingTickets(false)
            return
        }

        let isCurrent = true
        setLoadingTickets(true)
        setTicketLoadError(null)
        void ticketApi.tickets().then((items) => {
            if (isCurrent) setTickets(items)
        }).catch((cause: unknown) => {
            if (!isCurrent) return
            setTickets([])
            setTicketLoadError(cause instanceof Error ? cause.message : "Không thể tải danh sách yêu cầu.")
        }).finally(() => {
            if (isCurrent) setLoadingTickets(false)
        })
        return () => {
            isCurrent = false
        }
    }, [canViewTickets, customerMode, ticketReload])

    useEffect(() => {
        if (!customerMode) {
            setLoadingNotifications(false)
            return
        }

        if (!accountId) {
            setNotifications([])
            setNotificationLoadError("Không tìm thấy tài khoản để tải thông báo.")
            setLoadingNotifications(false)
            return
        }

        let isCurrent = true
        setLoadingNotifications(true)
        setNotificationLoadError(null)
        void ticketApi.notificationsByRecipient(accountId).then((items) => {
            if (isCurrent) {
                setNotifications([...items].sort((left, right) =>
                    Date.parse(right.notificationCreatedAt) - Date.parse(left.notificationCreatedAt)))
            }
        }).catch((cause: unknown) => {
            if (!isCurrent) return
            setNotifications([])
            setNotificationLoadError(cause instanceof Error ? cause.message : "Không thể tải thông báo.")
        }).finally(() => {
            if (isCurrent) setLoadingNotifications(false)
        })
        return () => {
            isCurrent = false
        }
    }, [accountId, customerMode, notificationReload])

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
            const imageDataUrls: string[] = await Promise.all(selectedImages.map((file) => compressImageToDataUrl(file, "standard")))
            const created = await ticketApi.createTicket({
                accountId: ticketForm.accountId,
                content: ticketForm.content,
                type: ticketForm.type
            })
            let ticketMedias = created.ticketMedias ?? []
            if (selectedImages.length > 0) {
                await ticketApi.addTicketMedias(created.ticketId, imageDataUrls)
                ticketMedias = await ticketApi.ticketMedias(created.ticketId)
            }
            setTickets((current) => [{...created, ticketMedias}, ...current])
            setTicketPage(1)
        }, "Đã tạo yêu cầu.")
        if (done) {
            setTicketImages([]);
            setModal(null)
        }
    }
    const markRead = (notification: NotificationModel) => void run(async () => {
        const updated = await ticketApi.updateNotification(notification, true)
        setNotifications((current) => current.map((item) =>
            item.notificationId === updated.notificationId ? updated : item))
    }, "Đã đánh dấu thông báo đã đọc.")
    const accountTickets = ownOnly && accountId ? tickets.filter((ticket) => ticket.ticketAccountId === accountId) : tickets
    const filteredTickets = accountTickets.filter((ticket) =>
        (filter === "all" || ticket.ticketIsResolved === (filter === "resolved"))
        && (ticketTypeFilter === "all" || ticket.ticketType === ticketTypeFilter))
    const ticketPageSize = APP_SETTINGS.pagination.defaultPageSize
    const ticketPageCount = Math.max(1, Math.ceil(filteredTickets.length / ticketPageSize))
    const activeTicketPage = Math.min(ticketPage, ticketPageCount)
    const visibleTickets = filteredTickets.slice((activeTicketPage - 1) * ticketPageSize, activeTicketPage * ticketPageSize)
    const filteredNotifications = notifications.filter((notification) =>
        (notificationFilter === "all"
            || notification.notificationIsRead === (notificationFilter === "read"))
        && (notificationTypeFilter === "all" || notification.notificationType === notificationTypeFilter))
    const notificationPageSize = APP_SETTINGS.pagination.defaultPageSize
    const notificationPageCount = Math.max(1, Math.ceil(filteredNotifications.length / notificationPageSize))
    const activeNotificationPage = Math.min(notificationPage, notificationPageCount)
    const visibleNotifications = filteredNotifications.slice(
        (activeNotificationPage - 1) * notificationPageSize,
        activeNotificationPage * notificationPageSize)
    const unreadNotificationCount = notifications.filter((notification) =>
        !notification.notificationIsRead
        && (notificationTypeFilter === "all" || notification.notificationType === notificationTypeFilter)).length

    useEffect(() => {
        if (ticketPage !== activeTicketPage) setTicketPage(activeTicketPage)
    }, [activeTicketPage, ticketPage])
    useEffect(() => setTicketPageInput(String(activeTicketPage)), [activeTicketPage])
    useEffect(() => {
        if (notificationPage !== activeNotificationPage) setNotificationPage(activeNotificationPage)
    }, [activeNotificationPage, notificationPage])
    useEffect(() => setNotificationPageInput(String(activeNotificationPage)), [activeNotificationPage])

    const commitTicketPageInput = () => {
        const inputPage = Number(ticketPageInput)
        const nextPage = Number.isFinite(inputPage)
            ? Math.max(1, Math.min(ticketPageCount, Math.trunc(inputPage)))
            : activeTicketPage
        setTicketPage(nextPage)
        setTicketPageInput(String(nextPage))
        setIsEditingTicketPage(false)
    }

    const commitNotificationPageInput = () => {
        const inputPage = Number(notificationPageInput)
        const nextPage = Number.isFinite(inputPage)
            ? Math.max(1, Math.min(notificationPageCount, Math.trunc(inputPage)))
            : activeNotificationPage
        setNotificationPage(nextPage)
        setNotificationPageInput(String(nextPage))
        setIsEditingNotificationPage(false)
    }

    return <div>
        <div className="mb-5 flex flex-wrap items-center justify-between gap-3">
            <div><h1
                className="text-xl font-bold text-slate-900">{customerMode ? "Thông báo của tôi" : "Yêu cầu"}</h1>
                <p className="mt-1 text-sm text-slate-500">{customerMode ? `${unreadNotificationCount} thông báo chưa đọc` : `${accountTickets.filter((ticket) => !ticket.ticketIsResolved).length} ticket chưa xử lý`}</p>
            </div>
            {canCreateViolation && <div className="flex gap-2">
                <button className={`${button} bg-blue-600 text-white hover:bg-blue-700`} onClick={() => {
                    setTicketForm({accountId: accountId ?? "", content: "", type: "Violation"});
                    setTicketImages([]);
                    setModal("tickets")
                }}>Tạo ticket vi phạm
                </button>
            </div>}
        </div>

        {error && <div role="alert"
                       className="mb-4 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">{error}</div>}
        {notice && <div role="status"
                        className="mb-4 rounded-lg border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700">{notice}</div>}

        {!customerMode && canViewTickets ? <>
            {ticketLoadError && <div role="alert"
                                      className="mb-4 flex flex-wrap items-center justify-between gap-3 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
                <span>{ticketLoadError}</span>
                <button type="button" className={`${button} border border-red-200 bg-white`}
                        onClick={() => setTicketReload((current) => current + 1)}>Thử tải lại</button>
            </div>}
            <div className="mb-4 flex flex-wrap items-center gap-3">
                <div
                    className="flex gap-1 rounded-lg bg-slate-100 p-1 w-fit">{(["all", "open", "resolved"] as const).map((item) =>
                    <button key={item} onClick={() => {
                        setFilter(item)
                        setTicketPage(1)
                    }}
                            className={`rounded-md px-3 py-1.5 text-xs font-medium ${filter === item ? "bg-white text-slate-900 shadow-sm" : "text-slate-500"}`}>{item === "all" ? "Tất cả" : item === "open" ? "Chưa xử lý" : "Đã xử lý"}</button>)}</div>
                <label className="flex items-center gap-2 text-sm text-slate-600">
                    <span>Loại yêu cầu</span>
                    <select className={selectCls} aria-label="Lọc theo loại yêu cầu"
                            value={ticketTypeFilter}
                            onChange={(event) => {
                                setTicketTypeFilter(event.target.value as TicketType | "all")
                                setTicketPage(1)
                            }}>
                        <option value="all">Tất cả loại</option>
                        {ticketTypes.map((type) =>
                            <option key={type} value={type}>{ticketLabel[type]}</option>)}
                    </select>
                </label>
            </div>
            <div className="space-y-3">{loadingTickets ? <p className="py-10 text-center text-sm text-slate-500">Đang
                tải...</p> : visibleTickets.map((ticket) => <article key={ticket.ticketId}
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
                        !ownOnly && <button className={`${button} bg-blue-600 text-white`} disabled={saving}
                                            onClick={() => void run(async () => {
                                                const updated = await ticketApi.updateTicket(ticket, true)
                                                setTickets((current) => current.map((item) => item.ticketId === updated.ticketId
                                                    ? {...updated, ticketMedias: item.ticketMedias}
                                                    : item))
                                            }, "Đã xử lý ticket.")}>Đánh dấu xử lý</button>}</div>
                {ticket.ticketMedias.length > 0 &&
                    <div className="mt-3 flex flex-wrap gap-2">{ticket.ticketMedias.map((media, index) => <button
                        key={media.ticketMediaId} type="button" onClick={() => setImageToView({
                        urls: ticket.ticketMedias.map((item) => item.ticketMediaImageUrl),
                        index
                    })} aria-label={`Xem ảnh ${index + 1} đính kèm`}
                        className="h-20 w-20 overflow-hidden rounded-lg border border-slate-200 bg-slate-100"><img
                        src={media.ticketMediaImageUrl} alt={`Ảnh ${index + 1} đính kèm ticket`}
                        className="h-full w-full object-cover"/></button>)}</div>}
            </article>)}{!loadingTickets && !ticketLoadError && filteredTickets.length === 0 &&
                <p className="rounded-xl border border-dashed border-slate-300 bg-white py-10 text-center text-sm text-slate-500">Không
                    có ticket.</p>}</div>
            {!loadingTickets && filteredTickets.length > ticketPageSize && <nav aria-label="Phân trang yêu cầu"
                className="mt-4 flex justify-center border-t border-slate-100 px-3 py-3">
                <div className="flex items-center justify-center gap-4 text-sm">
                    <button type="button" aria-label="Trang trước" disabled={activeTicketPage <= 1}
                            onClick={() => setTicketPage((current) => Math.max(1, current - 1))}
                            className="rounded-md border border-slate-200 p-2 text-slate-600 transition-colors hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-40">
                        <svg aria-hidden="true" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"
                             className="h-4 w-4">
                            <path strokeLinecap="round" strokeLinejoin="round" d="m15 18-6-6 6-6"/>
                        </svg>
                    </button>
                    <span aria-live="polite" className="min-w-20 text-center text-slate-600">
                        {isEditingTicketPage ? <input
                            aria-label="Chuyển đến trang"
                            type="number"
                            min={1}
                            max={ticketPageCount}
                            step={1}
                            autoFocus
                            value={ticketPageInput}
                            onChange={(event) => setTicketPageInput(event.target.value)}
                            onBlur={commitTicketPageInput}
                            onKeyDown={(event) => {
                                if (event.key === "Enter") commitTicketPageInput()
                                if (event.key === "Escape") {
                                    setTicketPageInput(String(activeTicketPage))
                                    setIsEditingTicketPage(false)
                                }
                            }}
                            className="w-12 rounded border border-blue-300 px-1 py-0.5 text-center font-semibold text-blue-600 outline-none focus:ring-2 focus:ring-blue-200"
                        /> : <button
                            type="button"
                            aria-label={`Trang hiện tại ${activeTicketPage}, bấm để nhập số trang`}
                            onClick={() => {
                                setTicketPageInput(String(activeTicketPage))
                                setIsEditingTicketPage(true)
                            }}
                            className="rounded px-1 font-semibold text-blue-600 hover:bg-blue-50"
                        >{activeTicketPage}</button>}
                        <span className="mx-1">/</span>
                        {ticketPageCount}
                    </span>
                    <button type="button" aria-label="Trang sau" disabled={activeTicketPage >= ticketPageCount}
                            onClick={() => setTicketPage((current) => Math.min(ticketPageCount, current + 1))}
                            className="rounded-md border border-slate-200 p-2 text-slate-600 transition-colors hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-40">
                        <svg aria-hidden="true" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"
                             className="h-4 w-4">
                            <path strokeLinecap="round" strokeLinejoin="round" d="m9 18 6-6-6-6"/>
                        </svg>
                    </button>
                </div>
            </nav>}
        </> : <>
            {notificationLoadError && <div role="alert"
                                           className="mb-4 flex flex-wrap items-center justify-between gap-3 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
                <span>{notificationLoadError}</span>
                {accountId && <button type="button" className={`${button} border border-red-200 bg-white`}
                                       onClick={() => setNotificationReload((current) => current + 1)}>Thử tải lại</button>}
            </div>}
            <div className="mb-4 flex flex-wrap items-center gap-3">
                <div className="flex w-fit gap-1 rounded-lg bg-slate-100 p-1">
                    {(["all", "unread", "read"] as const).map((item) =>
                        <button key={item} type="button" onClick={() => {
                            setNotificationFilter(item)
                            setNotificationPage(1)
                        }}
                                className={`rounded-md px-3 py-1.5 text-xs font-medium ${notificationFilter === item ? "bg-white text-slate-900 shadow-sm" : "text-slate-500"}`}>
                            {item === "all" ? "Tất cả" : item === "unread" ? "Chưa đọc" : "Đã đọc"}
                        </button>)}
                </div>
                <label className="flex items-center gap-2 text-sm text-slate-600">
                    <span>Loại thông báo</span>
                    <select className={selectCls} aria-label="Lọc theo loại thông báo"
                            value={notificationTypeFilter}
                            onChange={(event) => {
                                setNotificationTypeFilter(event.target.value as NotificationType | "all")
                                setNotificationPage(1)
                            }}>
                        <option value="all">Tất cả loại</option>
                        {Object.entries(notificationLabel).map(([type, label]) =>
                            <option key={type} value={type}>{label}</option>)}
                    </select>
                </label>
            </div>
            <div className="space-y-3">{loadingNotifications ?
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
                            className="mt-2 text-xs text-slate-500">{new Date(item.notificationCreatedAt).toLocaleString("vi-VN")}</p></div>
                        {item.notificationIsRead ? <span className="text-xs text-slate-400">Đã đọc</span> :
                            <button className={`${button} bg-slate-900 text-white`} disabled={saving}
                                    onClick={() => markRead(item)}>Đánh dấu đã đọc</button>}</div>
                </article>)}{!loadingNotifications && !notificationLoadError && visibleNotifications.length === 0 &&
            <p className="rounded-xl border border-dashed border-slate-300 bg-white py-10 text-center text-sm text-slate-500">Không
                có thông báo.</p>}</div>
            {!loadingNotifications && filteredNotifications.length > notificationPageSize && <nav
                aria-label="Phân trang thông báo"
                className="mt-4 flex justify-center border-t border-slate-100 px-3 py-3">
                <div className="flex items-center justify-center gap-4 text-sm">
                    <button type="button" aria-label="Trang thông báo trước" disabled={activeNotificationPage <= 1}
                            onClick={() => setNotificationPage((current) => Math.max(1, current - 1))}
                            className="rounded-md border border-slate-200 p-2 text-slate-600 transition-colors hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-40">
                        <svg aria-hidden="true" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"
                             className="h-4 w-4">
                            <path strokeLinecap="round" strokeLinejoin="round" d="m15 18-6-6 6-6"/>
                        </svg>
                    </button>
                    <span aria-live="polite" className="min-w-20 text-center text-slate-600">
                        {isEditingNotificationPage ? <input
                            aria-label="Chuyển đến trang thông báo"
                            type="number"
                            min={1}
                            max={notificationPageCount}
                            step={1}
                            autoFocus
                            value={notificationPageInput}
                            onChange={(event) => setNotificationPageInput(event.target.value)}
                            onBlur={commitNotificationPageInput}
                            onKeyDown={(event) => {
                                if (event.key === "Enter") commitNotificationPageInput()
                                if (event.key === "Escape") {
                                    setNotificationPageInput(String(activeNotificationPage))
                                    setIsEditingNotificationPage(false)
                                }
                            }}
                            className="w-12 rounded border border-blue-300 px-1 py-0.5 text-center font-semibold text-blue-600 outline-none focus:ring-2 focus:ring-blue-200"
                        /> : <button
                            type="button"
                            aria-label={`Trang thông báo hiện tại ${activeNotificationPage}, bấm để nhập số trang`}
                            onClick={() => {
                                setNotificationPageInput(String(activeNotificationPage))
                                setIsEditingNotificationPage(true)
                            }}
                            className="rounded px-1 font-semibold text-blue-600 hover:bg-blue-50"
                        >{activeNotificationPage}</button>}
                        <span className="mx-1">/</span>
                        {notificationPageCount}
                    </span>
                    <button type="button" aria-label="Trang thông báo sau"
                            disabled={activeNotificationPage >= notificationPageCount}
                            onClick={() => setNotificationPage((current) =>
                                Math.min(notificationPageCount, current + 1))}
                            className="rounded-md border border-slate-200 p-2 text-slate-600 transition-colors hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-40">
                        <svg aria-hidden="true" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"
                             className="h-4 w-4">
                            <path strokeLinecap="round" strokeLinejoin="round" d="m9 18 6-6-6-6"/>
                        </svg>
                    </button>
                </div>
            </nav>}
        </>}

        {modal === "tickets" && <Modal title="Tạo ticket" onClose={() => setModal(null)}>
            <ProcessingForm processing={saving} onSubmit={createTicket}>
                <FormField label="Account ID"><input className={inputCls} required placeholder="GUID tài khoản"
                                                     value={ticketForm.accountId} onChange={(event) => setTicketForm({
                    ...ticketForm,
                    accountId: event.target.value
                })}/></FormField>
                <FormField label="Loại ticket"><select className={selectCls} value={ticketForm.type} disabled
                                                       onChange={(event) => setTicketForm({
                                                           ...ticketForm,
                                                           type: event.target.value as TicketType
                                                       })}>{(canCreateViolation ? ["Violation"] as TicketType[] : ticketTypes).map((type) =>
                    <option key={type}
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
            </ProcessingForm>
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

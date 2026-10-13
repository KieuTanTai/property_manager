export type TicketType = "Review" | "Complaint" | "Violation" | "Feedback"
export type NotificationType = "Violation" | "Ticket" | "Other"

export type TicketMediaModel = {
  ticketMediaId: number
  ticketMediaTicketId: string
  ticketMediaImageUrl: string
}

export type TicketModel = {
  ticketId: string
  ticketAccountId: string
  ticketContent: string
  ticketType: TicketType
  ticketIsResolved: boolean
  ticketCreatedAt: string
  ticketUpdatedAt: string
  ticketMedias: TicketMediaModel[]
}

export type NotificationRecipientModel = {
  notificationId: string
  accountId: string
}

export type NotificationModel = {
  notificationId: string
  notificationSenderAccountId: string
  notificationType: NotificationType
  notificationContent: string
  notificationIsRead: boolean
  notificationCreatedAt: string
  notificationUpdatedAt: string
  notificationRecipients: NotificationRecipientModel[]
}

const base = (import.meta.env.VITE_TICKET_API_BASE_URL || "/api").replace(/\/$/, "")

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  let response: Response
  try {
    response = await fetch(`${base}${path}`, {
      ...init,
      headers: { ...(init?.body ? { "Content-Type": "application/json" } : {}), ...init?.headers },
    })
  } catch {
    throw new Error(`Không thể kết nối Ticket & Notification API tại ${base}; kiểm tra Backend, HTTPS và CORS.`)
  }
  if (!response.ok) throw new Error((await response.text()) || `Ticket & Notification API trả về HTTP ${response.status}.`)
  if (response.status === 204) return undefined as T
  return response.json() as Promise<T>
}

export const ticketApi = {
  tickets: () => request<TicketModel[]>("/tickets"),
  notifications: () => request<NotificationModel[]>("/notifications"),
  createTicket: (body: { accountId: string; content: string; type: TicketType }) =>
    request<TicketModel>("/tickets", { method: "POST", body: JSON.stringify(body) }),
  updateTicket: (ticket: TicketModel, isResolved: boolean) =>
    request<TicketModel>("/tickets", {
      method: "PUT",
      body: JSON.stringify({
        ticketId: ticket.ticketId,
        accountId: ticket.ticketAccountId,
        content: ticket.ticketContent,
        type: ticket.ticketType,
        isResolved,
      }),
    }),
  createNotification: (body: { senderAccountId: string; content: string; type: NotificationType }) =>
    request<NotificationModel>("/notifications", { method: "POST", body: JSON.stringify(body) }),
  addRecipient: (notificationId: string, accountId: string) =>
    request<number>("/notification-recipients", {
      method: "POST",
      body: JSON.stringify({ notificationId, accountId }),
    }),
  updateNotification: (notification: NotificationModel, isRead: boolean) =>
    request<NotificationModel>("/notifications", {
      method: "PUT",
      body: JSON.stringify({
        notificationId: notification.notificationId,
        senderAccountId: notification.notificationSenderAccountId,
        type: notification.notificationType,
        content: notification.notificationContent,
        isRead,
      }),
    }),
}

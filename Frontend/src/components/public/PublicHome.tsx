import React, {useEffect, useMemo, useRef, useState} from "react"
import {APP_SETTINGS} from "@/config/appSettings"
import {LOCATIONS, PREMISES} from "@/mocks/shared.ts"
import {getMockPremiseData, subscribeToMockPremiseChanges} from "@/mocks/premises.ts"
import {contractApi} from "@/api/contracts"
import type {ContactInfoModel} from "@/types/contracts"
import {Modal, PremiseStatusBadge, ProcessingForm, premiseStatusSurface} from "../modules/shared"
import type {Premise} from "../premises/types"
import {PublicHomeProps} from "@/components/public/types.ts";
import {throttle} from "lodash";
import debounce from "lodash.debounce"

const PREMISE_STATUS_TABS = [
    {key: "all", label: "Tất cả"},
    {key: "available", label: "Còn trống"},
    {key: "rented", label: "Đang thuê"},
    {key: "reserved", label: "Đã đặt cọc"},
] as const

type PremiseStatusTab = (typeof PREMISE_STATUS_TABS)[number]["key"]

const GOOGLE_LOGIN_SESSION_KEY = "contact-google-login"
const GOOGLE_LOGIN_SESSION_MS = 5 * 60 * 1000
const GOOGLE_LOGIN_EXTENSION_STEP = 60 * 1000

let googleIdentityScriptPromise: Promise<any> | null = null
let googleIdentityInitialized = false
let googleCredentialHandler: ((response: {credential?: string}) => void) | null = null

function loadGoogleIdentity() {
    const google = (window as any).google
    if (google?.accounts?.id) return Promise.resolve(google)
    if (!googleIdentityScriptPromise) {
        googleIdentityScriptPromise = new Promise((resolve, reject) => {
            const script = document.createElement("script")
            script.src = "https://accounts.google.com/gsi/client"
            script.async = true
            script.defer = true
            script.onload = () => resolve((window as any).google)
            script.onerror = () => reject(new Error("Không tải được Google Identity Services."))
            document.head.appendChild(script)
        })
    }
    return googleIdentityScriptPromise
}

const formatArea = (value: string) => value.replace(/m²?$/i, " m²")

export function PublicHome({onManager}: PublicHomeProps) {
    const [query, setQuery] = useState("")
    const [debouncedQuery, setDebouncedQuery] = useState("")
    const updateDebouncedQuery = useMemo(() => debounce(setDebouncedQuery, APP_SETTINGS.debounceDelayMs), [])
    useEffect(() => () => updateDebouncedQuery.cancel(), [updateDebouncedQuery])
    const [mockPremiseData, setMockPremiseData] = useState(getMockPremiseData)
    const [statusTab, setStatusTab] = useState<PremiseStatusTab>("all")
    const [page, setPage] = useState(1)
    const [contactName, setContactName] = useState("")
    const [contactPhoneNumber, setContactPhoneNumber] = useState("")
    const [contactSenderEmail, setContactSenderEmail] = useState(() => {
        try {
            const session = JSON.parse(sessionStorage.getItem(GOOGLE_LOGIN_SESSION_KEY) ?? "null")
            return session?.expiresAt > Date.now() ? session.email as string : ""
        } catch {
            return ""
        }
    })
    const [googleLoginExpiresAt, setGoogleLoginExpiresAt] = useState<number | null>(() => {
        try {
            const session = JSON.parse(sessionStorage.getItem(GOOGLE_LOGIN_SESSION_KEY) ?? "null")
            return session?.expiresAt > Date.now() ? session.expiresAt as number : null
        } catch {
            return null
        }
    })
    
    const [logout, setLogout] = useState(false)
    const [googleIdentityReady, setGoogleIdentityReady] = useState(false)
    const googleButtonRef = useRef<HTMLDivElement>(null)
    const [contactPremiseId, setContactPremiseId] = useState("")
    const [contactMessage, setContactMessage] = useState("")
    const [contactError, setContactError] = useState<string | null>(null)
    const [contactSuccess, setContactSuccess] = useState<string | null>(null)
    const [contactSending, setContactSending] = useState(false)
    const [ownerContact, setOwnerContact] = useState<ContactInfoModel | null>(null)
    const [selectedPremise, setSelectedPremise] = useState<Premise | null>(null)
    const [activeMediaIndex, setActiveMediaIndex] = useState(0)
    const [isImageFullscreen, setIsImageFullscreen] = useState(false)

    useEffect(() => subscribeToMockPremiseChanges(() => setMockPremiseData(getMockPremiseData())), [])
    const googleLoginMaxExpiresAt =
        googleLoginExpiresAt ? googleLoginExpiresAt + GOOGLE_LOGIN_SESSION_MS : GOOGLE_LOGIN_SESSION_MS

    useEffect(() => {
        const clientId = import.meta.env.VITE_GOOGLE_CLIENT_ID?.trim()
        if (!clientId) return
        let isCurrent = true
        const handleGoogleCredential = (response: {credential?: string}) => {
            try {
                const payload = response.credential?.split(".")[1]
                if (!payload) throw new Error("Google không trả về email.")
                const claims = JSON.parse(atob(payload.replace(/-/g, "+").replace(/_/g, "/")))
                if (!claims.email || !claims.email_verified || !claims.email.toLowerCase().endsWith("@gmail.com")) {
                    throw new Error("Vui lòng đăng nhập bằng tài khoản Gmail đã xác minh.")
                }
                setContactSenderEmail(claims.email)
                const expiresAt = Date.now() + GOOGLE_LOGIN_SESSION_MS
                sessionStorage.setItem(GOOGLE_LOGIN_SESSION_KEY, JSON.stringify({email: claims.email, expiresAt}))
                setGoogleLoginExpiresAt(expiresAt)
                setContactError(null)
            } catch (cause) {
                setContactError(cause instanceof Error ? cause.message : "Không đọc được email Google.")
            }
        }
        googleCredentialHandler = handleGoogleCredential
        void loadGoogleIdentity().then((google) => {
            if (!isCurrent || !google?.accounts?.id) return
            if (!googleIdentityInitialized) {
                google.accounts.id.initialize({
                    client_id: clientId,
                    callback: (response: {credential?: string}) => googleCredentialHandler?.(response),
                })
                googleIdentityInitialized = true
            }
            if (googleButtonRef.current) {
                googleButtonRef.current.replaceChildren()
                google.accounts.id.renderButton(googleButtonRef.current, {
                    type: "standard",
                    theme: "outline",
                    size: "large",
                    text: "signin_with",
                    shape: "rectangular",
                    width: 360,
                })
            }
            setGoogleIdentityReady(true)
        }).catch((cause) => {
            if (isCurrent) setContactError(cause instanceof Error ? cause.message : "Không tải được đăng nhập Google.")
        })
        return () => {
            isCurrent = false
            if (googleCredentialHandler === handleGoogleCredential) googleCredentialHandler = null
        }
    }, [])

    useEffect(() => {
        if (!contactSenderEmail || !googleLoginExpiresAt) return
        const remaining = googleLoginExpiresAt - Date.now()
        if (remaining <= 0) {
            sessionStorage.removeItem(GOOGLE_LOGIN_SESSION_KEY)
            setContactSenderEmail("")
            setGoogleLoginExpiresAt(null)
            return
        }
        const timeout = window.setTimeout(() => {
            sessionStorage.removeItem(GOOGLE_LOGIN_SESSION_KEY)
            setContactSenderEmail("")
            setGoogleLoginExpiresAt(null)
            setContactError("Phiên đăng nhập Google đã hết hạn. Vui lòng đăng nhập lại để tiếp tục.")
        }, remaining)
        return () => window.clearTimeout(timeout)
    }, [contactSenderEmail, googleLoginExpiresAt])

    useEffect(() => {
        let isCurrent = true
        void contractApi.contactInfos().then((contacts) => {
            const currentContact = [...contacts].sort((left, right) =>
                new Date(right.contactInfoUpdatedAt).getTime() - new Date(left.contactInfoUpdatedAt).getTime(),
            )[0]
            if (isCurrent && currentContact) setOwnerContact(currentContact)
        }).catch(() => {
            if (isCurrent) setOwnerContact(null)
        })
        return () => {
            isCurrent = false
        }
    }, [])

    useEffect(() => {
        if (!selectedPremise && !isImageFullscreen) return
        const closeOnEscape = (event: KeyboardEvent) => {
            if (event.key !== "Escape") return
            if (isImageFullscreen) {
                setIsImageFullscreen(false)
            } else {
                setSelectedPremise(null)
            }
        }
        window.addEventListener("keydown", closeOnEscape)
        return () => window.removeEventListener("keydown", closeOnEscape)
    }, [isImageFullscreen, selectedPremise])

    const contactEmail = ownerContact?.contactInfoEmail?.trim() ?? ""
    const contactPhone = ownerContact?.contactInfoPhone?.trim() ?? ""
    const zaloContactUrl = contactPhone
        ? `https://zalo.me/${contactPhone.replace(/\D/g, "")}/`
        : null

    const allPremises = mockPremiseData.premises
    const availablePremises = useMemo(
        () => allPremises.filter((premise) => premise.status === "available"),
        [allPremises],
    )
    const heroPremise = allPremises.find((premise) => premise.id === "0195c8f6-cd51-7222-8222-200000000002") ?? PREMISES[0]
    const heroMedia = mockPremiseData.media.filter((item) => item.premiseMediaPremiseId === heroPremise?.id)
    const selectedPremiseMedia = selectedPremise
        ? mockPremiseData.media.filter((item) => item.premiseMediaPremiseId === selectedPremise.id)
        : []
    const searchedPremises = allPremises.filter((premise) => {
        const searchText = `${premise.name} ${premise.location} ${premise.description}`.toLowerCase()
        return searchText.includes(debouncedQuery.trim().toLowerCase())
    })
    const visiblePremises = statusTab === "all"
        ? searchedPremises
        : searchedPremises.filter((premise) => premise.status === statusTab)
    const pageCount = Math.max(1, Math.ceil(visiblePremises.length / APP_SETTINGS.pagination.defaultPageSize))
    const currentPage = Math.min(page, pageCount)
    const pagedPremises = visiblePremises.slice(
        (currentPage - 1) * APP_SETTINGS.pagination.defaultPageSize,
        currentPage * APP_SETTINGS.pagination.defaultPageSize,
    )

    const openPremise = (premise: Premise) => {
        setContactPremiseId(premise.id)
        setContactError(null)
        setActiveMediaIndex(0)
        setIsImageFullscreen(false)
        setSelectedPremise(premise)
    }

    const sendInquiry = async (event: React.SubmitEvent<HTMLFormElement>) => {
        event.preventDefault()
        if (!contactSenderEmail) {
            setContactError("Vui lòng đăng nhập Google để gửi kèm Gmail của bạn.")
            return
        }
        setContactError(null)
        setContactSuccess(null)
        setContactSending(true)
        const premise = availablePremises.find((item) => item.id === contactPremiseId)
        try {
            const result = await contractApi.sendConsultationRequest({
                name: contactName,
                phone: contactPhoneNumber,
                senderEmail: contactSenderEmail,
                premiseName: premise?.name ?? null,
                message: contactMessage,
            })
            setContactSuccess(`Đã gửi yêu cầu tư vấn đến ${result.recipientEmail}.`)
            setContactName("")
            setContactPhoneNumber("")
            setContactPremiseId("")
            setContactMessage("")
        } catch (cause) {
            setContactError(cause instanceof Error ? cause.message : "Không gửi được yêu cầu tư vấn.")
        } finally {
            setContactSending(false)
        }
    }

    const requestPremiseConsultation = () => {
        if (!selectedPremise || selectedPremise.status !== "available") return
        setContactPremiseId(selectedPremise.id)
        setSelectedPremise(null)
        window.setTimeout(() => {
            document.getElementById("contact")?.scrollIntoView({behavior: "smooth", block: "start"})
        }, 0)
    }

    const logoutGoogle = () => {
        setLogout(true)
        sessionStorage.removeItem(GOOGLE_LOGIN_SESSION_KEY)
        setContactSenderEmail("")
        setGoogleLoginExpiresAt(null)
    }
    
    const throttleExtendGoogleLogin = useMemo(
    () => throttle(() => {
        if (!contactSenderEmail || !googleLoginExpiresAt) return
        const now = Date.now()
        const remaining = googleLoginExpiresAt - now
        
        if (remaining < GOOGLE_LOGIN_SESSION_MS)
            setGoogleLoginExpiresAt(Math.min(googleLoginExpiresAt + GOOGLE_LOGIN_EXTENSION_STEP, googleLoginMaxExpiresAt))
        
    }, 10000), [contactSenderEmail, googleLoginExpiresAt, googleLoginMaxExpiresAt])

    return (
        <div className="min-h-screen bg-[#f7f8fa] text-slate-900">
            <header className="sticky top-0 z-40 border-b border-white/10 bg-[#111b2e]/95 text-white backdrop-blur">
                <div className="mx-auto flex max-w-7xl items-center justify-between px-5 py-4 lg:px-8">
                    <a href="#top" className="flex items-center gap-3" aria-label="ManagerSpace trang chủ">
                        <span
                            className="flex h-10 w-10 items-center justify-center rounded-xl bg-blue-500 text-sm font-bold">M</span>
                        <span>
              <span className="block text-sm font-semibold tracking-wide">ManagerSpace</span>
              <span className="block text-[11px] text-slate-400">Mặt bằng kinh doanh</span>
            </span>
                    </a>
                    <nav className="hidden items-center gap-8 text-sm text-slate-300 md:flex">
                        <a href="#spaces" className="transition hover:text-white">Mặt bằng</a>
                        <a href="#about" className="transition hover:text-white">Giới thiệu</a>
                        <a href="#contact" className="transition hover:text-white">Liên hệ</a>
                    </nav>
                    <div className="flex items-center gap-2">
                        <button onClick={onManager}
                                className="rounded-xl bg-white px-4 py-2.5 text-sm font-semibold text-slate-900 transition hover:bg-blue-50">ManagerSpace
                        </button>
                    </div>
                </div>
            </header>

            <main id="top">
                <section className="relative isolate overflow-hidden bg-[#111b2e] text-white">
                    <div className="absolute inset-0 -z-10 opacity-35">
                        <img src={heroMedia[0]?.premiseMediaImageUrl} alt=""
                             className="h-full w-full object-cover"/>
                        <div
                            className="absolute inset-0 bg-linear-to-r from-[#111b2e] via-[#111b2e]/90 to-[#111b2e]/35"/>
                    </div>
                    <div
                        className="mx-auto grid max-w-7xl items-center gap-12 px-5 py-20 md:grid-cols-[1.1fr_0.9fr] md:py-28 lg:px-8">
                        <div>
                            <p className="mb-5 inline-flex items-center gap-2 rounded-full border border-white/15 bg-white/5 px-3 py-1.5 text-xs font-medium text-blue-200">
                                <span className="h-1.5 w-1.5 rounded-full bg-emerald-400"/>
                                Không gian kinh doanh đang sẵn sàng
                            </p>
                            <h1 className="max-w-2xl text-4xl font-semibold leading-[1.12] tracking-tight sm:text-5xl lg:text-6xl">Tìm
                                mặt bằng phù hợp để bắt đầu điều bạn tin tưởng.</h1>
                            <p className="mt-6 max-w-xl text-base leading-7 text-slate-300 sm:text-lg">Khám phá các mặt
                                bằng kinh doanh, xem diện tích, vị trí và tiện ích. Liên hệ ban quản lý để được tư vấn
                                lịch xem và điều kiện thuê.</p>
                            <div className="mt-9 flex flex-wrap gap-3">
                                <a href="#spaces"
                                   className="rounded-xl bg-blue-500 px-5 py-3 text-sm font-semibold text-white shadow-lg shadow-blue-950/30 transition hover:bg-blue-400">Khám
                                    phá mặt bằng <span className="material-symbols-outlined align-middle text-base"
                                                       aria-hidden="true">arrow_downward</span></a>
                                <a href="#contact"
                                   className="rounded-xl border border-white/20 px-5 py-3 text-sm font-semibold text-white transition hover:bg-white/10">Liên
                                    hệ tư vấn</a>
                            </div>
                            <div className="mt-12 flex gap-10 border-t border-white/10 pt-6">
                                <div><p className="text-2xl font-semibold">{availablePremises.length}</p><p
                                    className="mt-1 text-xs text-slate-400">Mặt bằng còn trống</p></div>
                                <div><p className="text-2xl font-semibold">3</p><p
                                    className="mt-1 text-xs text-slate-400">Tầng kinh doanh</p></div>
                                <div><p className="text-lg font-semibold">Trực tiếp</p><p
                                    className="mt-1 text-xs text-slate-400">Tư vấn cùng ban quản lý</p></div>
                            </div>
                        </div>
                        <div className="relative mx-auto w-full max-w-lg">
                            <div className="absolute -inset-5 rounded-4xl bg-blue-400/15 blur-2xl"/>
                            <div
                                className="relative overflow-hidden rounded-[1.75rem] border border-white/15 bg-white/5 p-3 shadow-2xl backdrop-blur-sm">
                                <img src={heroMedia[0]?.premiseMediaImageUrl}
                                     alt={`Mặt bằng ${heroPremise?.name ?? ""}`}
                                     className="h-85 w-full rounded-[1.25rem] object-cover sm:h-105"/>
                                <div
                                    className="absolute inset-x-3 bottom-3 rounded-b-[1.25rem] bg-linear-to-t from-slate-950/90 to-transparent p-6 pt-20">
                                    <p className="text-xs uppercase tracking-[0.18em] text-blue-200">Không gian được
                                        chọn</p>
                                    <p className="mt-2 text-xl font-semibold">Mặt bằng {heroPremise?.name}</p>
                                    <p className="mt-1 text-sm text-slate-300">{heroPremise && `${formatArea(heroPremise.area)} · Tầng ${heroPremise.floor} · ${heroPremise.location}`}</p>
                                </div>
                            </div>
                        </div>
                    </div>
                </section>

                <section id="spaces" className="mx-auto max-w-7xl scroll-mt-24 px-5 py-20 lg:px-8 lg:py-24">
                    <div className="flex flex-col justify-between gap-6 sm:flex-row sm:items-end">
                        <div>
                            <p className="text-xs font-semibold uppercase tracking-[0.2em] text-blue-600">Danh sách mặt
                                bằng</p>
                            <h2 className="mt-3 text-3xl font-semibold tracking-tight text-slate-950">Không gian đang
                                sẵn sàng</h2>
                            <p className="mt-3 max-w-xl text-sm leading-6 text-slate-500">Thông tin cơ bản để bạn chọn
                                nơi phù hợp. Gọi hoặc gửi yêu cầu để xác nhận tình trạng và lịch xem thực tế.</p>
                        </div>
                        <label className="block w-full sm:max-w-xs">
                            <span className="sr-only">Tìm mặt bằng</span>
                            <span className="relative block">
                <span className="pointer-events-none absolute left-4 top-1/2 -translate-y-1/2 text-slate-400"><span
                    className="material-symbols-outlined" aria-hidden="true">search</span></span>
                <input value={query} onChange={(event) => {
                    const value = event.target.value
                    setQuery(value)
                    updateDebouncedQuery(value)
                    setPage(1)
                }}
                       placeholder="Tìm theo tên, địa chỉ..."
                       className="w-full rounded-xl border bg-white py-3 pl-10 pr-4 text-sm outline-none transition focus:border-blue-500 focus:ring-4 focus:ring-blue-500/10"/>
              </span>
                        </label>
                    </div>

                    <div className="flex flex-col items-start justify-between gap-4 sm:flex-row sm:items-center">
                        <div className="mt-8 flex w-fit flex-wrap gap-1 rounded-xl bg-slate-100 p-1">
                            {PREMISE_STATUS_TABS.map((tab) => {
                                const count = tab.key === "all"
                                    ? searchedPremises.length
                                    : searchedPremises.filter((premise) => premise.status === tab.key).length
                                return (
                                    <button
                                        type="button"
                                        key={tab.key}
                                        onClick={() => {
                                            setStatusTab(tab.key)
                                            setPage(1)
                                        }}
                                        className={`rounded-lg px-4 py-2 text-sm font-medium transition ${
                                            statusTab === tab.key
                                                ? "bg-white text-slate-950 shadow-sm"
                                                : "text-slate-500 hover:text-slate-800"
                                        }`}
                                    >
                                        {tab.label}
                                        <span className="ml-1.5 text-slate-400">{count}</span>
                                    </button>
                                )
                            })}
                        </div>

                        {pageCount > 1 && (
                            <nav className="mt-8 flex items-center justify-center gap-1.5"
                                 aria-label="Phân trang mặt bằng">
                                <button
                                    type="button"
                                    onClick={() => setPage(currentPage - 1)}
                                    disabled={currentPage === 1}
                                    aria-label="Trang trước"
                                    className="flex h-9 w-9 items-center justify-center rounded-lg border border-slate-200 bg-white text-slate-600 transition hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-40"
                                >
                                    ‹
                                </button>
                                {Array.from({length: pageCount}, (_, index) => index + 1).map((pageNumber) => (
                                    <button
                                        type="button"
                                        key={pageNumber}
                                        onClick={() => setPage(pageNumber)}
                                        aria-current={pageNumber === currentPage ? "page" : undefined}
                                        className={`h-9 min-w-9 rounded-lg px-3 text-sm font-medium transition ${
                                            pageNumber === currentPage
                                                ? "bg-blue-600 text-white"
                                                : "border border-slate-200 bg-white text-slate-600 hover:bg-slate-50"
                                        }`}
                                    >
                                        {pageNumber}
                                    </button>
                                ))}
                                <button
                                    type="button"
                                    onClick={() => setPage(currentPage + 1)}
                                    disabled={currentPage === pageCount}
                                    aria-label="Trang sau"
                                    className="flex h-9 w-9 items-center justify-center rounded-lg border border-slate-200 bg-white text-slate-600 transition hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-40"
                                >
                                    ›
                                </button>
                            </nav>
                        )}
                    </div>

                    {visiblePremises.length ? (
                        <div className="mt-6 grid gap-6 md:grid-cols-2 xl:grid-cols-3">
                            {pagedPremises.map((premise) => {
                                const media = mockPremiseData.media.filter((item) => item.premiseMediaPremiseId === premise.id)
                                const location = LOCATIONS.find((item) => item.locationId === premise.locationId)
                                const canRequestConsultation = premise.status === "available"
                                return (
                                    <button
                                        type="button"
                                        key={premise.id}
                                        onClick={() => openPremise(premise)}
                                        className={`group flex w-full flex-col overflow-hidden rounded-2xl border border-slate-200 ${premiseStatusSurface[premise.status]} text-left shadow-sm transition hover:-translate-y-1 hover:shadow-xl hover:shadow-slate-900/5 focus-visible:outline-none focus-visible:ring-4 focus-visible:ring-blue-500/20`}
                                    >
                                        <div
                                            className="relative aspect-16/10 w-full shrink-0 overflow-hidden bg-slate-100">
                                            {media[0] ? (
                                                <img
                                                    src={media[0].premiseMediaImageUrl}
                                                    alt={`Mặt bằng ${premise.name}`}
                                                    className="h-full w-full object-cover transition duration-500 group-hover:scale-105"
                                                />
                                            ) : (
                                                <div
                                                    className="flex h-full items-center justify-center text-sm text-slate-400">
                                                    Chưa có hình ảnh
                                                </div>
                                            )}
                                            <span className="absolute left-4 top-4">
                                                <PremiseStatusBadge status={premise.status}/>
                                            </span>
                                            {media.length > 1 && <span
                                                className="absolute bottom-3 right-3 rounded-full bg-slate-950/65 px-2.5 py-1 text-xs text-white">{media.length} ảnh</span>}
                                        </div>
                                        <div className="flex flex-1 flex-col p-5">
                                            <div className="min-h-18">
                                                <h3 className="text-xl font-semibold leading-7 text-slate-950">
                                                    Mặt bằng {premise.name}
                                                </h3>
                                                <p className="mt-1 min-h-10 text-sm leading-5 text-slate-500">
                                                    {location?.address ?? premise.location}
                                                </p>
                                            </div>
                                            <div
                                                className="mt-5 flex min-h-19 gap-5 border-y border-slate-100 py-4 text-sm">
                                                <div><p className="text-xs text-slate-400">Diện tích</p><p
                                                    className="mt-1 font-semibold text-slate-800">{formatArea(premise.area)}</p>
                                                </div>
                                                <div><p className="text-xs text-slate-400">Vị trí</p><p
                                                    className="mt-1 font-semibold text-slate-800">Tầng {premise.floor} ·
                                                    Vị trí {premise.position}</p></div>
                                            </div>
                                            <p className="mt-4 min-h-12 text-sm leading-6 text-slate-600">{premise.description}</p>
                                            <span
                                                className={`mt-auto flex items-center justify-between rounded-xl px-4 py-3 text-sm font-semibold bg-white text-slate-800 transition ${
                                                    canRequestConsultation ? "group-hover:bg-blue-50 group-hover:text-blue-700" : ""
                                                }`}>
                                                {canRequestConsultation ? "Xem chi tiết và liên hệ" : "Xem chi tiết mặt bằng"}
                                                <span className="material-symbols-outlined text-base"
                                                      aria-hidden="true">
                                                    open_in_new
                                                </span>
                                            </span>
                                        </div>
                                    </button>
                                )
                            })}
                        </div>
                    ) : (
                        <div
                            className="mt-9 rounded-2xl border border-dashed border-slate-300 bg-white p-12 text-center text-sm text-slate-500">Không
                            tìm thấy mặt bằng phù hợp.</div>
                    )}
                    <p className="mt-5 text-xs text-slate-400">Tình trạng và thông tin thuê có thể thay đổi. Vui lòng
                        liên hệ ban quản lý để xác nhận.</p>
                </section>

                <section id="about" className="border-y border-slate-100 bg-white scroll-mt-24">
                    <div className="mx-auto grid max-w-7xl gap-12 px-5 py-20 md:grid-cols-2 md:items-center lg:px-8">
                        <div>
                            <p className="text-xs font-semibold uppercase tracking-[0.2em] text-blue-600">ManagerSpace</p>
                            <h2 className="mt-3 text-3xl font-semibold tracking-tight text-slate-950">Tìm nơi kinh doanh
                                phù hợp, bắt đầu thuận lợi.</h2>
                            <p className="mt-5 text-sm leading-7 text-slate-600">Ban quản lý hỗ trợ khách thuê tìm hiểu
                                mặt bằng, sắp xếp lịch tham quan và trao đổi các điều khoản hợp đồng. Tài khoản khách
                                thuê sẽ được cấp trực tiếp sau khi hợp đồng được ký.</p>
                        </div>
                        <div className="grid gap-3 sm:grid-cols-2">
                            {["Thông tin mặt bằng rõ ràng", "Hỗ trợ sắp xếp lịch xem", "Trao đổi trực tiếp với quản lý", "Hợp đồng và tài khoản minh bạch"].map((item, index) => (
                                <div key={item} className="rounded-2xl border border-slate-100 bg-[#f8fafc] p-5"><span
                                    className="flex h-9 w-9 items-center justify-center rounded-xl bg-blue-100 text-sm font-semibold text-blue-700">0{index + 1}</span>
                                    <p className="mt-4 text-sm font-semibold text-slate-800">{item}</p></div>
                            ))}
                        </div>
                    </div>
                </section>

                <section id="contact" className="mx-auto max-w-7xl scroll-mt-24 px-5 py-20 lg:px-8 lg:py-24">
                    <div
                        className="grid gap-10 rounded-4xl bg-[#111b2e] p-6 text-white sm:p-10 lg:grid-cols-[0.9fr_1.1fr] lg:p-14">
                        <div>
                            <p className="text-xs font-semibold uppercase tracking-[0.2em] text-blue-300">Liên hệ</p>
                            <h2 className="mt-3 text-3xl font-semibold tracking-tight">Cùng tìm mặt bằng cho kế hoạch
                                của bạn.</h2>
                            <p className="mt-4 text-sm leading-7 text-slate-300">Để lại thông tin quan tâm, hoặc liên hệ
                                trực tiếp với ban quản lý để được tư vấn.</p>
                            <div className="mt-8 space-y-3 text-sm">
                                {contactPhone && zaloContactUrl && <a className="group -mx-2 flex items-center gap-3 rounded-xl px-2 py-1.5 text-slate-200 transition hover:bg-white/10 hover:text-white focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-300"
                                   href={zaloContactUrl} target="_blank" rel="noreferrer"><span
                                        className="flex h-9 w-9 items-center justify-center rounded-xl bg-white/10"><span
                                        className="material-symbols-outlined"
                                        aria-hidden="true">chat</span></span><span className="flex-1">Zalo: {contactPhone}</span><span className="material-symbols-outlined text-base text-slate-400 transition group-hover:translate-x-0.5 group-hover:text-white" aria-hidden="true">open_in_new</span></a>}
                                {contactEmail ? <a className="group -mx-2 flex items-center gap-3 rounded-xl px-2 py-1.5 text-slate-200 transition hover:bg-white/10 hover:text-white focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-300"
                                                   href={`https://mail.google.com/mail/?view=cm&fs=1&to=${contactEmail}`} target="_blank"><span
                                        className="flex h-9 w-9 items-center justify-center rounded-xl bg-white/10"><span
                                        className="material-symbols-outlined"
                                        aria-hidden="true">mail</span></span><span className="flex-1">{contactEmail}</span><span className="material-symbols-outlined text-base text-slate-400 transition group-hover:translate-x-0.5 group-hover:text-white" aria-hidden="true">open_in_new</span></a> :
                                    <p className="text-xs text-slate-400">Chưa có email liên hệ trong dữ liệu.</p>}
                                {ownerContact?.contactInfoAddress &&
                                    <a className="group -mx-2 flex items-start gap-3 rounded-xl px-2 py-1.5 text-slate-300 transition hover:bg-white/10 hover:text-white focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-300"
                                       href={`https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(ownerContact.contactInfoAddress)}`} target="_blank" rel="noreferrer"><span
                                        className="flex h-9 w-9 shrink-0 items-center justify-center rounded-xl bg-white/10"><span
                                        className="material-symbols-outlined"
                                        aria-hidden="true">location_on</span></span><span
                                        className="flex-1 pt-2">{ownerContact.contactInfoAddress}</span><span className="material-symbols-outlined mt-2 text-base text-slate-400 transition group-hover:translate-x-0.5 group-hover:text-white" aria-hidden="true">open_in_new</span></a>}
                                {ownerContact?.contactSocialLink &&
                                    <a className="group -mx-2 flex items-center gap-3 rounded-xl px-2 py-1.5 text-slate-200 transition hover:bg-white/10 hover:text-white focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-300"
                                       href={ownerContact.contactSocialLink} target="_blank" rel="noreferrer"><span
                                       className="flex h-9 w-9 items-center justify-center rounded-xl bg-white/10"><span
                                        className="material-symbols-outlined" aria-hidden="true">share</span></span><span className="flex-1">Mạng xã hội</span><span className="material-symbols-outlined text-base text-slate-400 transition group-hover:translate-x-0.5 group-hover:text-white" aria-hidden="true">open_in_new</span></a>}
                            </div>
                        </div>
                        <ProcessingForm processing={contactSending} onSubmit={sendInquiry} onChange={() => throttleExtendGoogleLogin()} className="rounded-2xl bg-white p-5 text-slate-900 sm:p-7">
                            <h3 className="text-lg font-semibold">Yêu cầu tư vấn</h3>
                            <p className="mt-1 text-sm text-slate-500">Thông tin sẽ được gửi đến ban quản lý qua
                                email.</p>
                            <div ref={googleButtonRef} className={contactSenderEmail ? "hidden" : "flex mt-4 justify-center"}/>
                            {contactSenderEmail && <div className="flex justify-between">
                                <p className="mt-4 rounded-xl bg-emerald-50 px-4 py-3 text-sm font-medium border-2 border-gradient-success text-emerald-800">Đăng nhập thành công! Phiên kết thúc sau 5 phút.</p>
                                <button onClick={logoutGoogle} className="mt-4 rounded-xl border-gradient-error bg-red-500 px-4 py-3 text-sm font-medium text-white">Đăng xuất</button>
                            </div>}
                            {contactSenderEmail && <p className="mt-2 text-xs font-bold italic text-yellow-500">Email này chỉ được dùng để gửi yêu cầu và trả lời thư.</p>}
                            {!googleIdentityReady && <p className="mt-2 text-xs text-slate-400">Đăng nhập Google chưa khả dụng. Kiểm tra VITE_GOOGLE_CLIENT_ID và kết nối mạng.</p>}
                            <fieldset disabled={!contactSenderEmail || contactSending} className="mt-5 grid gap-4 disabled:opacity-50 sm:grid-cols-2">
                                <label className="block"><span
                                    className="mb-1.5 block text-xs font-medium text-slate-600">Họ tên</span><input
                                    required value={contactName}
                                    onChange={(event) => setContactName(event.target.value)}
                                    className="w-full rounded-xl border px-3.5 py-3 text-sm outline-none focus:border-blue-500 focus:ring-4 focus:ring-blue-500/10"
                                    placeholder="Tên của bạn"/></label>
                                <label className="block"><span
                                    className="mb-1.5 block text-xs font-medium text-slate-600">Số điện thoại</span><input
                                    required type="tel" value={contactPhoneNumber}
                                    onChange={(event) => setContactPhoneNumber(event.target.value)}
                                    className="w-full rounded-xl border px-3.5 py-3 text-sm outline-none focus:border-blue-500 focus:ring-4 focus:ring-blue-500/10"
                                    placeholder="09xx xxx xxx"/></label>
                                <label className="block sm:col-span-2"><span
                                    className="mb-1.5 block text-xs font-medium text-slate-600">Mặt bằng quan tâm</span><select
                                    value={contactPremiseId}
                                    onChange={(event) => setContactPremiseId(event.target.value)}
                                    className="w-full rounded-xl border bg-white px-3.5 py-3 text-sm outline-none focus:border-blue-500 focus:ring-4 focus:ring-blue-500/10">
                                    <option value="">Chưa chọn</option>
                                    {availablePremises.map((premise) => <option key={premise.id}
                                                                                value={premise.id}>{premise.name} · {premise.area} ·
                                        Tầng {premise.floor}</option>)}</select></label>
                                <label className="block sm:col-span-2"><span
                                    className="mb-1.5 block text-xs font-medium text-slate-600">Nội dung cần tư vấn</span><textarea
                                    rows={3} value={contactMessage}
                                    onChange={(event) => setContactMessage(event.target.value)}
                                    className="w-full resize-y rounded-xl border px-3.5 py-3 text-sm outline-none focus:border-blue-500 focus:ring-4 focus:ring-blue-500/10"
                                    placeholder="Thời gian muốn tham quan, mô hình kinh doanh..."/></label>
                            </fieldset>
                            {contactError && <p role="alert"
                                                className="mt-4 rounded-xl bg-amber-50 px-4 py-3 text-xs leading-5 border-2 border-gradient-error text-amber-800">{contactError}</p>}
                            {contactSuccess && <p role="status"
                                                  className="mt-4 rounded-xl bg-emerald-50 px-4 py-3 text-xs leading-5 border-2 border-gradient-success text-emerald-800">{contactSuccess}</p>}
                            <button type="submit" disabled={!contactSenderEmail || contactSending}
                                    className="mt-5 w-full rounded-xl bg-blue-600 px-4 py-3 text-sm font-semibold text-white transition hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-60">
                                {contactSending ? "Đang gửi yêu cầu..." : "Gửi yêu cầu tư vấn"}
                            </button>
                        </ProcessingForm>
                    </div>
                </section>
            </main>

            <footer className="border-t border-slate-200 bg-white">
                <div
                    className="mx-auto flex max-w-7xl flex-col gap-3 px-5 py-6 text-xs text-slate-400 sm:flex-row sm:items-center sm:justify-between lg:px-8">
                    <span>© {new Date().getFullYear()} ManagerSpace · Thông tin mặt bằng</span>
                    <button onClick={onManager}
                            className="inline-flex items-center gap-1 text-left font-medium text-slate-600 hover:text-blue-700">Dành
                        cho ban quản lý <span className="material-symbols-outlined text-base"
                                              aria-hidden="true">arrow_forward</span></button>
                </div>
            </footer>

            {selectedPremise && (
                <Modal
                    title={`Thông tin mặt bằng: ${selectedPremise.name}`}
                    onClose={() => setSelectedPremise(null)}
                    wide
                    compact
                    panelClassName={premiseStatusSurface[selectedPremise.status]}
                >
                    <div className="grid gap-5 lg:grid-cols-2">
                        <div className="min-w-0">
                            <div className="relative overflow-hidden rounded-xl bg-slate-900">
                                <div className="absolute left-3 top-3 z-10">
                                    <PremiseStatusBadge status={selectedPremise.status}/>
                                </div>
                                {selectedPremiseMedia[activeMediaIndex] ? (
                                    <img
                                        src={selectedPremiseMedia[activeMediaIndex].premiseMediaImageUrl}
                                        alt={`Mặt bằng ${selectedPremise.name} - ảnh ${activeMediaIndex + 1}`}
                                        role="button"
                                        tabIndex={0}
                                        onClick={() => setIsImageFullscreen(true)}
                                        onKeyDown={(event) => {
                                            if (event.key === "Enter" || event.key === " ") {
                                                event.preventDefault()
                                                setIsImageFullscreen(true)
                                            }
                                        }}
                                        className="h-48 w-full cursor-zoom-in object-contain"
                                    />
                                ) : (
                                    <div className="flex h-48 items-center justify-center text-sm text-slate-400">Chưa
                                        có hình ảnh</div>
                                )}
                                {selectedPremiseMedia.length > 1 && (
                                    <>
                                        <button
                                            type="button"
                                            aria-label="Ảnh trước"
                                            onClick={() => setActiveMediaIndex((index) => (index - 1 + selectedPremiseMedia.length) % selectedPremiseMedia.length)}
                                            className="absolute left-3 top-1/2 flex h-9 w-9 -translate-y-1/2 items-center justify-center rounded-full bg-slate-950/70 text-2xl leading-none text-white hover:bg-slate-950"
                                        >
                                            ‹
                                        </button>
                                        <button
                                            type="button"
                                            aria-label="Ảnh tiếp theo"
                                            onClick={() => setActiveMediaIndex((index) => (index + 1) % selectedPremiseMedia.length)}
                                            className="absolute right-3 top-1/2 flex h-9 w-9 -translate-y-1/2 items-center justify-center rounded-full bg-slate-950/70 text-2xl leading-none text-white hover:bg-slate-950"
                                        >
                                            ›
                                        </button>
                                        <span
                                            className="absolute bottom-3 right-3 rounded-full bg-slate-950/70 px-2.5 py-1 text-xs font-semibold text-white">
                                            {activeMediaIndex + 1} / {selectedPremiseMedia.length}
                                        </span>
                                    </>
                                )}
                            </div>
                            {selectedPremiseMedia.length > 1 && (
                                <div className="mt-3 flex justify-center gap-1.5">
                                    {selectedPremiseMedia.map((media, index) => (
                                        <button
                                            key={media.premiseMediaId}
                                            type="button"
                                            onClick={() => setActiveMediaIndex(index)}
                                            aria-label={`Xem ảnh ${index + 1}`}
                                            className={`h-1.5 rounded-full transition-all ${index === activeMediaIndex ? "w-6 bg-blue-600" : "w-1.5 bg-slate-300 hover:bg-slate-400"}`}
                                        />
                                    ))}
                                </div>
                            )}
                            <div className="mt-4">
                                <h3 className="text-lg font-semibold text-slate-900">Mặt
                                    bằng {selectedPremise.name}</h3>
                                <p className="mt-1 text-sm text-slate-500">{selectedPremise.location}</p>
                            </div>
                            <div className="mt-4 grid grid-cols-2 gap-3 border-y border-slate-200 py-4 text-sm">
                                <div>
                                    <p className="text-xs text-slate-400">Diện tích</p>
                                    <p className="mt-1 font-semibold text-slate-800">{formatArea(selectedPremise.area)}</p>
                                </div>
                                <div>
                                    <p className="text-xs text-slate-400">Vị trí</p>
                                    <p className="mt-1 font-semibold text-slate-800">Tầng {selectedPremise.floor} · Vị
                                        trí {selectedPremise.position}</p>
                                </div>
                            </div>
                        </div>
                        <div className="min-w-0 rounded-xl bg-white/65 p-5">
                            <p className="text-xs font-semibold uppercase tracking-[0.18em] text-slate-500">Mô tả</p>
                            <p className="mt-3 text-sm leading-6 text-slate-600">{selectedPremise.description}</p>
                            {(selectedPremise.openingHours || selectedPremise.floorArea || selectedPremise.leasableArea || selectedPremise.storeCount || selectedPremise.parkingCapacity) && (
                                <div className="mt-5 space-y-3 border-t border-slate-200 pt-4 text-sm">
                                    {selectedPremise.openingHours &&
                                        <div><p className="text-xs text-slate-400">Giờ hoạt động</p><p
                                            className="mt-1 text-slate-700">{selectedPremise.openingHours}</p></div>}
                                    {selectedPremise.floorArea &&
                                        <div><p className="text-xs text-slate-400">Diện tích mặt sàn</p><p
                                            className="mt-1 text-slate-700">{selectedPremise.floorArea}</p></div>}
                                    {selectedPremise.leasableArea &&
                                        <div><p className="text-xs text-slate-400">Diện tích cho thuê</p><p
                                            className="mt-1 text-slate-700">{selectedPremise.leasableArea}</p></div>}
                                    {selectedPremise.storeCount &&
                                        <div><p className="text-xs text-slate-400">Cửa hàng</p><p
                                            className="mt-1 text-slate-700">{selectedPremise.storeCount}</p></div>}
                                    {selectedPremise.parkingCapacity &&
                                        <div><p className="text-xs text-slate-400">Bãi đỗ xe</p><p
                                            className="mt-1 text-slate-700">{selectedPremise.parkingCapacity}</p></div>}
                                </div>
                            )}
                            {(contactPhone || contactEmail) && (
                                <div className="mt-5 space-y-2 border-t border-slate-200 pt-4 text-sm">
                                    <p className="text-xs font-semibold uppercase tracking-wide text-slate-400">Liên hệ
                                        ban quản lý</p>
                                    {contactPhone && zaloContactUrl && <a href={zaloContactUrl} target="_blank" rel="noreferrer"
                                                        className="group flex items-center gap-2 rounded-lg py-1 text-slate-700 transition hover:text-blue-700 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500"><span
                                        className="material-symbols-outlined text-base"
                                        aria-hidden="true">chat</span>Zalo · {contactPhone}<span className="material-symbols-outlined text-sm opacity-60 transition group-hover:translate-x-0.5" aria-hidden="true">open_in_new</span></a>}
                                    {contactEmail && <a href={`mailto:${contactEmail}`}
                                                        className="group flex items-center gap-2 rounded-lg py-1 text-slate-700 transition hover:text-blue-700 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500"><span
                                        className="material-symbols-outlined text-base"
                                        aria-hidden="true">mail</span>{contactEmail}<span className="material-symbols-outlined text-sm opacity-60 transition group-hover:translate-x-0.5" aria-hidden="true">arrow_forward</span></a>}
                                </div>
                            )}
                        </div>
                        {selectedPremise.status === "available" ?
                            <div className="rounded-xl border border-emerald-200 bg-emerald-100/70 p-4 lg:col-span-2">
                                <p className="text-sm leading-6 text-slate-700">Mặt bằng đang còn trống và có thể tiếp
                                    nhận yêu cầu tư vấn.</p>
                                <button type="button" onClick={requestPremiseConsultation}
                                        className="mt-4 rounded-xl bg-blue-600 px-4 py-3 text-sm font-semibold text-white transition hover:bg-blue-700">
                                    Yêu cầu tư vấn
                                </button>
                            </div> : <div className="rounded-2xl border border-slate-100 bg-slate-50 p-6 lg:col-span-2">
                                <h3 className="text-lg font-semibold text-slate-900">Mặt bằng hiện không nhận tư
                                    vấn</h3>
                                <p className="mt-2 text-sm leading-6 text-slate-600">
                                    Mặt bằng này hiện đang ở trạng
                                    thái <strong>{selectedPremise.status === "rented" ? "Đang thuê" : selectedPremise.status === "reserved" ? "Đã đặt cọc" : "Bảo trì"}</strong>.
                                    Bạn có thể xem thông tin chi tiết, nhưng chưa thể gửi yêu cầu liên hệ cho mặt bằng
                                    này.
                                </p>
                                <div className="flex justify-end">
                                    <button type="button" onClick={() => setSelectedPremise(null)}
                                            className="mt-6 rounded-xl border border-slate-200 bg-white px-4 py-3 text-sm font-medium text-slate-600 transition hover:bg-slate-100">
                                        Đóng
                                    </button>
                                </div>
                            </div>}
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
                        alt={`Mặt bằng ${selectedPremise.name} - ảnh ${activeMediaIndex + 1}`}
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
                                    setActiveMediaIndex((index) => (index - 1 + selectedPremiseMedia.length) % selectedPremiseMedia.length)
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
                            <span
                                className="absolute bottom-4 left-1/2 -translate-x-1/2 rounded-full bg-white/10 px-3 py-1 text-sm text-white">
                                {activeMediaIndex + 1} / {selectedPremiseMedia.length}
                            </span>
                        </>
                    )}
                </div>
            )}
        </div>
    )
}

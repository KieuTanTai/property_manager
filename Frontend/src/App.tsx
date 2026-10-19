import {useEffect, useRef, useState} from "react"
import {identityApi} from "@/api/identity"
import {LoginModal} from "./components/public/LoginModal"
import {PublicHome} from "./components/public/PublicHome"
import {ManagerWorkspace} from "./components/modules/ManagerWorkspace"

const MANAGER_PATH = "/manager"
const SESSION_KEY = "managerspace.session"
const SESSION_IDLE_TIMEOUT_MS = 15 * 60 * 1000
const SESSION_CHECK_INTERVAL_MS = 15 * 1000

type ManagerSession = {
    accountId: string
    email: string
    roleCodes: string[]
    sessionInstanceId: string
    lastActivityAt: number
}

function loadSession(): ManagerSession | null {
    try {
        const stored = sessionStorage.getItem(SESSION_KEY)
        if (!stored) return null
        const session = JSON.parse(stored) as ManagerSession
        if (!session.email || !session.accountId || !session.sessionInstanceId || !session.lastActivityAt) {
            sessionStorage.removeItem(SESSION_KEY)
            return null
        }
        if (Date.now() - session.lastActivityAt >= SESSION_IDLE_TIMEOUT_MS) {
            sessionStorage.removeItem(SESSION_KEY)
            return null
        }
        return session
    } catch {
        return null
    }
}

export default function App() {
    const [path, setPath] = useState(() => window.location.pathname.replace(/\/$/, "") || "/")
    const [session, setSession] = useState<ManagerSession | null>(loadSession)
    const [isLoginOpen, setIsLoginOpen] = useState(false)
    const lastActivityAt = useRef(session?.lastActivityAt ?? 0)

    const navigate = (nextPath: string) => {
        if (window.location.pathname !== nextPath) window.history.pushState({}, "", nextPath)
        setPath(nextPath)
    }

    useEffect(() => {
        const onPopState = () => setPath(window.location.pathname.replace(/\/$/, "") || "/")
        window.addEventListener("popstate", onPopState)
        return () => window.removeEventListener("popstate", onPopState)
    }, [])
    
    useEffect(() => {
        if (session) {
            setIsLoginOpen(false)
            return
        }

        setIsLoginOpen(path === MANAGER_PATH || path === "/login")
    }, [path, session])

    useEffect(() => {
        if (!session) return

        let active = true
        let checkingServer = false
        lastActivityAt.current = session.lastActivityAt

        const endSession = () => {
            if (!active) return
            sessionStorage.removeItem(SESSION_KEY)
            setSession(null)
            navigate("/")
        }

        const checkSession = async () => {
            if (Date.now() - lastActivityAt.current >= SESSION_IDLE_TIMEOUT_MS) {
                endSession()
                return
            }
            if (checkingServer) return
            checkingServer = true
            try {
                const instance = await identityApi.getSessionInstanceId()
                if (active && instance.sessionInstanceId !== session.sessionInstanceId) endSession()
            } catch {
                // A stopped or unreachable Identity service invalidates the local session.
                endSession()
            } finally {
                checkingServer = false
            }
        }

        const onActivity = () => {
            const now = Date.now()
            if (now - lastActivityAt.current < 1000) return
            lastActivityAt.current = now
            setSession((current) => current ? {...current, lastActivityAt: now} : current)
        }

        const activityEvents: (keyof WindowEventMap)[] = ["pointerdown", "pointermove", "keydown", "wheel", "touchstart"]
        activityEvents.forEach((eventName) => window.addEventListener(eventName, onActivity, {passive: true}))
        document.addEventListener("visibilitychange", checkSession)
        const timer = window.setInterval(() => void checkSession(), SESSION_CHECK_INTERVAL_MS)
        void checkSession()

        return () => {
            active = false
            window.clearInterval(timer)
            activityEvents.forEach((eventName) => window.removeEventListener(eventName, onActivity))
            document.removeEventListener("visibilitychange", checkSession)
        }
    }, [session?.sessionInstanceId])

    useEffect(() => {
        if (session) sessionStorage.setItem(SESSION_KEY, JSON.stringify(session))
    }, [session])

    const openManager = () => {
        navigate(MANAGER_PATH)
        if (!session) setIsLoginOpen(true)
    }

    const closeLogin = () => {
        setIsLoginOpen(false)
        if (path === MANAGER_PATH || path === "/login") {
            sessionStorage.removeItem(SESSION_KEY)
            setSession(null)
            navigate("/")
        }
    }

    const handleLogin = (account: ManagerSession) => {
        lastActivityAt.current = account.lastActivityAt
        sessionStorage.setItem(SESSION_KEY, JSON.stringify(account))
        setSession(account)
        setIsLoginOpen(false)
        navigate(MANAGER_PATH)
    }

    const handleLogout = () => {
        sessionStorage.removeItem(SESSION_KEY)
        setSession(null)
        navigate("/")
    }

    const isManagerRoute = path === MANAGER_PATH
    const isKnownPublicRoute = path === "/" || path === "/login"

    return (
        <>
            {isManagerRoute && session ? (
                <ManagerWorkspace accountId={session.accountId} email={session.email} roleCodes={session.roleCodes}
                                  onLogout={handleLogout}/>
            ) : (
                <PublicHome onManager={openManager}/>
            )}

            {isLoginOpen && (
                <LoginModal onClose={closeLogin} onLogin={handleLogin}/>
            )}

            {!isKnownPublicRoute && !isManagerRoute && (
                <div
                    className="fixed bottom-5 right-5 z-50 rounded-xl bg-slate-900 px-4 py-3 text-sm text-white shadow-xl">
                    Đường dẫn không tồn tại. <button onClick={() => navigate("/")}
                                                     className="ml-1 font-semibold underline">Về trang chủ</button>
                </div>
            )}
        </>
    )
}

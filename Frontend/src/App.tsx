import { useEffect, useState } from "react"
import { LoginModal } from "./components/public/LoginModal"
import { PublicHome } from "./components/public/PublicHome"
import { ManagerWorkspace } from "./components/modules/ManagerWorkspace"

const MANAGER_PATH = "/manager"
const SESSION_KEY = "managerspace.session"

type ManagerSession = { accountId: string; email: string; roleCodes: string[] }

function loadSession(): ManagerSession | null {
  try {
    const stored = sessionStorage.getItem(SESSION_KEY)
    if (!stored) return null
    const session = JSON.parse(stored) as ManagerSession
    return session.email && session.accountId ? session : null
  } catch {
    return null
  }
}

export default function App() {
  const [path, setPath] = useState(() => window.location.pathname.replace(/\/$/, "") || "/")
  const [session, setSession] = useState<ManagerSession | null>(loadSession)
  const [isLoginOpen, setIsLoginOpen] = useState(() => window.location.pathname === MANAGER_PATH || window.location.pathname === "/login")

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
    if (path === MANAGER_PATH && !session) setIsLoginOpen(true)
    if (path === "/login") setIsLoginOpen(true)
    if (path !== MANAGER_PATH && path !== "/login") setIsLoginOpen(false)
  }, [path, session])

  const openManager = () => {
    navigate(MANAGER_PATH)
    if (!session) setIsLoginOpen(true)
  }

  const closeLogin = () => {
    setIsLoginOpen(false)
    if (!session && (path === MANAGER_PATH || path === "/login")) navigate("/")
  }

  const handleLogin = (account: ManagerSession) => {
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
        <ManagerWorkspace accountId={session.accountId} email={session.email} roleCodes={session.roleCodes} onLogout={handleLogout} />
      ) : (
        <PublicHome onLogin={() => setIsLoginOpen(true)} onManager={openManager} />
      )}

      {isLoginOpen && (
        <LoginModal onClose={closeLogin} onLogin={handleLogin} />
      )}

      {!isKnownPublicRoute && !isManagerRoute && (
        <div className="fixed bottom-5 right-5 z-50 rounded-xl bg-slate-900 px-4 py-3 text-sm text-white shadow-xl">
          Đường dẫn không tồn tại. <button onClick={() => navigate("/")} className="ml-1 font-semibold underline">Về trang chủ</button>
        </div>
      )}
    </>
  )
}

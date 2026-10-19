import {PREMISE_MEDIA, PREMISES} from "./shared"
import type {Premise, PremiseMedia} from "@/components/premises/types"

const STORAGE_KEY = "manager-space.mock.created-premises"
const CHANGE_EVENT = "manager-space:mock-premises-changed"

type StoredPremises = {
    premises: Premise[]
    media: PremiseMedia[]
}

export type MockPremiseData = {
    premises: Premise[]
    media: PremiseMedia[]
}

function readStoredPremises(): StoredPremises {
    try {
        const serialized = localStorage.getItem(STORAGE_KEY)
        if (!serialized) return {premises: [], media: []}
        const parsed = JSON.parse(serialized) as Partial<StoredPremises>
        return {
            premises: Array.isArray(parsed.premises) ? parsed.premises : [],
            media: Array.isArray(parsed.media) ? parsed.media : [],
        }
    } catch {
        return {premises: [], media: []}
    }
}

export function getMockPremiseData(): MockPremiseData {
    const stored = readStoredPremises()
    const premises = new Map(PREMISES.map((premise) => [premise.id, premise]))
    const media = new Map(PREMISE_MEDIA.map((item) => [item.premiseMediaId, item]))
    stored.premises.forEach((premise) => premises.set(premise.id, premise))
    stored.media.forEach((item) => media.set(item.premiseMediaId, item))
    return {premises: [...premises.values()], media: [...media.values()]}
}

/** Stores mock-created premise data in browser storage until the real API exists. */
export function saveMockPremise(premise: Premise, media: PremiseMedia[] = []) {
    const stored = readStoredPremises()
    const premisesById = new Map(stored.premises.map((item) => [item.id, item]))
    const mediaById = new Map(stored.media.map((item) => [item.premiseMediaId, item]))
    premisesById.set(premise.id, premise)
    media.forEach((item) => mediaById.set(item.premiseMediaId, item))
    localStorage.setItem(STORAGE_KEY, JSON.stringify({
        premises: [...premisesById.values()],
        media: [...mediaById.values()],
    } satisfies StoredPremises))
    window.dispatchEvent(new Event(CHANGE_EVENT))
}

export function subscribeToMockPremiseChanges(onChange: () => void) {
    const handleStorageChange = (event: StorageEvent) => {
        if (event.key === STORAGE_KEY) onChange()
    }
    window.addEventListener("storage", handleStorageChange)
    window.addEventListener(CHANGE_EVENT, onChange)
    return () => {
        window.removeEventListener("storage", handleStorageChange)
        window.removeEventListener(CHANGE_EVENT, onChange)
    }
}

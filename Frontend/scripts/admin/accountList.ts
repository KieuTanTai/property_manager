interface AccountListElements {
    search: HTMLInputElement;
    summary: HTMLElement;
    pagination: HTMLElement;
    empty: HTMLElement;
    rows: HTMLElement[];
}

function getAccountListElements(root: HTMLElement): AccountListElements | null {
    const search = root.querySelector<HTMLInputElement>("[data-account-search]");
    const summary = root.querySelector<HTMLElement>("[data-account-summary]");
    const pagination = root.querySelector<HTMLElement>("[data-account-pagination]");
    const empty = root.querySelector<HTMLElement>("[data-account-empty]");
    const rows = Array.from(root.querySelectorAll<HTMLElement>("[data-account-row]"));

    if (!search || !summary || !pagination || !empty) {
        return null;
    }

    return {search, summary, pagination, empty, rows};
}

function renderAccountList(
    elements: AccountListElements,
    currentPage: number,
    pageSize: number
): number {
    const query = elements.search.value.trim().toLowerCase();
    const filteredRows = elements.rows.filter((row: HTMLElement) =>
        (row.dataset.email ?? "").toLowerCase().includes(query));
    const pageCount = Math.max(1, Math.ceil(filteredRows.length / pageSize));
    const page = Math.min(currentPage, pageCount);
    const firstIndex = (page - 1) * pageSize;

    elements.rows.forEach((row: HTMLElement) => {
        row.hidden = true;
    });
    filteredRows.slice(firstIndex, firstIndex + pageSize).forEach((row: HTMLElement) => {
        row.hidden = false;
    });

    elements.empty.hidden = filteredRows.length > 0;
    elements.summary.textContent = filteredRows.length
        ? `Showing ${firstIndex + 1}-${Math.min(firstIndex + pageSize, filteredRows.length)} of ${filteredRows.length}`
        : "Showing 0 of 0";

    elements.pagination.innerHTML = "";
    if (pageCount <= 1) {
        return page;
    }

    for (let pageNumber = 1; pageNumber <= pageCount; pageNumber += 1) {
        const button = document.createElement("button");
        button.type = "button";
        button.className = `account-page-button${pageNumber === page ? " is-current" : ""}`;
        button.textContent = String(pageNumber);
        button.setAttribute("aria-label", `Go to page ${pageNumber}`);
        button.setAttribute("aria-current", pageNumber === page ? "page" : "false");
        button.addEventListener("click", () => {
            renderAccountList(elements, pageNumber, pageSize);
        });
        elements.pagination.append(button);
    }

    return page;
}

document.addEventListener("DOMContentLoaded", () => {
    const root = document.querySelector<HTMLElement>("[data-account-list]");
    if (!root) {
        return;
    }

    const elements = getAccountListElements(root);
    if (!elements) {
        return;
    }

    const pageSize = 5;
    let currentPage = 1;

    elements.search.addEventListener("input", () => {
        currentPage = renderAccountList(elements, 1, pageSize);
    });

    currentPage = renderAccountList(elements, currentPage, pageSize);
});

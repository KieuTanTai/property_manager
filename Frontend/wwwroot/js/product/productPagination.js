"use strict";
function getProductListElements() {
    const cards = Array.from(document.querySelectorAll("[data-product-card]"));
    const pagination = document.querySelector("[data-product-pagination]");
    if (!pagination || cards.length === 0) {
        return null;
    }
    return { cards, pagination };
}
function renderProductPagination(elements, currentPage, pageSize) {
    const pageCount = Math.ceil(elements.cards.length / pageSize);
    const page = Math.min(Math.max(currentPage, 1), pageCount);
    const firstIndex = (page - 1) * pageSize;
    elements.cards.forEach((card, index) => {
        card.hidden = index < firstIndex || index >= firstIndex + pageSize;
    });
    elements.pagination.innerHTML = "";
    for (let pageNumber = 1; pageNumber <= pageCount; pageNumber += 1) {
        const button = document.createElement("button");
        button.type = "button";
        button.className = `product-page-button${pageNumber === page ? " is-current" : ""}`;
        button.textContent = String(pageNumber);
        button.setAttribute("aria-label", `Go to product page ${pageNumber}`);
        button.setAttribute("aria-current", pageNumber === page ? "page" : "false");
        button.addEventListener("click", () => {
            renderProductPagination(elements, pageNumber, pageSize);
        });
        elements.pagination.append(button);
    }
}
document.addEventListener("DOMContentLoaded", () => {
    const elements = getProductListElements();
    if (!elements) {
        return;
    }
    renderProductPagination(elements, 1, 8);
});
//# sourceMappingURL=productPagination.js.map
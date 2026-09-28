import { initializeAccountMenu } from "./account/accountMenu.js";
import { initializeAuthOverlay } from "./auth/authOverlay.js";
import { initializeCartBadge } from "./cart/cart.js";
import { initializeProductOverlay } from "./product/productOverlay.js";
import "./product/productPagination.js";
import { initializeSidebar } from "./sidebar/sidebar.js";
document.addEventListener("DOMContentLoaded", () => {
    initializeAccountMenu();
    initializeCartBadge();
    initializeSidebar();
    initializeAuthOverlay();
    initializeProductOverlay();
});
//# sourceMappingURL=site.js.map
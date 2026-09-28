import { initializeAccountMenu } from "./account/accountMenu.js";
import { initializeAuthOverlay } from "./auth/authOverlay.js";
import { initializeCartBadge } from "./cart/cart.js";
import { initializeProductOverlay } from "./product/productOverlay.js";
import { initializeSidebar } from "./sidebar/sidebar.js";
import { initializeAccountList } from "./admin/accountList.js";
document.addEventListener("DOMContentLoaded", () => {
    initializeAccountMenu();
    initializeCartBadge();
    initializeSidebar();
    initializeAuthOverlay();
    initializeProductOverlay();
    //     ADMIN
    initializeAccountList();
});
//# sourceMappingURL=site.js.map
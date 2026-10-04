"use strict";
const dashboard = document.querySelector('[data-manager-dashboard]');
dashboard?.querySelector('[data-dashboard-refresh]')?.addEventListener('click', (event) => {
    const button = event.currentTarget;
    button.disabled = true;
    button.textContent = 'Đã cập nhật';
    window.setTimeout(() => {
        button.disabled = false;
        button.textContent = 'Làm mới';
    }, 1000);
});
dashboard?.querySelector('[data-dashboard-activity]')?.addEventListener('click', () => {
    window.alert('Danh sách hoạt động đầy đủ sẽ được triển khai cùng module nhật ký.');
});
//# sourceMappingURL=dashboard.js.map
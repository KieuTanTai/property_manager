"use strict";
const modal = document.querySelector('[data-manager-modal]');
const modalTitle = modal?.querySelector('[data-manager-modal-title]');
const modalContent = modal?.querySelector('[data-manager-modal-content]');
const closeModal = () => {
    if (!modal)
        return;
    modal.hidden = true;
    modal.setAttribute('aria-hidden', 'true');
    document.body.classList.remove('manager-modal-open');
};
const renderModalContent = (form, message) => {
    if (form === 'report')
        return `<form class="manager-form" data-manager-form><label>Nhóm báo cáo<select><option>Tài chính</option><option>Tài khoản</option><option>Mặt bằng</option><option>Hợp đồng</option><option>Vi phạm</option></select></label><label>Khoảng thời gian<input type="month" value="2026-10"></label><button type="submit" class="admin-primary-button">Tạo báo cáo</button></form>`;
    if (form === 'premise')
        return `<form class="manager-form" data-manager-form><label>Tên mặt bằng<input required placeholder="A03"></label><label>Diện tích<input required placeholder="45m²"></label><label>Địa chỉ<input required placeholder="Địa chỉ mặt bằng"></label><label>Trạng thái<select><option>Còn trống</option><option>Đang thuê</option><option>Đã đặt cọc</option><option>Bảo trì</option></select></label><button type="submit" class="admin-primary-button">Lưu mặt bằng</button></form>`;
    if (form === 'contract')
        return `<form class="manager-form" data-manager-form><label>Khách thuê<input required placeholder="Tên khách thuê"></label><label>Mặt bằng<input required placeholder="A01"></label><label>Giá thuê<input required type="number" min="0"></label><label>Ngày kết thúc<input required type="date"></label><button type="submit" class="admin-primary-button">Lưu hợp đồng</button></form>`;
    if (form === 'invoice')
        return `<form class="manager-form" data-manager-form><label>Mã hợp đồng<input required placeholder="CTR-2026-001"></label><label>Số tiền<input required type="number" min="0"></label><label>Hạn thanh toán<input required type="date"></label><button type="submit" class="admin-primary-button">Lưu hóa đơn</button></form>`;
    if (form === 'ticket')
        return `<form class="manager-form" data-manager-form><label>Tài khoản<input required placeholder="Tài khoản khách thuê"></label><label>Nội dung<textarea required rows="3"></textarea></label><label>Loại<select><option>Khiếu nại</option><option>Vi phạm</option><option>Phản hồi</option><option>Đánh giá</option></select></label><button type="submit" class="admin-primary-button">Tạo yêu cầu</button></form>`;
    if (form === 'quick-action')
        return `<div class="manager-quick-form"><p>Chọn loại dữ liệu cần tạo mới:</p><button type="button" data-manager-modal-open data-modal-title="Thêm mặt bằng" data-modal-form="premise">Mặt bằng</button><button type="button" data-manager-modal-open data-modal-title="Tạo hợp đồng" data-modal-form="contract">Hợp đồng</button><button type="button" data-manager-modal-open data-modal-title="Tạo hóa đơn" data-modal-form="invoice">Hóa đơn</button></div>`;
    return `<p>${message ?? 'Chức năng đang được chuẩn bị.'}</p>`;
};
document.addEventListener('click', (event) => {
    const target = event.target;
    if (!(target instanceof Element))
        return;
    const button = target.closest('[data-manager-modal-open]');
    if (!button || !modal || !modalTitle || !modalContent)
        return;
    modalTitle.textContent = button.dataset.modalTitle ?? 'Thông báo';
    modalContent.innerHTML = renderModalContent(button.dataset.modalForm, button.dataset.modalMessage);
    modal.hidden = false;
    modal.setAttribute('aria-hidden', 'false');
    document.body.classList.add('manager-modal-open');
    modal?.addEventListener('submit', (event) => {
        const form = event.target;
        if (!(form instanceof HTMLFormElement))
            return;
        event.preventDefault();
        const submit = form.querySelector('button[type="submit"]');
        if (submit)
            submit.textContent = 'Đã lưu';
    });
});
modal?.querySelectorAll('[data-manager-modal-close]').forEach((element) => {
    element.addEventListener('click', closeModal);
});
document.addEventListener('keydown', (event) => {
    if (event.key === 'Escape')
        closeModal();
});
//# sourceMappingURL=modal.js.map
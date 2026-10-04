const dashboard = document.querySelector<HTMLElement>('[data-manager-dashboard]')

dashboard?.querySelector<HTMLButtonElement>('[data-dashboard-refresh]')?.addEventListener('click', (event) => {
    const button = event.currentTarget as HTMLButtonElement
    button.disabled = true
    button.textContent = 'Đã cập nhật'
    window.setTimeout(() => {
        button.disabled = false
        button.textContent = 'Làm mới'
    }, 1000)
})

dashboard?.querySelector<HTMLButtonElement>('[data-dashboard-activity]')?.addEventListener('click', () => {
    window.alert('Danh sách hoạt động đầy đủ sẽ được triển khai cùng module nhật ký.')
})

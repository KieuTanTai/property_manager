const statisticsPage = document.querySelector<HTMLElement>('[data-statistics-page]')

statisticsPage?.querySelectorAll<HTMLButtonElement>('[data-statistics-tab]').forEach((tab) => {
    tab.addEventListener('click', () => {
        const selected = tab.dataset.statisticsTab
        if (!selected) return
        statisticsPage.querySelectorAll<HTMLButtonElement>('[data-statistics-tab]').forEach((item) => item.classList.toggle('is-active', item === tab))
        statisticsPage.querySelectorAll<HTMLElement>('[data-statistics-content]').forEach((content) => {
            content.hidden = content.dataset.statisticsContent !== selected
        })
    })
})

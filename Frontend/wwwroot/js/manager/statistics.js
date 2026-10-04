"use strict";
const statisticsPage = document.querySelector('[data-statistics-page]');
statisticsPage?.querySelectorAll('[data-statistics-tab]').forEach((tab) => {
    tab.addEventListener('click', () => {
        const selected = tab.dataset.statisticsTab;
        if (!selected)
            return;
        statisticsPage.querySelectorAll('[data-statistics-tab]').forEach((item) => item.classList.toggle('is-active', item === tab));
        statisticsPage.querySelectorAll('[data-statistics-content]').forEach((content) => {
            content.hidden = content.dataset.statisticsContent !== selected;
        });
    });
});
//# sourceMappingURL=statistics.js.map
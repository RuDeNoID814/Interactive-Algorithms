// =========================================
// ВСПОМОГАТЕЛЬНЫЕ ФУНКЦИИ ДЕМОНСТРАЦИИ СОРТИРОВКИ
// =========================================

window.algoLabSortDemo = {

    // Прокручивает список лога так, чтобы текущий шаг был виден.
    // Вызывается из Blazor после каждого шага показа.
    scrollCurrent: function (container) {
        if (!container) return;

        const row = container.querySelector('[data-current="true"]');
        if (!row) return;

        // Плавная прокрутка только если строка вне видимой области,
        // чтобы не дёргать список на каждом шаге.
        const rowTop = row.offsetTop;
        const rowBottom = rowTop + row.offsetHeight;
        const viewTop = container.scrollTop;
        const viewBottom = viewTop + container.clientHeight;

        if (rowTop < viewTop || rowBottom > viewBottom) {
            container.scrollTo({
                top: rowTop - container.clientHeight / 2 + row.offsetHeight / 2,
                behavior: 'smooth'
            });
        }
    },

    // Отдаёт пользователю текстовый файл: внешняя сортировка обещает «записать
    // результат в результирующий файл», и его нужно уметь забрать.
    downloadTextFile: function (fileName, text) {
        const blob = new Blob([text], { type: 'text/plain;charset=utf-8' });
        const url = URL.createObjectURL(blob);

        const link = document.createElement('a');
        link.href = url;
        link.download = fileName;
        document.body.appendChild(link);
        link.click();

        document.body.removeChild(link);
        URL.revokeObjectURL(url);
    }
};

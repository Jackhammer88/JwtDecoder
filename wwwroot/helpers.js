window.getSelectionRect = function () {
    const sel = window.getSelection();
    if (!sel.rangeCount) return null;
    const range = sel.getRangeAt(0);
    const rect = range.getBoundingClientRect();
    // Возвращаем координаты с учётом прокрутки
    return {
        top: rect.top + window.scrollY,
        left: rect.left + window.scrollX,
        width: rect.width,
        height: rect.height
    };
};

window.registerPreCodeSelectionHandler = function (elementId, dotNetHelper) {
    const el = document.getElementById(elementId);
    if (!el) return;

    el.addEventListener('mouseup', function () {
        // Если выделение есть, получаем его текст и координаты
        const selection = window.getSelection().toString();
        if (selection) {
            const rect = window.getSelectionRect();
            // Передаём в Blazor: выделенный текст и координаты
            dotNetHelper.invokeMethodAsync('OnTextSelected', selection, rect);
        }
    });
};

window.copyToClipboard = (text) => {
    navigator.clipboard.writeText(text)
        .then(() => {
            console.log('Copied to clipboard');
        })
        .catch(err => {
            console.error('Error copying to clipboard', err);
        });
};
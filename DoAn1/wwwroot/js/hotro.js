// wwwroot/js/hotro.js
document.addEventListener("DOMContentLoaded", function () {
    const guideOpenBtns = document.querySelectorAll('.js-open-guide');
    const guideOverlay = document.getElementById('hdOverlay');
    const guideCloseBtns = document.querySelectorAll('.js-close-guide');

    function openGuide() {
        if (!guideOverlay) return;
        guideOverlay.style.display = 'block';
        document.body.style.overflow = 'hidden';
        const panel = guideOverlay.querySelector('.hd-panel');
        if (panel) panel.focus();
    }
    function closeGuide() {
        if (!guideOverlay) return;
        guideOverlay.style.display = 'none';
        document.body.style.overflow = '';
    }

    guideOpenBtns.forEach(btn => btn.addEventListener('click', openGuide));
    guideCloseBtns.forEach(btn => btn.addEventListener('click', closeGuide));

    if (guideOverlay) {
        guideOverlay.addEventListener('click', function (e) {
            if (e.target === guideOverlay) closeGuide();
        });
    }

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') closeGuide();
    });

    // scroll khi click mục lục
    const anchors = document.querySelectorAll('.hd-panel .hd-sidebar li[data-target]');
    anchors.forEach(item => {
        item.addEventListener('click', function () {
            const target = item.getAttribute('data-target');
            const el = document.querySelector(target);
            if (el) el.scrollIntoView({ behavior: 'smooth', block: 'start' });
        });
    });
});
// Liên hệ
document.querySelector('.js-open-contact').addEventListener('click', function () {
    document.getElementById('contactOverlay').style.display = 'block';
});

document.querySelector('.js-close-contact').addEventListener('click', function () {
    document.getElementById('contactOverlay').style.display = 'none';
});

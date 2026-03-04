// Hamburger menu toggle — portrait mobile only
(function () {
    const btn = document.getElementById('hamburgerBtn');
    const nav = document.getElementById('navActions');

    if (!btn || !nav) return;

    btn.addEventListener('click', function () {
        const isOpen = nav.classList.toggle('is-open');
        btn.classList.toggle('is-open', isOpen);
        btn.setAttribute('aria-expanded', isOpen);
    });

    // Close menu if window rotates to landscape
    window.addEventListener('orientationchange', function () {
        nav.classList.remove('is-open');
        btn.classList.remove('is-open');
        btn.setAttribute('aria-expanded', 'false');
    });
})();
// Hash-based page navigation.
// One delegated click handler, and history entries are only pushed on a user click,
// so Back and Forward behave normally.
document.addEventListener('DOMContentLoaded', () => {
    const pages = document.querySelectorAll('.page');
    const navLinks = document.querySelectorAll('.nav-link');
    const DEFAULT_PAGE = '#home';

    const isPage = (hash) => Boolean(hash) && hash !== '#' && document.querySelector(`.page${hash}`) !== null;

    function render(hash) {
        const target = isPage(hash) ? hash : DEFAULT_PAGE;

        pages.forEach((page) => page.classList.toggle('active', `#${page.id}` === target));

        navLinks.forEach((link) => {
            const active = link.getAttribute('href') === target;
            link.classList.toggle('active', active);
            if (active) {
                link.setAttribute('aria-current', 'page');
                link.scrollIntoView({ block: 'nearest', inline: 'nearest' });
            } else {
                link.removeAttribute('aria-current');
            }
        });

        return target;
    }

    document.addEventListener('click', (e) => {
        const link = e.target.closest('a[href^="#"]');
        if (!link) return;

        const hash = link.getAttribute('href');
        if (!isPage(hash)) return; // e.g. the skip link: let the browser handle it

        e.preventDefault();
        if (hash !== window.location.hash) {
            history.pushState(null, '', hash);
        }
        render(hash);
        const reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
        window.scrollTo({ top: 0, behavior: reduceMotion ? 'auto' : 'smooth' });
    });

    // Back / Forward: re-render only, never push a new entry.
    window.addEventListener('popstate', () => render(window.location.hash));

    // First load: normalise the URL without adding a history entry.
    const initial = render(window.location.hash);
    if (window.location.hash && window.location.hash !== initial) {
        history.replaceState(null, '', initial);
    }
});

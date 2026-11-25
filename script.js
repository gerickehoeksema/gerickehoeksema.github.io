// Hash-based page navigation.
// One delegated click handler, and history entries are only pushed on a user click,
// so Back and Forward behave normally.
document.addEventListener('DOMContentLoaded', () => {
    const pages = document.querySelectorAll('.page');
    const navLinks = document.querySelectorAll('.nav-link');
    const DEFAULT_PAGE = '#home';
    if (pages.length === 0) return; // standalone pages (articles) have no hash sections

    const isPage = (hash) => Boolean(hash) && hash !== '#' && document.querySelector(`.page${hash}`) !== null;
    const baseTitle = document.title;
    const status = document.getElementById('page-status');

    // Screen readers don't notice a hash page switch on their own. Give the new page its own
    // tab title, move focus to its first heading (so reading starts there), and announce it.
    function announce(target) {
        const page = document.querySelector(`.page${target}`);
        const name = page.getAttribute('aria-label') || '';
        document.title = target === DEFAULT_PAGE || !name ? baseTitle : `${name} · ${baseTitle}`;

        const heading = page.querySelector('h1, h2');
        if (heading) {
            heading.setAttribute('tabindex', '-1');
            heading.focus({ preventScroll: true });
        }
        if (status) {
            status.textContent = '';
            // A fresh text node after a tick, so the same page announced twice is still read.
            setTimeout(() => { status.textContent = `${name} page`; }, 50);
        }
    }

    function render(hash) {
        const target = isPage(hash) ? hash : DEFAULT_PAGE;

        pages.forEach((page) => {
            const active = `#${page.id}` === target;
            page.classList.toggle('active', active);
            // Belt and braces: inactive pages are display:none, but hide them from assistive tech explicitly too.
            if (active) page.removeAttribute('aria-hidden'); else page.setAttribute('aria-hidden', 'true');
        });

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
        const target = render(hash);
        const reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
        window.scrollTo({ top: 0, behavior: reduceMotion ? 'auto' : 'smooth' });
        announce(target);
    });

    // Back / Forward: re-render only, never push a new entry.
    window.addEventListener('popstate', () => {
        const target = render(window.location.hash);
        window.scrollTo({ top: 0 });
        announce(target);
    });

    // First load: normalise the URL without adding a history entry. Focus is left alone,
    // so the page reads from the top, but a deep link (/#projects) still gets its own title.
    const initial = render(window.location.hash);
    const initialName = document.querySelector(`.page${initial}`).getAttribute('aria-label');
    if (initial !== DEFAULT_PAGE && initialName) document.title = `${initialName} · ${baseTitle}`;
    if (window.location.hash && window.location.hash !== initial) {
        history.replaceState(null, '', initial);
    }
});

// Light / dark theme toggle.
// The OS preference is the default; a click stores an explicit choice on <html data-theme>
// and in localStorage. The inline script in <head> re-applies it before first paint.
document.addEventListener('DOMContentLoaded', () => {
    const root = document.documentElement;
    const toggle = document.getElementById('theme-toggle');
    const prefersLight = window.matchMedia('(prefers-color-scheme: light)');
    if (!toggle) return;

    const current = () => root.dataset.theme || (prefersLight.matches ? 'light' : 'dark');

    function sync() {
        const next = current() === 'dark' ? 'light' : 'dark';
        toggle.setAttribute('aria-label', `Switch to ${next} theme`);
        toggle.title = `Switch to ${next} theme`;
        const bg = getComputedStyle(document.body).backgroundColor;
        document.querySelectorAll('meta[name="theme-color"]').forEach((m) => m.setAttribute('content', bg));
    }

    toggle.addEventListener('click', () => {
        const next = current() === 'dark' ? 'light' : 'dark';
        root.dataset.theme = next;
        try { localStorage.setItem('theme', next); } catch (e) { /* storage blocked: theme lasts for this page view */ }
        sync();
    });

    prefersLight.addEventListener('change', sync);
    sync();
});

// "Copy link" buttons on articles: copies the canonical URL.
document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('[data-copy-link]').forEach((button) => {
        const label = button.querySelector('span');
        button.addEventListener('click', async () => {
            const url = document.querySelector('link[rel="canonical"]')?.href || window.location.href;
            try {
                await navigator.clipboard.writeText(url);
                label.textContent = 'Link copied';
            } catch (e) {
                window.prompt('Copy this link:', url);
                return;
            }
            setTimeout(() => { label.textContent = 'Copy link'; }, 2000);
        });
    });
});

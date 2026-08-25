// Small, dependency-free helpers that Blazor calls over JS interop.
window.portfolio = (function () {
    const THEME_KEY = 'portfolio-theme';
    let revealObserver = null;
    let scrollSpyObserver = null;
    let dotNetRef = null;
    let warmedUp = false;

    function safeStorage(fn, fallback) {
        try { return fn(); } catch (e) { return fallback; }
    }

    function getTheme() {
        return document.documentElement.getAttribute('data-theme') || 'dark';
    }

    function setTheme(theme) {
        document.documentElement.setAttribute('data-theme', theme);
        const meta = document.querySelector('meta[name="theme-color"]');
        if (meta) meta.setAttribute('content', theme === 'light' ? '#f7f8fa' : '#08090d');
        safeStorage(() => localStorage.setItem(THEME_KEY, theme));
        return theme;
    }

    function prefersReducedMotion() {
        return window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    }

    // Reveals elements marked with [data-reveal] as they scroll into view.
    // Called after each render pass, so it must be safe to call repeatedly.
    function observeReveals() {
        const targets = document.querySelectorAll('[data-reveal]:not(.is-revealed)');
        if (!targets.length) return;

        if (prefersReducedMotion()) {
            targets.forEach(el => el.classList.add('is-revealed'));
            return;
        }

        if (!revealObserver) {
            revealObserver = new IntersectionObserver((entries) => {
                entries.forEach(entry => {
                    if (!entry.isIntersecting) return;
                    entry.target.classList.add('is-revealed');
                    revealObserver.unobserve(entry.target);
                });
            }, { rootMargin: '0px 0px -12% 0px', threshold: 0.08 });
        }

        targets.forEach(el => revealObserver.observe(el));
    }

    // Registers the .NET callback that receives the section currently in view.
    function observeSections(ref) {
        dotNetRef = ref;
        return refreshSections();
    }

    // Re-scans for sections. Called after every render pass because the sections only
    // exist once the resume data has arrived.
    function refreshSections() {
        if (!dotNetRef) return false;
        if (scrollSpyObserver) scrollSpyObserver.disconnect();

        const sections = document.querySelectorAll('section[id]');
        if (!sections.length) return false;

        scrollSpyObserver = new IntersectionObserver((entries) => {
            const visible = entries
                .filter(e => e.isIntersecting)
                .sort((a, b) => b.intersectionRatio - a.intersectionRatio)[0];
            if (visible && dotNetRef) {
                dotNetRef.invokeMethodAsync('OnSectionChanged', visible.target.id);
            }
        }, { rootMargin: '-45% 0px -45% 0px', threshold: [0, 0.25, 0.5, 1] });

        sections.forEach(s => scrollSpyObserver.observe(s));
        return true;
    }

    function scrollToSection(id) {
        const el = document.getElementById(id);
        if (!el) return;
        el.scrollIntoView({ behavior: prefersReducedMotion() ? 'auto' : 'smooth', block: 'start' });
    }

    // The API sleeps on a free tier and takes ~50s to wake. Ping it once the moment the contact
    // section becomes visible: the visitor still has to read and type, which is far longer than the
    // cold start. Fire-and-forget, no-cors, and only for people who actually scroll this far.
    function warmUpWhenVisible(healthUrl) {
        if (!healthUrl || warmedUp) return;
        const target = document.getElementById('contact');
        if (!target) return;

        const observer = new IntersectionObserver((entries) => {
            if (!entries.some(e => e.isIntersecting)) return;
            observer.disconnect();
            if (warmedUp) return;
            warmedUp = true;
            fetch(healthUrl, { mode: 'no-cors', cache: 'no-store' }).catch(() => { /* waking it is enough */ });
        }, { rootMargin: '200px 0px' });

        observer.observe(target);
    }

    function watchScroll() {
        const onScroll = () => {
            document.body.classList.toggle('is-scrolled', window.scrollY > 12);
        };
        window.addEventListener('scroll', onScroll, { passive: true });
        onScroll();
    }

    return { getTheme, setTheme, observeReveals, observeSections, refreshSections, scrollToSection, watchScroll, warmUpWhenVisible };
})();

// SPA Navigation
document.addEventListener('DOMContentLoaded', function() {
    // Get all navigation links
    const navLinks = document.querySelectorAll('.nav-link');
    const pages = document.querySelectorAll('.page');
    
    // Function to show a specific page
    function showPage(pageId) {
        // Hide all pages
        pages.forEach(page => {
            page.classList.remove('active');
        });
        
        // Remove active class from all nav links
        navLinks.forEach(link => {
            link.classList.remove('active');
        });
        
        // Show the selected page
        const targetPage = document.querySelector(pageId);
        if (targetPage) {
            targetPage.classList.add('active');
        }
        
        // Add active class to the corresponding nav link
        const activeLink = document.querySelector(`a[href="${pageId}"]`);
        if (activeLink) {
            activeLink.classList.add('active');
        }
        
        // Update URL hash without scrolling
        history.pushState(null, null, pageId);
        
        // Scroll to top smoothly
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }
    
    // Add click event listeners to all navigation links
    navLinks.forEach(link => {
        link.addEventListener('click', function(e) {
            e.preventDefault();
            const pageId = this.getAttribute('href');
            showPage(pageId);
        });
    });
    
    // Handle browser back/forward buttons
    window.addEventListener('popstate', function() {
        const hash = window.location.hash || '#home';
        showPage(hash);
    });
    
    // Handle direct URL access with hash
    window.addEventListener('load', function() {
        const hash = window.location.hash || '#home';
        showPage(hash);
    });
    
    // Smooth scrolling for internal links
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function(e) {
            const href = this.getAttribute('href');
            if (href === '#' || href === '') return;
            
            e.preventDefault();
            const pageId = href;
            showPage(pageId);
        });
    });
});

// Optional: Add fade-in animation when pages change
document.addEventListener('DOMContentLoaded', function() {
    const style = document.createElement('style');
    style.textContent = `
        .page {
            animation: fadeIn 0.3s ease-in;
        }
        
        @keyframes fadeIn {
            from {
                opacity: 0;
                transform: translateY(10px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }
    `;
    document.head.appendChild(style);
});

// ============================================
// MEDICAL ROUTE HANDLER - Future-proofing QR codes
// ============================================
// This handles /medical redirecting to /medical.html
// Works with GitHub Pages and most static hosts

(function() {
    // Check if we're on the medical route (without .html extension)
    const path = window.location.pathname;
    
    // Handle /medical route (without .html)
    if (path.endsWith('/medical') || path === '/medical') {
        // Redirect to medical.html
        window.location.replace(window.location.origin + '/medical.html');
    }
})();

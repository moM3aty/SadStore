document.addEventListener('DOMContentLoaded', function () {
    const navItems = document.querySelectorAll('.profile-nav .nav-item');
    const pageContents = document.querySelectorAll('.page-content');

    // Tab Switching Logic
    navItems.forEach(item => {
        item.addEventListener('click', function (e) {
            // Check if it's a link to logout or non-tab link
            const pageId = this.getAttribute('href')?.replace('#', '');
            if (pageId === 'logout') return; // Let default action happen or handled by other script

            const targetContent = document.getElementById(`${pageId}-content`);
            if (targetContent) {
                e.preventDefault();

                navItems.forEach(nav => nav.classList.remove('active'));
                this.classList.add('active');

                pageContents.forEach(content => content.classList.remove('active'));
                targetContent.classList.add('active');

                window.scrollTo({ top: 0, behavior: 'smooth' });
            }
        });
    });

    // Account Form Submission (Mock)
    const accountForm = document.getElementById('accountForm');
    if (accountForm) {
        accountForm.addEventListener('submit', function (e) {
            e.preventDefault();
            // In a real app, this would be an AJAX call
            alert('تم حفظ التغييرات بنجاح!');
        });
    }
});
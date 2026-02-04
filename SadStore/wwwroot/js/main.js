let relatedSwiper;

function initRelatedSwiper(isRTL) {
    if (relatedSwiper) {
        relatedSwiper.destroy(true, true);
    }

    const swiperElement = document.querySelector(".relatedSwiper");
    if (swiperElement) {
        relatedSwiper = new Swiper(".relatedSwiper", {
            slidesPerView: 2,
            spaceBetween: 15,
            loop: true,
            speed: 600,
            rtl: isRTL,
            direction: "horizontal",
            autoplay: {
                delay: 3000,
                disableOnInteraction: false,
            },
            breakpoints: {
                576: { slidesPerView: 2 },
                768: { slidesPerView: 3 },
                992: { slidesPerView: 4 },
                1200: { slidesPerView: 5 },
            },
        });
    }
}

function setLanguage(lang) {
    const flag = document.getElementById("langFlag");
    const text = document.getElementById("langText");

    document.querySelectorAll("[data-" + lang + "]").forEach(el => {
        // Only update if it's not a script or style tag to avoid errors
        if (el.tagName !== 'SCRIPT' && el.tagName !== 'STYLE') {
            el.innerHTML = el.dataset[lang];
        }
    });

    document.querySelectorAll("[data-" + lang + "-placeholder]").forEach(el => {
        el.placeholder = el.dataset[lang + "Placeholder"];
    });

    const isRTL = lang === "ar";
    document.documentElement.dir = isRTL ? "rtl" : "ltr";
    document.documentElement.lang = lang;

    if (flag && text) {
        if (isRTL) {
            flag.src = "https://flagcdn.com/w20/sa.png";
            text.textContent = "العربية";
        } else {
            flag.src = "https://flagcdn.com/w20/us.png";
            text.textContent = "English";
        }
    }
    localStorage.setItem("lang", lang);
    initRelatedSwiper(isRTL);
}

document.addEventListener("DOMContentLoaded", () => {
    const items = document.querySelectorAll(".dropdown-item[data-lang]");
    items.forEach(item => {
        item.addEventListener("click", e => {
            e.preventDefault();
            setLanguage(item.dataset.lang);
        });
    });

    // Default Language
    const savedLang = localStorage.getItem("lang") || "ar";
    // We prefer not to auto-set on load to keep server-side rendering consistent, 
    // but if client-side switching is needed:
    // setLanguage(savedLang); 

    // Initialize Swiper regardless of lang set immediately
    const isRTL = document.documentElement.dir === 'rtl';
    initRelatedSwiper(isRTL);
});

// Helper for Layout Marquee
const track = document.getElementById("track");
if (track) {
    const originalSpan = track.querySelector(".content");
    // Simple logic to duplicate content for smooth marquee
    if (originalSpan) {
        while (track.offsetWidth < window.innerWidth + originalSpan.offsetWidth) {
            const clone = originalSpan.cloneNode(true);
            track.appendChild(clone);
        }
        // One more duplication for safety
        track.innerHTML += track.innerHTML;
    }
}
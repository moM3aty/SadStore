let relatedSwiper;

// تحديد اتجاه الصفحة من الـ HTML مباشرة الذي يتم ضبطه من السيرفر
const htmlDir = document.documentElement.dir;
const isRTL = htmlDir === 'rtl';

function initRelatedSwiper(isRtlParam) {
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
            // تفعيل وضع RTL في Swiper بناءً على اللغة
            rtl: isRtlParam,
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

document.addEventListener("DOMContentLoaded", () => {
    // تشغيل السلايدر عند تحميل الصفحة
    initRelatedSwiper(isRTL);
});
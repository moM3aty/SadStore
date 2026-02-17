function changeImage(element) {
    var mainImage = document.getElementById('mainImage');
    mainImage.src = element.src;

    var thumbnails = document.querySelectorAll('.thumbnail');
    thumbnails.forEach(function (thumb) {
        thumb.classList.remove('active');
    });
    element.classList.add('active');
}

function increaseQty() {
    var qty = document.getElementById('quantity');
    var val = parseInt(qty.value);
    qty.value = val + 1;
}

function decreaseQty() {
    var qty = document.getElementById('quantity');
    var val = parseInt(qty.value);
    if (val > 1) qty.value = val - 1;
}
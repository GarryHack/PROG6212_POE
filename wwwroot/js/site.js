document.querySelectorAll('input[type=file][data-preview]').forEach(function (input) {
    input.addEventListener('change', function () {
        var img = document.querySelector(input.getAttribute('data-preview'));
        if (img && input.files && input.files[0]) {
            img.src = URL.createObjectURL(input.files[0]);
            img.classList.remove('d-none');
            var ph = document.getElementById('pic-ph');
            if (ph) ph.classList.add('d-none');
        }
    });
});

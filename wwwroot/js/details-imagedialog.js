// details-imagedialog.js
$(document).ready(function () {
    // Attach click event to all images with class 'clickable-image'
    $('.clickable-image').on('click', function () {
        var fullSrc = $(this).data('fullsrc');
        var fileName = $(this).data('filename');
        if (fullSrc) {
            $('#modalImage').attr('src', fullSrc);
            $('#imageModalLabel').text(fileName || 'Image Preview');
            // Bootstrap 5 modal show
            var myModal = new bootstrap.Modal(document.getElementById('imageModal'));
            myModal.show();
        }
    });
});
// edit-image-preview.js
let selectedFilesPartI = [];
let selectedFilesPartII = [];

function createPreviewCardForEdit(file, containerId, index, part) {
    const reader = new FileReader();
    const cardDiv = document.createElement('div');
    cardDiv.setAttribute('data-index', index);
    cardDiv.setAttribute('data-part', part);
    reader.onload = function (e) {
        cardDiv.innerHTML = `
            <div class="image-preview-card">
                <img src="${e.target.result}" alt="Preview" />
                <div class="card-body">
                    <small>${escapeHtml(file.name)}</small><br />
                    <span class="remove-preview-btn" style="cursor:pointer; color:red; display:inline-block; margin-top:5px;">Remove</span>
                </div>
            </div>
        `;
        const removeBtn = cardDiv.querySelector('.remove-preview-btn');
        removeBtn.addEventListener('click', function (event) {
            event.stopPropagation();
            if (part === 'I') {
                selectedFilesPartI.splice(index, 1);
                rebuildInputAndPreviews('partIimages', 'edit-partI-images-preview', selectedFilesPartI, 'I');
            } else {
                selectedFilesPartII.splice(index, 1);
                rebuildInputAndPreviews('partIIimages', 'edit-partII-images-preview', selectedFilesPartII, 'II');
            }
        });
        document.getElementById(containerId).appendChild(cardDiv);
    };
    reader.readAsDataURL(file);
}

function rebuildInputAndPreviews(inputId, containerId, fileArray, part) {
    const dataTransfer = new DataTransfer();
    for (let i = 0; i < fileArray.length; i++) {
        dataTransfer.items.add(fileArray[i]);
    }
    document.getElementById(inputId).files = dataTransfer.files;

    const container = document.getElementById(containerId);
    if (!container) return;
    container.innerHTML = '';
    for (let i = 0; i < fileArray.length; i++) {
        createPreviewCardForEdit(fileArray[i], containerId, i, part);
    }
}

function escapeHtml(str) {
    if (!str) return '';
    return str.replace(/[&<>]/g, function (m) {
        if (m === '&') return '&amp;';
        if (m === '<') return '&lt;';
        if (m === '>') return '&gt;';
        return m;
    }).replace(/["']/g, function (m) {
        if (m === '"') return '&quot;';
        if (m === "'") return '&#39;';
        return m;
    });
}

// Edit version: drag and drop
function setupDragAndDrop(uploadCardId, fileInputId, previewContainerId, part) {
    var $card = $(uploadCardId);

    $card.on('dragover', function (e) {
        e.preventDefault();
        e.stopPropagation();
        $(this).addClass('drag-over');
    });

    $card.on('dragleave', function (e) {
        e.preventDefault();
        e.stopPropagation();
        $(this).removeClass('drag-over');
    });

    $card.on('drop', function (e) {
        e.preventDefault();
        e.stopPropagation();
        $(this).removeClass('drag-over');

        var files = e.originalEvent.dataTransfer.files;
        if (files && files.length) {
            var imageFiles = Array.from(files).filter(f => f.type.startsWith('image/'));
            if (imageFiles.length) {
                if (part === 'I') {
                    selectedFilesPartI = selectedFilesPartI.concat(imageFiles);
                    rebuildInputAndPreviews(fileInputId, previewContainerId, selectedFilesPartI, 'I');
                } else {
                    selectedFilesPartII = selectedFilesPartII.concat(imageFiles);
                    rebuildInputAndPreviews(fileInputId, previewContainerId, selectedFilesPartII, 'II');
                }
            } else {
                alert('Please drop image files only (jpg, png, gif, etc.)');
            }
        }
        return false;
    });
}

$(document).ready(function () {
    // Upload cards
    $('#uploadCardPartI').on('click', function () { $('#partIimages').click(); });
    $('#uploadCardPartII').on('click', function () { $('#partIIimages').click(); });

    // New file selections
    $('#partIimages').on('change', function (e) {
        const newFiles = Array.from(e.target.files);
        selectedFilesPartI = selectedFilesPartI.concat(newFiles);
        rebuildInputAndPreviews('partIimages', 'edit-partI-images-preview', selectedFilesPartI, 'I');
    });
    $('#partIIimages').on('change', function (e) {
        const newFiles = Array.from(e.target.files);
        selectedFilesPartII = selectedFilesPartII.concat(newFiles);
        rebuildInputAndPreviews('partIIimages', 'edit-partII-images-preview', selectedFilesPartII, 'II');
    });

    // Handle removal of existing images (from database) using event delegation
    $(document).on('click', '.remove-existing-btn', function (e) {
        e.preventDefault();
        var $imageDiv = $(this).closest('.existing-image-item');
        var imageId = $imageDiv.data('image-id');
        console.log('Remove clicked, imageId:', imageId);  // <-- Ilagay dito
        if (imageId) {
            // Add hidden field for deletion
            $('<input>').attr({
                type: 'hidden',
                name: 'deleteImageIds',
                value: imageId
            }).appendTo('form');
            // Remove the image div from UI
            $imageDiv.remove();
        } else {
            // Fallback in case the structure is different (e.g., checkbox)
            var checkbox = $imageDiv.find('input[type="checkbox"]');
            if (checkbox.length) {
                checkbox.prop('checked', true);
                $imageDiv.hide();
            }
        }
    });

    // Drag and drop setup
    setupDragAndDrop('#uploadCardPartI', 'partIimages', 'edit-partI-images-preview', 'I');
    setupDragAndDrop('#uploadCardPartII', 'partIIimages', 'edit-partII-images-preview', 'II');
});

function removeExistingImage(spanElement, imageId) {
    if (confirm('Remove this image?')) {
        var $imageDiv = $(spanElement).closest('.existing-image-item');
        $imageDiv.remove();
        $('<input>').attr({
            type: 'hidden',
            name: 'deleteImageIds',
            value: imageId
        }).appendTo('form');
    }
}
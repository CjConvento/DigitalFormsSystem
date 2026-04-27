// edit-image-preview.js
let selectedFilesPartI = [];
let selectedFilesPartII = [];

function createPreviewCardForEdit(file, containerId, index, part) {
    const reader = new FileReader();
    const cardDiv = document.createElement('div');
    cardDiv.className = 'col-md-3 mb-2';
    cardDiv.setAttribute('data-index', index);
    cardDiv.setAttribute('data-part', part);
    reader.onload = function (e) {
        cardDiv.innerHTML = `
            <div class="image-preview-card">
                <img src="${e.target.result}" alt="Preview" />
                <div class="card-body">
                    <small>${escapeHtml(file.name)}</small><br />
                    <span class="remove-preview-btn" style="cursor:pointer; color:red;">Remove</span>
                </div>
            </div>
        `;
        cardDiv.querySelector('.remove-preview-btn').addEventListener('click', function () {
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

$(document).ready(function () {
    $('#uploadCardPartI').on('click', function () {
        $('#partIimages').click();
    });
    $('#uploadCardPartII').on('click', function () {
        $('#partIIimages').click();
    });

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
});
// create-image-preview.js
let selectedFilesPartI = [];
let selectedFilesPartII = [];

function createPreviewCard(file, containerId, index, part) {
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
            // Remove file from the corresponding array
            if (part === 'I') {
                selectedFilesPartI.splice(index, 1);
                rebuildInputAndPreviews('partIimages', 'partI-images-preview', selectedFilesPartI, 'I');
            } else {
                selectedFilesPartII.splice(index, 1);
                rebuildInputAndPreviews('partIIimages', 'partII-images-preview', selectedFilesPartII, 'II');
            }
        });
        document.getElementById(containerId).appendChild(cardDiv);
    };
    reader.readAsDataURL(file);
}

function rebuildInputAndPreviews(inputId, containerId, fileArray, part) {
    // Create a new DataTransfer to hold the remaining files
    const dataTransfer = new DataTransfer();
    for (let i = 0; i < fileArray.length; i++) {
        dataTransfer.items.add(fileArray[i]);
    }
    document.getElementById(inputId).files = dataTransfer.files;

    // Clear preview container and rebuild all previews
    const container = document.getElementById(containerId);
    container.innerHTML = '';
    for (let i = 0; i < fileArray.length; i++) {
        createPreviewCard(fileArray[i], containerId, i, part);
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
    // Card click triggers hidden file input
    $('#uploadCardPartI').on('click', function () {
        $('#partIimages').click();
    });
    $('#uploadCardPartII').on('click', function () {
        $('#partIIimages').click();
    });

    // Handle file selection for Part I
    $('#partIimages').on('change', function (e) {
        const newFiles = Array.from(e.target.files);
        // Merge new files with existing selectedFilesPartI (avoid duplicates by name & size? simple add)
        selectedFilesPartI = selectedFilesPartI.concat(newFiles);
        rebuildInputAndPreviews('partIimages', 'partI-images-preview', selectedFilesPartI, 'I');
    });

    // Handle file selection for Part II
    $('#partIIimages').on('change', function (e) {
        const newFiles = Array.from(e.target.files);
        selectedFilesPartII = selectedFilesPartII.concat(newFiles);
        rebuildInputAndPreviews('partIIimages', 'partII-images-preview', selectedFilesPartII, 'II');
    });
});
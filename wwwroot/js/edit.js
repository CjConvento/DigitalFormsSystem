// edit.js
let rowIndex = 0;

function escapeHtml(str) {
    if (str === undefined || str === null) return '';
    // Convert to string (handles numbers, etc.)
    str = str.toString();
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

function addRow(itemNo = '', description = '', location = '', userName = '', remarks = '') {
    const newRow = `
        <tr>
            <td><input type="number" name="ExistingUnits[${rowIndex}].ItemNo" class="form-control item-no" value="${escapeHtml(itemNo)}" /></td>
            <td><input type="text" name="ExistingUnits[${rowIndex}].Description" class="form-control description" value="${escapeHtml(description)}" required /></td>
            <td><input type="text" name="ExistingUnits[${rowIndex}].Location" class="form-control location" value="${escapeHtml(location)}" /></td>
            <td><input type="text" name="ExistingUnits[${rowIndex}].UserName" class="form-control user-name" value="${escapeHtml(userName)}" /></td>
            <td><input type="text" name="ExistingUnits[${rowIndex}].Remarks" class="form-control remarks" value="${escapeHtml(remarks)}" /></td>
            <td><button type="button" class="btn btn-danger btn-sm removeRow">Remove</button></td>
        </tr>`;
    $("#existingUnitsTable tbody").append(newRow);
    rowIndex++;
}

function reindexRows() {
    $("#existingUnitsTable tbody tr").each(function (idx) {
        $(this).find(".item-no").val(idx + 1);
        $(this).find(".item-no").attr("name", `ExistingUnits[${idx}].ItemNo`);
        $(this).find(".description").attr("name", `ExistingUnits[${idx}].Description`);
        $(this).find(".location").attr("name", `ExistingUnits[${idx}].Location`);
        $(this).find(".user-name").attr("name", `ExistingUnits[${idx}].UserName`);
        $(this).find(".remarks").attr("name", `ExistingUnits[${idx}].Remarks`);
    });
    rowIndex = $("#existingUnitsTable tbody tr").length;
}

$(document).ready(function () {
    // Retrieve server‑side data
    var requestType = window.fixedAssetEdit ? window.fixedAssetEdit.requestType : null;
    var existingUnits = window.fixedAssetEdit ? window.fixedAssetEdit.existingUnits : null;

    console.log("requestType:", requestType);
    console.log("existingUnits:", existingUnits);

    // Show/hide existing units section based on RequestType
    $("#RequestType").change(function () {
        if ($(this).val() === "Additional") {
            $("#existingUnitsSection").show();
            if ($("#existingUnitsTable tbody tr").length === 0) {
                addRow();
            }
        } else {
            $("#existingUnitsSection").hide();
            $("#existingUnitsTable tbody").empty();
            rowIndex = 0;
        }
    });

    // Add row button
    $("#addUnitRow").click(function () {
        let lastRow = $("#existingUnitsTable tbody tr:last");
        let lastDesc = lastRow.find(".description").val();
        if (lastDesc && lastDesc.trim() !== "") {
            addRow();
        } else {
            alert("Please fill in the Description for the current row before adding another.");
        }
    });

    // Remove row
    $(document).on("click", ".removeRow", function () {
        $(this).closest("tr").remove();
        reindexRows();
    });

    // Pre‑populate existing units when the page loads
    if (requestType === 'Additional') {
        $("#existingUnitsSection").show();
        var existingRowCount = $("#existingUnitsTable tbody tr").length;
        if (existingRowCount > 0) {
            rowIndex = existingRowCount;
            reindexRows();
        } else {
            if (existingUnits && existingUnits.length) {
                for (var i = 0; i < existingUnits.length; i++) {
                    addRow(
                        existingUnits[i].itemNo,
                        existingUnits[i].description,
                        existingUnits[i].location,
                        existingUnits[i].userName,
                        existingUnits[i].remarks
                    );
                }
            } else {
                addRow();
            }
        }
    } else {
        $("#existingUnitsSection").hide();
        $("#existingUnitsTable tbody").empty();
        rowIndex = 0;
    }

    // Validation before submit
    $("form").submit(function (e) {
        if ($("#RequestType").val() === "Additional") {
            let isValid = true;
            $("#existingUnitsTable tbody tr").each(function () {
                let desc = $(this).find(".description").val();
                if (!desc || desc.trim() === "") {
                    isValid = false;
                    $(this).find(".description").addClass("is-invalid");
                } else {
                    $(this).find(".description").removeClass("is-invalid");
                }
            });
            if (!isValid) {
                e.preventDefault();
                alert("Please fill in the Description for all existing unit rows. Remove any empty rows.");
            }
        }
    });
});


// ============================================================
// PART IV – FOLLOW-UP STATUS (Add/Remove Rows)
// ============================================================

$(document).ready(function () {
    // Add row
    $(document).on('click', '.add-row-btn', function () {
        var rowCount = $('#followUpBody .follow-up-row').length;
        var newRow = `
            <tr class="follow-up-row">
                <td>
                    <input type="date" name="FollowUps[${rowCount}].FollowUpDate" class="form-control form-control-sm" />
                </td>
                <td>
                    <input type="text" name="FollowUps[${rowCount}].Status" class="form-control form-control-sm" placeholder="Status" />
                </td>
                <td>
                    <input type="text" name="FollowUps[${rowCount}].UpdateBy" class="form-control form-control-sm" placeholder="Update by" />
                </td>
                <td>
                    <input type="text" name="FollowUps[${rowCount}].NotedBy" class="form-control form-control-sm" placeholder="Noted by" />
                </td>
                <td>
                    <button type="button" class="btn btn-sm btn-danger remove-row-btn" title="Remove Row">
                        <i class="fas fa-times"></i>
                    </button>
                </td>
            </tr>
        `;
        $('#followUpBody').append(newRow);
        updateRowIndices();
    });

    // Remove row
    $(document).on('click', '.remove-row-btn', function () {
        if ($('#followUpBody .follow-up-row').length > 1) {
            $(this).closest('.follow-up-row').remove();
            updateRowIndices();
        } else {
            alert('You must have at least one row.');
        }
    });

    // Update row indices after add/remove
    function updateRowIndices() {
        $('#followUpBody .follow-up-row').each(function (index) {
            $(this).find('input[name*="FollowUps["]').each(function () {
                var name = $(this).attr('name');
                if (name) {
                    var newName = name.replace(/FollowUps\[\d+\]/, 'FollowUps[' + index + ']');
                    $(this).attr('name', newName);
                }
            });
            $(this).find('input[type="hidden"][name*="FollowUps["]').each(function () {
                var name = $(this).attr('name');
                if (name) {
                    var newName = name.replace(/FollowUps\[\d+\]/, 'FollowUps[' + index + ']');
                    $(this).attr('name', newName);
                }
            });
        });
    }
});
// fixedasset-create.js
$(document).ready(function () {
    let rowIndex = 0;

    function addRow() {
        const newRow = `
            <tr>
                <td><input type="number" name="ExistingUnits[${rowIndex}].ItemNo" class="form-control item-no" /></td>
                <td><input type="text" name="ExistingUnits[${rowIndex}].Description" class="form-control description" required /></td>
                <td><input type="text" name="ExistingUnits[${rowIndex}].Location" class="form-control" /></td>
                <td><input type="text" name="ExistingUnits[${rowIndex}].UserName" class="form-control" /></td>
                <td><input type="text" name="ExistingUnits[${rowIndex}].Remarks" class="form-control" /></td>
                <td><button type="button" class="btn btn-danger btn-sm removeRow">Remove</button></td>
            </tr>`;
        $("#existingUnitsTable tbody").append(newRow);
        rowIndex++;
    }

    // Show/hide existing units section based on RequestType
    $("#RequestType").change(function () {
        if ($(this).val() === "Additional") {
            $("#existingUnitsSection").show();
            if ($("#existingUnitsTable tbody tr").length === 0) {
                addRow(); // magdagdag ng isang default row
            }
        } else {
            $("#existingUnitsSection").hide();
            $("#existingUnitsTable tbody").empty();
            rowIndex = 0;
        }
    });

    // Add row button
    $("#addUnitRow").click(function () {
        // Siguraduhing ang huling row ay may laman ang Description bago magdagdag ng bago
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
        // Re-index rows (opsyonal: para maging maayos ang ItemNo)
        reindexRows();
    });

    function reindexRows() {
        $("#existingUnitsTable tbody tr").each(function (idx) {
            $(this).find(".item-no").val(idx + 1);
            // I-update ang name attribute upang manatili ang tamang index sa server
            $(this).find(".item-no").attr("name", `ExistingUnits[${idx}].ItemNo`);
            $(this).find(".description").attr("name", `ExistingUnits[${idx}].Description`);
            $(this).find(".location").attr("name", `ExistingUnits[${idx}].Location`);
            $(this).find(".user-name").attr("name", `ExistingUnits[${idx}].UserName`);
            $(this).find(".remarks").attr("name", `ExistingUnits[${idx}].Remarks`);
        });
        rowIndex = $("#existingUnitsTable tbody tr").length;
    }

    // Bago mag-submit ng form, i-validate ang lahat ng existing units rows
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
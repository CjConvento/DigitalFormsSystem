// create-formdatetime.js
document.addEventListener('DOMContentLoaded', function () {
    // Helper function para i-update ang hidden field
    function updateCombinedDateTime(prefix) {
        var dateVal = document.getElementById(prefix + 'Date')?.value || '';
        var timeVal = document.getElementById(prefix + 'Time')?.value || '';
        var combined = (dateVal && timeVal) ? dateVal + 'T' + timeVal : '';
        var hiddenField = document.querySelector('[name="' + prefix + 'DateTime"]');
        if (hiddenField) {
            hiddenField.value = combined;
            // Optional: i-trigger ang validation manually kung kinakailangan
            hiddenField.dispatchEvent(new Event('change', { bubbles: true }));
        }
    }

    // --- INCIDENT Date & Time Pickers ---
    flatpickr("#IncidentDate", {
        dateFormat: "Y-m-d",
        allowInput: false,
        onChange: function () { updateCombinedDateTime('Incident'); }
    });
    flatpickr("#IncidentTime", {
        enableTime: true,
        noCalendar: true,
        dateFormat: "H:i",
        time_24hr: true,
        allowInput: false,
        onChange: function () { updateCombinedDateTime('Incident'); }
    });

    // --- RECEIVED Date & Time Pickers ---
    flatpickr("#ReceivedDate", {
        dateFormat: "Y-m-d",
        allowInput: false,
        onChange: function () { updateCombinedDateTime('Received'); }
    });
    flatpickr("#ReceivedTime", {
        enableTime: true,
        noCalendar: true,
        dateFormat: "H:i",
        time_24hr: true,
        allowInput: false,
        onChange: function () { updateCombinedDateTime('Received'); }
    });

    // Siguruhin na kapag nag-submit ang form, updated ang hidden fields (redundant pero safe)
    document.querySelector('form').addEventListener('submit', function () {
        updateCombinedDateTime('Incident');
        updateCombinedDateTime('Received');
    });
});
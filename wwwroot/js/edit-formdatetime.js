// edit-formdatetime.js
document.addEventListener('DOMContentLoaded', function () {
    function convertTo24Hour(time12) {
        if (!time12) return '';
        var parts = time12.match(/(\d+):(\d+)\s*(AM|PM)/i);
        if (!parts) return time12;
        var hour = parseInt(parts[1]);
        var minute = parts[2];
        var ampm = parts[3].toUpperCase();
        if (ampm === 'PM' && hour !== 12) hour += 12;
        if (ampm === 'AM' && hour === 12) hour = 0;
        return hour.toString().padStart(2, '0') + ':' + minute;
    }

    function updateCombinedDateTime(prefix) {
        var dateVal = document.getElementById(prefix + 'Date')?.value || '';
        var timeVal12 = document.getElementById(prefix + 'Time')?.value || '';
        var timeVal24 = convertTo24Hour(timeVal12);
        var combined = (dateVal && timeVal24) ? dateVal + 'T' + timeVal24 : '';
        var hiddenField = document.querySelector('[name="' + prefix + 'DateTime"]');
        if (hiddenField) hiddenField.value = combined;
    }

    // Initialize Flatpickr (same as create)
    flatpickr("#IncidentDate", {
        dateFormat: "Y-m-d",
        allowInput: false,
        onChange: function () { updateCombinedDateTime('Incident'); }
    });
    flatpickr("#IncidentTime", {
        enableTime: true,
        noCalendar: true,
        dateFormat: "h:i K",
        time_24hr: false,
        allowInput: false,
        onChange: function () { updateCombinedDateTime('Incident'); }
    });

    flatpickr("#ReceivedDate", {
        dateFormat: "Y-m-d",
        allowInput: false,
        onChange: function () { updateCombinedDateTime('Received'); }
    });
    flatpickr("#ReceivedTime", {
        enableTime: true,
        noCalendar: true,
        dateFormat: "h:i K",
        time_24hr: false,
        allowInput: false,
        onChange: function () { updateCombinedDateTime('Received'); }
    });

    // Also update on submit (kung may nagbago bago mag-submit)
    document.querySelector('form')?.addEventListener('submit', function () {
        updateCombinedDateTime('Incident');
        updateCombinedDateTime('Received');
    });
});
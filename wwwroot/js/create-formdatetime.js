// create-formdatetime.js
document.querySelector('form').addEventListener('submit', function () {
    // Incident
    var incidentDate = document.getElementById('IncidentDate').value;
    var incidentTime = document.getElementById('IncidentTime').value;
    var incidentHidden = document.querySelector('[name="IncidentDateTime"]');
    if (incidentDate && incidentTime) {
        incidentHidden.value = incidentDate + 'T' + incidentTime;
    } else {
        incidentHidden.value = '';
    }

    // Received
    var receivedDate = document.getElementById('ReceivedDate').value;
    var receivedTime = document.getElementById('ReceivedTime').value;
    var receivedHidden = document.querySelector('[name="ReceivedDateTime"]');
    if (receivedDate && receivedTime) {
        receivedHidden.value = receivedDate + 'T' + receivedTime;
    } else {
        receivedHidden.value = '';
    }
});
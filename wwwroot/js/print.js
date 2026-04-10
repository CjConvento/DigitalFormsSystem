// print.js
var requestTypeElement = document.getElementById('requestTypeData');
if (requestTypeElement) {
    var requestType = requestTypeElement.getAttribute('data-request-type');
    var checkboxes = document.querySelectorAll('.request-checkbox');
    checkboxes.forEach(function (cb) {
        if (cb.value === requestType) {
            cb.checked = true;
        }
    });
}
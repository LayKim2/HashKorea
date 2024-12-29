
function getCommonCodes(type, targetDropdownId) {
    $.ajax({
        url: '/api/shared/commoncodes',
        type: 'GET',
        data: { type: type },
        success: function (response) {
            if (response.success) {
                var commonCodes = response.data;
                populateDropdown(commonCodes, targetDropdownId);
            } else {
                alert('Failed to load common codes: ' + response.message);
            }
        },
        error: function () {
            alert('An error occurred while loading common codes.');
        }
    });
}

function populateDropdown(commonCodes, targetDropdownId) {
    var $dropdown = $('#' + targetDropdownId);
    $dropdown.empty();

    $dropdown.append($('<option></option>').val('').text('Select a category'));

    $.each(commonCodes, function (index, code) {
        $dropdown.append($('<option></option>').val(code.code).text(code.name));
    });
}


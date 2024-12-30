

// 1. Get CommonCodes

function getCommonCodes(type, targetDropdownId, currentCategory) {
    $.ajax({
        url: '/api/shared/commoncodes',
        type: 'GET',
        data: { type: type },
        success: function (response) {
            if (response.success) {
                var commonCodes = response.data;
                populateDropdown(commonCodes, targetDropdownId, currentCategory);
            } else {
                alert('Failed to load common codes: ' + response.message);
            }
        },
        error: function () {
            alert('An error occurred while loading common codes.');
        }
    });
}

function populateDropdown(commonCodes, targetDropdownId, currentCategory) {
    var $dropdown = $('#' + targetDropdownId);
    $dropdown.empty();

    $dropdown.append($('<option></option>').val('').text('Select a category'));

    $.each(commonCodes, function (index, code) {
        var $option = $('<option></option>').val(code.code).text(code.name);
        if (currentCategory && code.name === currentCategory) {
            $option.prop('selected', true);
        }
        $dropdown.append($option);
    });

}


// 2. Image Upload

let blobUrls = [];

function handleFiles(files) {

    $.each(files, function (i, file) {
        if (file.type.startsWith('image/')) {
            const reader = new FileReader();
            reader.onload = function (event) {
                const blob = new Blob([event.target.result], { type: file.type });
                const url = URL.createObjectURL(blob);

                blobUrls.push(url);

                const img = new Image();
                img.onload = function () {
                    insertImageAtCursor(this);
                };
                img.src = url;
            };
            reader.readAsArrayBuffer(file);
        }
    });
}

// Function to insert image at cursor position
function insertImageAtCursor(img) {
    const $editor = $('#editor');
    $editor.focus();

    const selection = window.getSelection();
    const range = document.createRange();

    if (selection.rangeCount > 0 && $editor[0].contains(selection.anchorNode)) {
        range.setStart(selection.anchorNode, selection.anchorOffset);
    } else {
        const lastChild = $editor[0].lastChild;
        if (lastChild) {
            if (lastChild.nodeType === Node.TEXT_NODE) {
                range.setStart(lastChild, lastChild.length);
            } else {
                range.setStartAfter(lastChild);
            }
        } else {
            range.setStart($editor[0], 0);
        }
    }

    range.insertNode(img);
    range.collapse(false);

    selection.removeAllRanges();
    selection.addRange(range);

    $editor[0].dispatchEvent(new Event('input', { bubbles: true }));

    $editor.html($editor.html());

    $editor.focus();
}
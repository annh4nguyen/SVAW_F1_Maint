$(document).ready(function () {
    //set lai value cho SSCOde khi Edit record
    if ($('#SSCode').val().length) {
        var data = {
            id: 1,
            text: $('#SSCode').val()
        };

        var newOption = new Option(data.text, data.id, false, false);
        $('#select_SSCode').empty().append(newOption).trigger('change');
    };

    //set lai value cho SSCOde khi Edit record
    if ($('#VendorCode').val().length) {
        var data = {
            id: 1,
            text: $('#VendorCode').val()
        };

        var newOption = new Option(data.text, data.id, false, false);
        $('#select_VendorCode').empty().append(newOption).trigger('change');
    }
})
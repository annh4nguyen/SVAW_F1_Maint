//Load Data function
function loadData() {
    $.ajax({
        url: "/CarryInDetail/List",
        data: {
            parentid: $('#ParentId').val()
        },
        type: "GET",
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (result) {
            var html = '';
            $.each(result, function (key, item) {
                html += '<tr>';
                html += '<td>' + item.SSCode + '</td>';
                html += '<td>' + item.VendorCode + '</td>';
                html += '<td>' + item.Mo_del + '</td>';
                html += '<td>' + item.Division + '</td>';
                html += '<td>' + item.Vendor + '</td>';
                html += '<td>' + item.Quantity + '</td>';
                html += '<td>' + item.Issue + '</td>';
                html += '<td>' + item.Remark + '</td>';
                html += '<td><a href="#" onclick="return getbyID(' + item.ID + ')"><i class="fa fa-pencil"></i>Sửa</a> | <a href="#" onclick="Delele(' + item.ID + ')"><i class="fa fa-trash-o"></i>Xóa</a></td>';
                html += '</tr>';
            });
            $('.tbody').html(html);
        },
        error: function (errormessage) {
            console.log(errormessage);
            //alert(errormessage.responseText);
        }
    });
}


//Function for getting the Data Based upon Employee ID
function getbyID(ID) {
    $('#ChildId').val(ID); //khi click save se biet dc mode: edit hay create
    $.ajax({
        url: "/CarryInDetail/getbyID",
        data: {
            ID: ID
        },
        typr: "GET",
        contentType: "application/json;charset=UTF-8",
        dataType: "json",
        success: function (result) {
            console.log(result);
            $('#SSCode').val(result.SSCode);
            var data = {
                id: 1,
                text: $('#SSCode').val()
            };
            var newOption = new Option(data.text, data.id, false, false);
            $('#select_SSCode').empty().append(newOption).trigger('change');

            $('#VendorCode').val(result.VendorCode);
            var data = {
                id: 1,
                text: $('#VendorCode').val()
            };
            var newOption = new Option(data.text, data.id, false, false);
            $('#select_VendorCode').empty().append(newOption).trigger('change');

            $('#Mo_del').val(result.Mo_del);
            $('#Division').val(result.Division);
            $('#Vendor').val(result.Vendor);
            $('#Quantity').val(result.Quantity);
            $('#Issue').val(result.Issue);
            $('#Remark').val(result.Remark);
        },
        error: function (errormessage) {
            console.log('err: ', errormessage.responseText);
        }
    });
    return false;
}

function SaveDetail() {
    var res = validate();
    if (res == false) {
        return false;
    }
    var entity = {
        ID: $('#ChildId').val(),
        SSCode: $('#SSCode').val(),
        VendorCode: $('#VendorCode').val(),
        Vendor: $('#Vendor').val(),
        Mo_del: $('#Mo_del').val(),
        Division: $('#Division').val(),
        Quantity: $('#Quantity').val(),
        Issue: $('#Issue').val(),
        Remark: $('#Remark').val(),
        CarryInId : $('#ParentId').val()
    };
    $.ajax({
        url: "/CarryInDetail/AddOrUpdate",
        data: {
            entity
        },
        type: "POST",
        success: function (result) {
            loadData();
        },
        error: function (errormessage) {
            console.log("error",errormessage);
        }
    });
    //reset mode
    ResetDetailForm();
}


function Delele(ID) {
    var ans = confirm("Bạn có chắc chắn muốn xóa?");
    if (ans) {
        $.ajax({
            url: "/CarryInDetail/Delete",
            data: {
                ID : ID
            },
            type: "POST",           
            success: function (result) {
                loadData();
            },
            error: function (errormessage) {
                //alert(errormessage.responseText);
            }
        });
    }
}

//Function for clearing the textboxes
function ResetDetailForm() {
    $('#ChildId').val(0);
    $('#SSCode').val("");

    var optionSelectSSCode = new Option("", 1, false, false);
    $('#select_SSCode')
        .empty()
        .append(optionSelectSSCode).trigger('change');

    $('#VendorCode').val("");

    var optionSelectVendorCode = new Option("", 1, false, false);
    $('#select_VendorCode')
        .empty()
        .append(optionSelectVendorCode).trigger('change');

    $('#Mo_del').val("");
    $('#Division').val("");
    $('#Vendor').val("");
    $('#Quantity').val("");
    $('#Issue').val("");
    $('#Remark').val("");

    $('.error').remove();  
}

//Valdidation using jquery
function validate() {
    var isValid = true;
    if ($('#SSCode').val().trim() == "") {
        $("<label id='SSCode_error' class='error'>This field is required.</label>").insertAfter("#SSCode");
        isValid = false;
    }
    else {
        $('SSCode_error').remove();
    }
    if ($('#VendorCode').val().trim() == "") {
        $("<label id='VendorCode_error' class='error'>This field is required.</label>").insertAfter("#VendorCode");
        isValid = false;
    }
    else {
        $('#VendorCode_error').remove();
    }
    if ($('#Quantity').val().trim() == "") {
        $("<label id='Quantity_error' class='error'>This field is required.</label>").insertAfter("#Quantity");
        isValid = false;
    }
    else {
        $('#Quantity_error').remove();
    }
    if ($('#Issue').val().trim() == "") {
        $("<label id='Issue_error' class='error'>This field is required.</label>").insertAfter("#Issue");
        isValid = false;
    }
    else {
        $('#Issue_error').remove();
    }
    return isValid;
}


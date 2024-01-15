function bindList() {
    $.ajax({
        url: "/Unit/List",
        type: "GET",
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (result) {
            //console.log(result);
            $.each(result, function (key, item) {
                $("#UnitId").append("<option value='" + item.ID + "'>" + item.Name + "</option>");
                $("#ServiceUnitId").append("<option value='" + item.ID + "'>" + item.Name + "</option>");
            });
        },
        error: function (errormessage) {
            console.log(errormessage);
            //alert(errormessage.responseText);
        }
    });

    //$.ajax({
    //    url: "/Service/List",
    //    type: "GET",
    //    contentType: "application/json;charset=utf-8",
    //    dataType: "json",
    //    success: function (result) {
    //        //console.log(result);
    //        $.each(result, function (key, item) {
    //            $("#ServiceId").append("<option value='" + item.ID + "'>" + item.Name + "</option>");
    //        });
    //    },
    //    error: function (errormessage) {
    //        console.log(errormessage);
    //        //alert(errormessage.responseText);
    //    }
    //});

    $.ajax({
        url: "/Provider/List",
        type: "GET",
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (result) {
            //console.log(result);
            $.each(result, function (key, item) {
                $("#ProviderId").append("<option value='" + item.ID + "'>" + item.Name + "</option>");
            });
        },
        error: function (errormessage) {
            console.log(errormessage);
            //alert(errormessage.responseText);
        }
    });
}
//Load Data function
function loadReceiptDetails(refreshTotal) {
    $.ajax({
        url: "/ReceiptDetail/List",
        data: {
            parentid: $('#ReceiptId').val()
        },
        type: "GET",
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (result) {
            var html = '';
            var _totaldetail = 0;
            $.each(result, function (key, item) {
                html += '<tr>';
                html += '<td>' + item.Name + '</td>';
                html += '<td>' + item.InvoiceNo + '</td>';
                html += '<td>' + item.Unit + '</td>';
                html += '<td>' + item.Quantity.toLocaleString() + '</td>';
                html += '<td>' + item.UnitPrice.toLocaleString() + '</td>';
                html += '<td>' + item.Amount.toLocaleString() + '</td>';
                _totaldetail += item.Amount;
                html += '<td>' + item.Description + '</td>';
                html += '<td>' + item.Package + '</td>';
                html += '<td>' + item.Picked + '</td>';
                html += '<td>' + item.Remain + '</td>';
                html += '<td><a href="#" onclick="return getReceiptDetailByID(' + item.ID + ')"><i class="fa fa-pencil"></i>Sửa</a> | <a href="#" onclick="DeleteReceiptDetail(' + item.ID + ')"><i class="fa fa-trash-o"></i>&nbsp;Xóa</a></td>';
                html += '</tr>';
            });
            $('#listReceiptDetails').html(html);
            $('#ReceiptDetailTotal').val(_totaldetail);

            if (refreshTotal) {
                UpdateReceiptTotal();
            }
        },
        error: function (errormessage) {
            console.log(errormessage);
            //alert(errormessage.responseText);
        }
    });
}


//Function for getting the Data Based upon Employee ID
function getReceiptDetailByID(ID) {
    $('#ReceiptDetailId').val(ID); //khi click save se biet dc mode: edit hay create
    $.ajax({
        url: "/ReceiptDetail/GetByID",
        data: {
            ID: ID
        },
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        dataType: "json",
        success: function (result) {
            console.log(result);
            $('#Name').val(result.Name);
            $('#Package').val(result.Package);
            $('#InvoiceNo').val(result.InvoiceNo);
            $('#Quantity').val(result.Quantity);
            $('#UnitPrice').val(result.UnitPrice);
            $('.error').remove();
            $('#UnitId').val(result.UnitId);
            $('#Description').val(result.Description);
        },
        error: function (errormessage) {
            console.log('err: ', errormessage.responseText);
        }
    });
    return false;
}

function SaveReceiptDetail() {
    var res = validateReceiptDetail();
    if (res == false) {
        return false;
    }
    var entity = {
        ID: $('#ReceiptDetailId').val(),
        ReceiptId: $('#ReceiptId').val(),
        Name: $('#Name').val(),
        InvoiceNo: $('#InvoiceNo').val(),
        Package: $('#Package').val(),
        UnitId: $('#UnitId').val(),
        Quantity: $('#Quantity').val(),
        Description: $('#Description').val(),
        UnitPrice: $('#UnitPrice').val()
    };
    //alert($('#Description').val());
    $.ajax({
        url: "/ReceiptDetail/AddOrUpdate",
        data: {
            entity
        },
        type: "POST",
        success: function (result) {
            loadReceiptDetails(true);
            //reset mode
            ResetReceiptDetailForm();
        },
        error: function (errormessage) {
            console.log("error",errormessage);
        }
    });
}


function DeleteReceiptDetail(ID) {
    var ans = confirm("Bạn có chắc chắn muốn xóa?");
    if (ans) {
        $.ajax({
            url: "/ReceiptDetail/Delete",
            data: {
                ID : ID
            },
            type: "POST",           
            success: function (result) {
                loadReceiptDetails(true);
            },
            error: function (errormessage) {
                //alert(errormessage.responseText);
            }
        });
    }
}

//Function for clearing the textboxes
function ResetReceiptDetailForm() {
    $('#ReceiptDetailId').val(0);
    $('#Name').val('');
    $('#InvoiceNo').val('');
    $('#Package').val('');
    $('#UnitId').val(0);
    $('#Quantity').val('');
    $('#UnitPrice').val('');
    $('#Description').val('');
    $('.error').remove();
}

//Function for clearing the textboxes
function ResetReceiptServiceForm() {

    $('#ServiceName').val('');
    $('#ServiceUnitId').val(0);
    $('#ServiceQuantity').val(0);
    $('#ServiceUnitPrice').val(0);
    $('.error').remove();
    $('#ServiceDescription').val('');
    $('#ProviderId').val(0);
    $('#ProviderId').select2().trigger('change');

    $('#Cost').val(0);
    
}

//Valdidation using jquery
function validateReceiptDetail() {
    var isValid = true;
    if ($('#Name').val().trim() == "") {
        $("<label id='Name_error' class='error'>Thông tin bắt buộc nhập.</label>").insertAfter("#Name");
        isValid = false;
    }
    else {
        $('#Name_error').remove();
    }
    if ($('#InvoiceNo').val().trim() == "") {
        $("<label id='InvoiceNo_error' class='error'>Thông tin bắt buộc nhập.</label>").insertAfter("#InvoiceNo");
        isValid = false;
    }
    else {
        $('#InvoiceNo_error').remove();
    }
    if ($('#UnitId').val().trim() == "0") {
        $("<label id='UnitId_error' class='error'>Thông tin bắt buộc nhập.</label>").insertAfter("#UnitId");
        isValid = false;
    }
    else {
        $('#UnitId_error').remove();
    }
    if ($('#Quantity').val().trim() == "") {
        $("<label id='Quantity_error' class='error'>Thông tin bắt buộc nhập.</label>").insertAfter("#Quantity");
        isValid = false;
    }
    else {
        if (!$.isNumeric($('#Quantity').val().trim())) {
            $("<label id='Quantity_error' class='error'>Chỉ được phép nhập số.</label>").insertAfter("#Quantity");
            isValid = false;
        }
        else {
            $('#Quantity_error').remove();
        }
    }
    if ($('#UnitPrice').val().trim() == "") {
        $("<label id='UnitPrice_error' class='error'>Thông tin bắt buộc nhập.</label>").insertAfter("#UnitPrice");
        isValid = false;
    }
    else {
        if (!$.isNumeric($('#UnitPrice').val().trim())) {
            $("<label id='UnitPrice_error' class='error'>Chỉ được phép nhập số.</label>").insertAfter("#UnitPrice");
            isValid = false;
        }
        else {
            $('#UnitPrice_error').remove();

        }
    }
    return isValid;
}

//Load Data function
function loadReceiptServices(refreshTotal) {
    //alert("Start load Service");
    var _role = $('#RoleName').val();
    $.ajax({
        url: "/ReceiptService/List",
        data: {
            parentid: $('#ReceiptId').val()
        },
        type: "GET",
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (result) {
            var html = '';
            var _totalservice = 0;
            $('#listReceiptServices').html(html);
            $.each(result, function (key, item) {
                html += '<tr>';
                html += '<td>' + item.ServiceName + '</td>';
                html += '<td>' + item.Unit + '</td>';
                html += '<td>' + item.Quantity.toLocaleString() + '</td>';
                html += '<td>' + item.UnitPrice.toLocaleString() + '</td>';
                html += '<td>' + item.Amount.toLocaleString() + '</td>';
                _totalservice += item.Amount;
                html += '<td>' + item.Description + '</td>';
                html += '<td>' + item.Provider + '</td>';
                if (_role == "MANAGER" || _role == "ACCOUNTANT") {
                    html += '<td>' + item.Cost + '</td>';
                }
                html += '<td><a href="#" onclick="return getReceiptServiceByID(' + item.ID + ')"><i class="fa fa-pencil"></i>Sửa</a> | <a href="#" onclick="DeleteReceiptService(' + item.ID + ')"><i class="fa fa-trash-o"></i>&nbsp;Xóa</a></td>';
                html += '</tr>';
            });
            //alert(html);
            $('#listReceiptServices').html(html);
            $('#ReceiptServiceTotal').val(_totalservice);
            if (refreshTotal) {
                UpdateReceiptTotal();
            }

        },
        error: function (errormessage) {
            console.log(errormessage);
            //alert(errormessage.responseText);
        }
    });
}


//Function for getting the Data Based upon Employee ID
function getReceiptServiceByID(ID) {
    $('#ReceiptServiceId').val(ID); //khi click save se biet dc mode: edit hay create
    $.ajax({
        url: "/ReceiptService/GetByID",
        data: {
            ID: ID
        },
        typr: "GET",
        contentType: "application/json;charset=UTF-8",
        dataType: "json",
        success: function (result) {
            console.log(result);
            $('#ServiceName').val(result.ServiceName);
            $('#ServiceUnitId').val(result.UnitId);
            $('#ServiceQuantity').val(result.Quantity);
            $('#ServiceUnitPrice').val(result.UnitPrice);
            $('.error').remove();
            $('#ServiceDescription').val(result.Description);
            $('#ProviderId').val(result.ProviderId);
            $('#ProviderId').select2().trigger('change');

            $('#Cost').val(result.Cost);
        },
        error: function (errormessage) {
            console.log('err: ', errormessage.responseText);
        }
    });
    return false;
}

function SaveReceiptService() {
    var res = validateReceiptService();
    if (res == false) {
        return false;
    }
    var _id = $('#ReceiptServiceId').val();
    if (_id == null) _id = 0;

    var _ReceiptId = $('#ReceiptId').val();
    if (_ReceiptId == null) _ReceiptId = 0;

    var _ServiceName = $('#ServiceName').val();
    if (_ServiceName == null) _ServiceName ='';

    var _ServiceUnitId = $('#ServiceUnitId').val();
    if (_ServiceUnitId == null) _ServiceUnitId = 0;

    var _ServiceQuantity = $('#ServiceQuantity').val();
    if (_ServiceQuantity == null) _ServiceQuantity = 0;

    var _ServiceUnitPrice = $('#ServiceUnitPrice').val();
    if (_ServiceUnitPrice == null) _ServiceUnitPrice = 0;

    var _ServiceDescription = $('#ServiceDescription').val();
    if (_ServiceDescription == null) _ServiceDescription = '';

    var _ProviderId = $('#ProviderId').val();
    if (_ProviderId == null) _ProviderId = 0;

    //alert(_ServiceDescription);

    var _Cost = $('#Cost').val();
    if (_Cost == null) _Cost = 0;


    var entity = {
        ID: _id,
        ReceiptId: _ReceiptId,
        ServiceName: _ServiceName,
        UnitId: _ServiceUnitId,
        Quantity: _ServiceQuantity,
        UnitPrice: _ServiceUnitPrice,
        Description: _ServiceDescription,
        ProviderId: _ProviderId,
        Cost: _Cost
    };
    //alert(entity.ID + "~" + entity.ReceiptId + "~" + entity.ServiceId + "~" + entity.UnitId + "~" + entity.Quantity + "~" + entity.UnitPrice + "~" + entity.ProviderId + "~" + entity.Cost + "~" + entity.Description);
    $.ajax({
        url: "/ReceiptService/AddOrUpdate",
        data: {
            entity
            },
        type: "POST",
        success: function (result) {
            //alert('Save OK');
            loadReceiptServices(true);
            //reset mode
            ResetReceiptServiceForm();
        },
        error: function (errormessage) {
            console.log("error", errormessage);
            alert('Có lỗi');
        }
    });
}


function DeleteReceiptService(ID) {
    var ans = confirm("Bạn có chắc chắn muốn xóa?");
    if (ans) {
        $.ajax({
            url: "/ReceiptService/Delete",
            data: {
                ID: ID
            },
            type: "POST",
            success: function (result) {
                loadReceiptServices(true);
            },
            error: function (errormessage) {
                //alert(errormessage.responseText);
            }
        });
    }
}

//Function for clearing the textboxes
function UpdateReceiptTotal() {
    //alert($('#ReceiptDetailTotal').val() + $('#ReceiptServiceTotal').val());
    //alert('OK');
    $('#Total').val(parseInt($('#ReceiptDetailTotal').val()) + parseInt($('#ReceiptServiceTotal').val()));
    $('#txtTotal').val(parseInt($('#Total').val()).toLocaleString());
}

//Valdidation using jquery
function validateReceiptService() {
    //alert("Validating");
    var isValid = true;
    if ($('#ServiceName').val().trim() == "0") {
        $("<label id='ServiceId_error' class='error'>Thông tin bắt buộc nhập.</label>").insertAfter("#ServiceId");
        isValid = false;
    }
    else {
        $('#ServiceId_error').remove();
    }
    if ($('#ServiceUnitId').val().trim() == "0") {
        $("<label id='ServiceUnitId_error' class='error'>Thông tin bắt buộc nhập.</label>").insertAfter("#ServiceUnitId");
        isValid = false;
    }
    else {
        $('#ServiceUnitId_error').remove();
    }
    if ($('#ServiceQuantity').val().trim() == "") {
        $("<label id='ServiceQuantity_error' class='error'>Thông tin bắt buộc nhập.</label>").insertAfter("#ServiceQuantity");
        isValid = false;
    }
    else {
        if (!$.isNumeric($('#ServiceQuantity').val().trim())) {
            $("<label id='ServiceQuantity_error' class='error'>Chỉ được phép nhập số.</label>").insertAfter("#ServiceQuantity");
            isValid = false;
        }
        else {
            $('#ServiceQuantity_error').remove();
        }
    }
    if ($('#ServiceUnitPrice').val().trim() == "") {
        $("<label id='ServiceUnitPrice_error' class='error'>Thông tin bắt buộc nhập.</label>").insertAfter("#ServiceUnitPrice");
        isValid = false;
    }
    else {
        if (!$.isNumeric($('#ServiceUnitPrice').val().trim())) {
            $("<label id='ServiceUnitPrice_error' class='error'>Chỉ được phép nhập số.</label>").insertAfter("#ServiceUnitPrice");
            isValid = false;
        }
        else {
            $('#ServiceUnitPrice_error').remove();
        }
    }
    return isValid;
}

function BindingPaymentMethod() {
    $.ajax({
        url: "/PaymentMethod/List",
        type: "GET",
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (result) {
            //console.log(result);
            $.each(result, function (key, item) {
                $("#PaymentMethod").append("<option value='" + item.ID + "'>" + item.Name + "</option>");
            });
        },
        error: function (errormessage) {
            console.log(errormessage);
            //alert(errormessage.responseText);
        }
    });
}
function BindingCustomers() {
    $.ajax({
        url: "/Customer/List",
        type: "GET",
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (result) {
            //console.log(result);
            $.each(result, function (key, item) {
                $("#CustomerId").append("<option value='" + item.ID + "'>" + item.Name + "</option>");
            });
        },
        error: function (errormessage) {
            console.log(errormessage);
            //alert(errormessage.responseText);
        }
    });
}



function PrintReceipt(_receiptId) {
    //alert(_receiptId);
    $.ajax({
        url: "/Receipt/GetReportUrl",
        data: {
            ReceiptId: _receiptId
        },
        type: "GET",
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (result) {
            //alert(result);
            window.open(result, "_blank");
            //console.log(result);
            //$.each(result, function (key, item) {
            //    $("#CustomerId").append("<option value='" + item.ID + "'>" + item.Name + "</option>");
            //});
        },
        error: function (errormessage) {
            console.log(errormessage);
            alert("Có lỗi xảy ra khi tạo báo cáo");
            //alert(errormessage.responseText);
        }
    });
}




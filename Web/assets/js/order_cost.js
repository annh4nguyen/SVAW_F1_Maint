function bindStatusList() {
 
}
//Load Data function
function loadOrderCosts() {
    //alert($('#OrderId').val());
    $.ajax({
        url: "/OrderCost/List",
        data: {
            OrderId: $('#OrderId').val()
        },
        type: "GET",
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (result) {
            var html = '';
            var _totaldetail = 0;
            var i = 1;
            $.each(result, function (key, item) {

                html += '<tr>';
                html += '<td>' + i + '</td>';
                html += '<td>' + item.Description + '</td>';
                html += '<td>' + item.Amount.toLocaleString() + '</td>';
                _totaldetail += item.Amount;
                html += '<td><a href="#" onclick="return getOrderCostByID(' + item.ID + ')"><i class="fa fa-pencil"></i>Sửa</a> | <a href="#" onclick="DeleteOrderCost(' + item.ID + ')"><i class="fa fa-trash-o"></i>&nbsp;Xóa</a></td>';
                i++;
                html += '</tr>';
            });
            //alert(html);

            $('#listOrderCost').html(html);
            $('#TotalAmount').val(_totaldetail.toLocaleString());
            //UpdateReceiptTotal();
        },
        error: function (errormessage) {
            console.log(errormessage);
            alert(errormessage.responseText);
        }
    });
}



//Function for getting the Data Based upon Employee ID
function getOrderCostByID(ID) {
    $('#OrderCostId').val(ID); //khi click save se biet dc mode: edit hay create
    $.ajax({
        url: "/OrderCost/GetByID",
        data: {
            ID: ID
        },
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        dataType: "json",
        success: function (result) {
            console.log(result);
            $('#Description').val(result.Description);
            $('#Amount').val(result.Amount);
            $('.error').remove();
        },
        error: function (errormessage) {
            console.log('err: ', errormessage.responseText);
        }
    });
    return false;
}

function SaveOrderCost() {
    var res = validateOrderCost();
    if (res == false) {
        return false;
    }
    var entity = {
        ID: $('#OrderCostId').val(),
        OrderId: $('#OrderId').val(),
        Description: $('#Description').val(),
        Amount: $('#Amount').val()
    };
    $.ajax({
        url: "/OrderCost/AddOrUpdate",
        data: {
            entity
        },
        type: "POST",
        success: function (result) {
            loadOrderCosts();
            //reset mode
            ResetOrderCostForm();
        },
        error: function (errormessage) {
            console.log("error",errormessage);
        }
    });
}


function DeleteOrderCost(ID) {
    var ans = confirm("Bạn có chắc chắn muốn xóa?");
    if (ans) {
        $.ajax({
            url: "/OrderCost/Delete",
            data: {
                ID : ID
            },
            type: "POST",           
            success: function (result) {
                loadOrderCosts(true);
            },
            error: function (errormessage) {
                //alert(errormessage.responseText);
            }
        });
    }
}


//Function for clearing the textboxes
function ResetOrderCostForm() {
    $('#OrderCostId').val(0);
    $('#Description').val('');
    $('#Amount').val('');
    $('.error').remove();
}


//Valdidation using jquery
function validateOrderCost() {

    $('.error').remove();

    var isValid = true;
    if ($('#Description').val().trim() == "") {
        $("<label id='Description_error' class='error'>Thông tin bắt buộc nhập.</label>").insertAfter("#Description");
        isValid = false;
    }
    else {
        $('#Description_error').remove();
    }
    if ($('#Amount').val().trim() == "") {
        $("<label id='Amount_error' class='error'>Thông tin bắt buộc nhập.</label>").insertAfter("#Amount");
        isValid = false;
    }
    else {

        if (!$.isNumeric($('#Amount').val().trim())) {
            $("<label id='Amount_error' class='error'>Chỉ được phép nhập số.</label>").insertAfter("#Amount");
            isValid = false;
        }
        else {
            $('#Amount_error').remove();
        }
    }

    return isValid;
}



function PrintOrder(_orderId) {
    //alert(_receiptId);
    $.ajax({
        url: "/Order/GetOrderReportUrl",
        data: {
            OrderId: _orderId
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




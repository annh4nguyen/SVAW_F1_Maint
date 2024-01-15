function bindStatusList() {
 
}
//Load Data function
function loadOrderDetails(refreshTotal) {
    $.ajax({
        url: "/OrderDetail/List",
        data: {
            parentid: $('#OrderId').val()
        },
        type: "GET",
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (result) {
            var html = '';
            var _totaldetail = 0;
            $.each(result, function (key, item) {
                _date = FormatDate(new Date(parseInt(item.ReceiptDate.substr(6))));

                html += '<tr>';
                html += '<td>' + _date + '</td>';
                html += '<td>' + item.CustomerName + '</td>';
                html += '<td>' + item.Receiver + '</td>';
                html += '<td>' + item.Name + '</td>';
                html += '<td>' + item.UnitName+ '</td>';
                html += '<td>' + item.InvoiceNo + '</td>';
                html += '<td>' + item.Quantity.toLocaleString() + '</td>';
                html += '<td>' + item.UnitPrice.toLocaleString() + '</td>';
                html += '<td>' + item.Amount.toLocaleString() + '</td>';
                _totaldetail += item.Amount;
                html += '<td>' + item.Description + '</td>';
                html += '<td>' + item.ReceiverAddress + '</td>';
                html += '<td><a href="#" onclick="DeleteOrderDetail(' + item.ID + ')"><i class="fa fa-trash-o"></i>&nbsp;Xóa</a></td>';
                html += '</tr>';
            });
            $('#listOrderDetails').html(html);
            if (refreshTotal)
                $('#Total').val(_totaldetail.toLocaleString());
            $('#txtTotal').val(_totaldetail.toLocaleString());
            //UpdateReceiptTotal();
        },
        error: function (errormessage) {
            console.log(errormessage);
            //alert(errormessage.responseText);
        }
    });
}


//Load Data function
function loadOrderCostDetails() {
    $.ajax({
        url: "/OrderDetail/List",
        data: {
            parentid: $('#OrderId').val()
        },
        type: "GET",
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (result) {
            var html = '';
            var _totaldetail = 0;

            $.each(result, function (key, item) {
                _date = FormatDate(new Date(parseInt(item.ReceiptDate.substr(6))));

                html += '<tr>';
                html += '<td>' + _date + '</td>';
                html += '<td>' + item.CustomerName + '</td>';
                html += '<td>' + item.Receiver + '</td>';
                html += '<td>' + item.Name + '</td>';
                html += '<td>' + item.Quantity.toLocaleString() + '</td>';
                _totaldetail += item.DeliveryCost;
                html += '<td>' + item.Description + '</td>';
                html += '<td>' + item.ReceiverAddress + '</td>';
                html += '<td><input type="text" id="txtCost_' + item.ID + '" name="txtCost_' + item.ID + '" class="form-control" value=' + item.DeliveryCost + '></td>';
                html += '<td>';
                html += '<select id="cboProviderId_' + item.ID + '" name="cboProviderId_' + item.ID + '" class="form-control"></select>';
                html += '<input type="hidden" id="ProviderId_' + item.ID + '" name="ProviderId_' + item.ID + '" class="form-control" value=' + item.DeliveryProviderId + '></select>';
                html += '</td > ';
                html += '</tr>';
            });
            $('#listOrderDetails').html(html);
            $('#txtDeliveryCost').val(_totaldetail.toLocaleString());

            SetSelect2();
        },
        error: function (errormessage) {
            console.log(errormessage);
            //alert(errormessage.responseText);
        }
    });
}

function SetSelect2() {
    $.ajax({
        url: "/Provider/List",
        //data: {
        //    parentid: $('#OrderId').val()
        //},
        type: "GET",
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (result) {
            var DataSet = []; 

            //DataSet.push({
            //    id: 0,
            //    text: '---'
            //})

            $.each(result, function (key, item) {
                DataSet.push({
                    id: item.ID,
                    text: item.Name
                })
                //console.log(item.ID + ':' + item.Name);
            });

            $("select[name^='cboProviderId_']").each(function () {
                $(this).select2({
                    data: DataSet
                });
                var _name = this.name;
                var arrName = _name.split('_');
                $(this).val($("#ProviderId_" + arrName[1]).val());
                $(this).select2().trigger('change');

            });

        },
        error: function (errormessage) {
            console.log(errormessage);
            //alert(errormessage.responseText);
        }
    });

}

function SaveCost() {
    var strUpdate = '';
    $("select[name^='cboProviderId_']").each(function () {
        var _name = this.name;
        var arrName = _name.split('_');

        var _id = arrName[1];
        var _cost = $("#txtCost_" + arrName[1]).val();
        var _providerid = $(this).val();

        strUpdate += _id + ',' + _providerid + ',' + _cost + ';'
    });
    console.log(strUpdate);
    $.ajax({
        url: "/OrderDetail/SaveCost",
        data: {
            OrderId: $('#OrderId').val(),
            UpdateParameter: strUpdate
        },
        type: "GET",
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (result) {
            window.location.replace("/Order/Cost/" + result);
        },
        error: function (errormessage) {
            //console.log(errormessage);
            alert('Có lỗi xảy ra: ' + errormessage.responseText);
        }
    });

}

////Load Data function
//function loadInventory() {
//    $.ajax({
//        url: "/Inventory/ListInventory",
//        data: {
//            FromDepartmentId: $("#FromDepartmentId").val()
//        },
//        type: "GET",
//        contentType: "application/json;charset=utf-8",
//        dataType: "json",
//        success: function (result) {
//            var html = '';
//            $.each(result, function (key, item) {
//                //var _date = dateFormat(new Date(parseInt(item.ReceiptDate.substr(6))), );
//                _date = FormatDate(new Date(parseInt(item.ReceiptDate.substr(6))));
//                //var _date = item.ReceiptDate.toDateString;
//                html += '<tr>';
//                html += '<td>' + _date + '</td>';
//                html += '<td>' + item.CustomerName + '</td>';
//                html += '<td>' + item.DetailName + '</td>';
//                html += '<td>' + item.DetailUnitName + '</td>';
//                html += '<td>' + item.DetailDescription + '</td>';
//                html += '<td>' + item.DetailQuantity + '</td>';
//                html += '<td>' + item.DetailRemain + '<input type="hidden" id="Remain_' + item.DetailId + '" name="Remain_' + item.DetailId + '" value="' + item.DetailRemain + '" /></td>';
//                html += '<td><input type="text" size=10 id="Pick_' + item.DetailId + '" name="Pick_' + item.DetailId + '" class="form-control col-md-1 col-xs-12" /> </td>';
//                html += '</tr>';
//            });
//            $('#listInventory').html(html);
//        },
//        error: function (errormessage) {
//            console.log(errormessage);
//            //alert(errormessage.responseText);
//        }
//    });
//}

//function SaveInventory2Order(_orderId) {
//    //var _orderId = $("#OrderId").val();

//    //alert(_orderId);

//    $('[id^="Pick_"]').each(function () {
//        var element = $(this);
//        var DetailId = element.attr('id').split('_')[1];
//        var _value = parseInt(element.val());
//        if (isNaN(_value)) { _value = 0; }

//        if (_value < 0) { _value = 0; }

//        var _remain = parseInt($("#Remain_" + DetailId).val());

//        if (_value > _remain) {
//            _value = _remain;
//            alert(_value);
//        }
//        if (_value > 0) {
//            //alert(DetailId);
//            SaveOrderDetail(_orderId, DetailId, _value);
//        }
//    });
//    loadOrderDetails();
//    loadInventory();
//}

//function SaveOrderDetail(_orderId, DetailId, value) {
//    $.ajax({
//        url: "/OrderDetail/UpdateByReceiptDetail",
//        data: {
//            OrderId: _orderId,
//            ReceiptDetailId: DetailId,
//            Picked: value
//        },
//        type: "POST",
//        success: function (result) {
//        },
//        error: function (errormessage) {
//            console.log("error", errormessage);
//            alert("Có lỗi");
//        }
//    });
//}

function FormatDate(_date)
{
    var dd = _date.getDate();
    var mm = _date.getMonth() + 1; //January is 0!

    var yyyy = _date.getFullYear();
    if (dd < 10) {
        dd = '0' + dd;
    }
    if (mm < 10) {
        mm = '0' + mm;
    }

    return dd + '/' + mm + '/' + yyyy;

}



function DeleteOrderDetail(ID) {
    var ans = confirm("Bạn có chắc chắn muốn xóa?");
    if (ans) {
        $.ajax({
            url: "/OrderDetail/Delete",
            data: {
                ID : ID
            },
            type: "POST",           
            success: function (result) {
                loadOrderDetails(true);
                loadInventory();
            },
            error: function (errormessage) {
                //alert(errormessage.responseText);
            }
        });
    }
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




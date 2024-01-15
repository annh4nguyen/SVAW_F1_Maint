$(document).ready(function () {
    $("#RatingID").select2({
        //placeholder: "Chọn",
        //allowClear: true
    });
    $("#Area1Id").select2({
        //placeholder: "Chọn",
        //allowClear: true
    });
    $("#Area2Id").select2({
        //placeholder: "Chọn",
        //allowClear: true
    });
    $("#Area3Id").select2({
        //placeholder: "Chọn",
        //allowClear: true
    });
    $("#LeaderId").select2({
        //placeholder: "Chọn",
        //allowClear: true
    });


    //Form OrderReturn (Create va Edit)
    $("#OrderCode").select2({
        //placeholder: "Chọn",
        //allowClear: true
    });

    $("#select_SSCode").select2({
        minimumInputLength: 1,  // minimumInputLength for sending ajax request to server
        ajax: {
            url: "/AutoComplete/FillCode1",
            data: function (params) {
                var query = {
                    sscode: params.term
                }
                // Query parameters will be ?search=[term]&type=public
                return query;
            },
            processResults: function (data) {
                return {
                    results: data.items
                };
            }
        }
    });


    var DataSourceVendorCode = []; //thu nghiem thay the datasource (ko dung cach call server de lay VendorCode)
    $("#select_VendorCode").select2({
        minimumInputLength: 0,  // minimumInputLength for sending ajax request to server
        data: DataSourceVendorCode
        //ajax: {
        //    url: "/AutoComplete/FillCode2",
        //    data: function (params) {
        //        var query = {
        //            //sscodeId: $('#select_SSCode').val(),
        //            sscode: $('#select_SSCode').find('option:selected').text(),
        //            vendorcode: params.term
        //        }
        //        // Query parameters will be ?search=[term]&type=public
        //        return query;
        //    },
        //    processResults: function (data) {
        //        return {
        //            results: data.items
        //        };
        //    }
        //}
    });

 
    $("#select_SSCode").on('select2:select', function (e) {
        var data = e.params.data;
        //console.log(data);
        $('#SSCode').val(data.text);

        //tu fill vendorcode va cac field con lai neu chi co 1 SSCode
        $.ajax({
            url: '/AutoComplete/FillCode1',
            data: {
                sscode: $('#SSCode').val()
            },
            success: function (data) {
                if (data.items.length == 1) {
                    //append result to select_VendorCode
                    var optionSelectVendorCode = new Option(data.items[0].VendorCode, 1, false, false);
                    $('#select_VendorCode')
                        .empty()
                        .append(optionSelectVendorCode).trigger('change');

                    $('#VendorCode').val(data.items[0].VendorCode);
                    $('#Mo_del').val(data.items[0].Mo_del);
                    $('#Division').val(data.items[0].Division);
                    $('#Vendor').val(data.items[0].Vendor);
                } else if (data.items.length >1) {
                    //tao datasource cho dropdown VendorCode
                    DataSourceVendorCode = [];
                    for (var i = 0, l = data.items.length; i < l; i++) {
                        var obj = data.items[i];
                        DataSourceVendorCode.push({
                            id: i,
                            text: obj.VendorCode,
                            Mo_del: obj.Mo_del,
                            Division: obj.Division,
                            Vendor: obj.Vendor
                        })
                    }
                }
            }

        });
        //end
    });

    $('#select_VendorCode').on('select2:select', function (e) {
        var data = e.params.data;
        $('#VendorCode').val(data.text);
        $('#Mo_del').val(data.Mo_del);
        $('#Division').val(data.Division);
        $('#Vendor').val(data.Vendor);
    });

    $('#JobDate').datepicker({
        autoclose: true,
        format: 'yyyy/mm/dd'
    })

    $('#StartDate').datepicker({
        autoclose: true,
        format: 'yyyy/mm/dd'
    })

    $('#EndDate').datepicker({
        autoclose: true,
        format: 'yyyy/mm/dd'
    })

    //Order screen
    $('#DateOrder').datepicker({
        autoclose: true,
        format: 'yyyy/mm/dd'
    })

    //$('#TransportStartDate').inputmask('dd/mm/yyyy', { 'placeholder': 'dd/mm/yyyy' })
    //$('#TransportEndDate').inputmask('dd/mm/yyyy', { 'placeholder': 'dd/mm/yyyy' })
    //$('#DateLimit').inputmask('dd/mm/yyyy', { 'placeholder': 'dd/mm/yyyy' })
    //$('#DateReport').inputmask('dd/mm/yyyy', { 'placeholder': 'dd/mm/yyyy' })

    $('#TransportStartDate').datepicker({
        autoclose: true,
        format: 'yyyy/mm/dd'
    })
    $('#TransportEndDate').datepicker({
        autoclose: true,
        format: 'yyyy/mm/dd'
    })
  
    $('#DateReport').datepicker({
        autoclose: true,
        format: 'yyyy/mm/dd'
    })


})
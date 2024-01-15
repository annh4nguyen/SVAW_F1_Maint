$(function () {

    var AndonTimers = [];

    //$("#btnViewData").click(function (event) {
    //    event.preventDefault();
    //    //alert("something wrong!");
    //    //var url = '@Url.Action("AutoGenerate", "Node", new { FactoryId = "FACTORY_ID" })';
    //    var url = '/Monitoring/ViewLayout?FactoryId=FACTORY_ID';
    //    url = url.replace("FACTORY_ID", $("#FactoryId").val());
    //    //alert(url); //just for debugging
    //    window.location.href = url;
    //});
    $(".toggle-expand-btn").click(function (e) {
        $(this).closest('.box').toggleClass('panel-fullscreen');
        $('#divLogo').toggleClass('toggle-monitoring ');
    });


    // update Monitoring 
    function updateMonitoring(message) {
        //Message structure
        //Nodeid|nodename|eventid|eventstatus|_eventtime|_data;
        //alert(message);
        var nodeMsgArr = message.split(';');
        console.log('Total Msg:' + message);
        //alert(nodeMsgArr.length);
        for (var i = 0; i < nodeMsgArr.length;i++) {
            var _nodeMsg = nodeMsgArr[i];
            if (_nodeMsg != "") {
                console.log('Msg:' + _nodeMsg);
                var arrMsg = _nodeMsg.split('|');

                var nodeId = arrMsg[0];
                var nodeName = arrMsg[1];
                var status = arrMsg[2];
                var eventTime = arrMsg[3];//.toString('yyyy/MM/dd HH:mm:ss');
                //alert(eventTime);
                var datashow = arrMsg[4];

                var planned = arrMsg[5];

                var arrStatus = status.split('~');

                //Hiện tại chỉ lấy mỗi chân IN1 mà thôi
                var _nodeStatus = arrStatus[0];
                //var zColor = '#FFF';
                var x;
                if (_nodeStatus == '01') { //Trạng thái chạy
                    x = $("#EventDef_1").css('background-color');
                }
                if (_nodeStatus == '00') { //Trạng thái dừng
                    x = $("#EventDef_2").css('background-color');
                }
                if (_nodeStatus == '11') { //Trạng thái mất kết nối
                    x = $("#EventDef_3").css('background-color');
                }
                zColor = x;

                //Set value into Textbox
                var element = $('#Time_' + nodeId);
                //alert(arrMsg[4]);

                //if (elementEventDef.val() != arrMsg[2]) {
                //    //Clear Timer in Element
                clearInterval(element.attr("data-timer-id"));
                //}

                //var elementStatus = element.find('#Status');
                //elementStatus.val(_nodeStatus);
                element.find('#Status').val(_nodeStatus);
                element.find('#UpdateTime').val(eventTime);

                if (planned.toUpperCase() == "WORKING" || planned.toUpperCase() == "STOP") {
                    element.find('#Planned').val(planned);
                }
                CheckToRunCountingTime(element);
                //if (_nodeStatus == '11') { //Trạng thái mất kết nối
                //    element.css("display", "none");
                //}
            }
        }

    }


 var uri = "ws://localhost:5000/ws";

        socket = new WebSocket(uri);
        function connect() {
           
            socket.onopen = function (e) {
                console.log("connection estabished");
            };
            socket.onclose = function (e) {
                console.log("connection closed");
            }
            socket.onmessage  = function (e) {
				 updateMonitoring(e.data);
            }
            socket.onerror = function (e) {
                console.log(e);
            }
        }
        connect();
	//===================================endsocket===========================
	
	
	
	

    $.fn.blink = function (speed, blink) {
        for (var i = 0; i < blink; i++) {
            this.fadeOut(speed);
            this.fadeIn(speed);
        }
        return this; // To support jQuery chain-ability
    };

    /*$.fn.CheckToRunCountingTime = */
    function CheckToRunCountingTime(element) {
        // check null
        if (typeof element.attr('id') === "undefined" || element.attr('id') === null){
            return;
        }

        // Cache very important elements, especially the ones used always
        var NodeId = element.attr('id').split('_')[1];
        var hoursElement = element.find('.hours');
        //alert(hoursElement.text());
        var minutesElement = element.find('.minutes');
        var secondsElement = element.find('.seconds');
        var UpdateTime = element.find('#UpdateTime').val();
        var Status = element.find('#Status').val();
        var NodeElement = $('#Node_' + NodeId);
        var NodePlanned = element.find('#Planned').val();
        var zColor = "#FFF";

        var MaxStopTime = $('#TimeOut_' + NodeId).val();
        
        //var isRunning = parseInt(Status) > 1;

        var hours, minutes, seconds, timer;

        if (Status == "01") {
            zColor = $("#EventDef_1").css('background-color');
        }
        if (Status == "00") {
			if (NodePlanned == "STOP"){
				zColor = $("#EventDef_0").css('background-color');
			}
			else {
				zColor = $("#EventDef_2").css('background-color');
			}
        }
        if (Status == "11") {
            zColor = $("#EventDef_3").css('background-color');
        }
        //if (NodeId == "1") {
        //    alert(zColor);
        //}
        if (Status == "11") {
            element.css('display', 'none');
        }
        else {
            element.css('display', 'block');
        }
        // And it's better to keep the state of time in variables 
        var starttime = Date.parse(UpdateTime.toString());// split(' ')[1];

            NodeElement.css('background-color', zColor);
            NodeElement.css('color', '#FFF');

            var timeElapsed = Date.now() - starttime;
            //alert(timeElapsed);
            hours = (timeElapsed / 3600000);
            minutes = (timeElapsed / 60000) % 60;
            seconds = (timeElapsed / 1000) % 60;

            /* if (isRunning) */
            runTimer();
            element.attr("data-timer-id", timer);

        //}
        //else {
        //    setStopwatch(0, 0, 0);
        //    $('#Node_' + element.attr('id').split('_')[1]).css('background-color', '#FFF');
        //    $('#Node_' + element.attr('id').split('_')[1]).css('color', '#000');
        //    element.css('display', 'none');
        //}


        function prependZero(time, length) {
            // Quick way to turn number to string is to prepend it with a string
            // Also, a quick way to turn floats to integers is to complement with 0
            time = '' + (time | 0);
            // And strings have length too. Prepend 0 until right.
            while (time.length < length) time = '0' + time;
            return time;
        }


        function setStopwatch(hours, minutes, seconds) {
            // Using text(). html() will construct HTML when it finds one, overhead.
            hoursElement.text(prependZero(hours, 2));
            minutesElement.text(prependZero(minutes, 2));
            secondsElement.text(prependZero(seconds, 2));
        }

        // Update time in stopwatch - every 1s
        function runTimer() {
            // Using ES5 Date.now() to get current timestamp            
      
            //var prevHours = hours;
            //var prevMinutes = minutes;
            //var prevSeconds = seconds;
            timer = setInterval(function () {
                seconds++;
                if (seconds >= 60) {
                    seconds = 0;
                    minutes++;
                    if (minutes >= 60) {
                        minutes = 0;
                        hours++;
                    }
                }

                setStopwatch(hours, minutes, seconds);
                if ((Status == "00") && (NodePlanned != "STOP")) {
						var totalMinutes = (hours | 0) * 60 + (minutes | 0);  //(timeElapsed / 60000)

						if (parseInt(totalMinutes) >= parseInt(MaxStopTime)) { //Qua thoi gian
							 //if (NodeId == "9") {
								 //   alert(MaxStopTime);
								 //alert(totalMinutes);
							 //}

							var timeElapsed_OddEven = parseInt(seconds) % 2;
							//alert(seconds);   
							if (timeElapsed_OddEven == 0) {
								zColor = "#800000";
							}
							else {
								zColor = $("#EventDef_2").css('background-color');
							}
							NodeElement.css('background-color', zColor);
                    }

                }
            }, 1000);

   
        }

    }
    //});


    $('[id^="Time_"]').each(function () {
        var element = $(this);

        CheckToRunCountingTime(element);
    });


});

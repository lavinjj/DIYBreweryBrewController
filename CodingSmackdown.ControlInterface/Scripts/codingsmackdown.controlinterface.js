function updateCurrentTemp() {
    // [{"sensorMilliVolts": "711.740", "timeOfReading": "11\/29\/2011 04:09:31", "temperatureFahrenheit": "70.112", "temperatureCelsius": "21.174"}]
    // show the activity widget
    $('#loadingWidget').show();
    // send a request to the server to get the current temperature from the sensor
    $.getJSON("temperature", "", function (j) {
        // walk the responses and check to see if there was a failure
        for (var i = 0; i < j.length; i++) {
            $("#timeOfReading").html(j[i].timeOfReading);
            $("#sensorMilliVolts").html(j[i].sensorMilliVolts);
            $("#tempCelsius").html(j[i].temperatureCelsius);
            $("#tempFahrenheit").html(j[i].temperatureFahrenheit);
        }

        // hide the activity widget
        $('#loadingWidget').hide();
    });
}

function getSettings() {
    // [{"netbiosName": "NETDUINOPLUS", "minutesBetweenNTPUpdate": "600000", "ntpServerName": "pool.ntp.org", "historyFilename": "temperatures.txt", "timeZoneOffset": "0", "temperatureOffset": "10", "minutesBetweenReadings": "600000"}]
    // show the activity widget
    $('#loadingWidget').show();
    // send a request to the server to retreive the config values
    $.getJSON("settings", "", function (j) {
        // walk the responses and check to see if there was a failure
        for (var i = 0; i < j.length; i++) {
            $("#minutesBetweenReadings").val(j[i].minutesBetweenReadings);
            $("#temperatureOffset").val(j[i].temperatureOffset);
            $("#historyFilename").val(j[i].historyFilename);
            $("#ntpServerName").val(j[i].ntpServerName);
            $("#minutesBetweenNTPUpdate").val(j[i].minutesBetweenNTPUpdate);
            $("#timeZoneOffset").val(j[i].timeZoneOffset);
            $("#netbiosName").val(j[i].netbiosName);
            $("#enableDHCP").attr('checked', j[i].enableDHCP);
            $("#staticIPAddress").val(j[i].staticIPAddress);
            $("#subnetMask").val(j[i].subnetMask);
            $("#defaultGateway").val(j[i].defaultGateway);
            $("#primaryDNSAddress").val(j[i].primaryDNSAddress);
            $("#secondaryDNSAddress").val(j[i].secondaryDNSAddress);
        }

        // hide the activity widget
        $('#loadingWidget').hide();
    });
}

function updateSettings() {
    // show the activity widget
    $('#loadingWidget').show();

    // send a request to the server to update the config values
    $.getJSON("updateSettings", { minutesBetweenReadings: $("#minutesBetweenReadings").val(), temperatureOffset: $("#temperatureOffset").val(), historyFilename: $("#historyFilename").val(),
        ntpServerName: $("#ntpServerName").val(), minutesBetweenNTPUpdate: $("#minutesBetweenNTPUpdate").val(), timeZoneOffset: $("#timeZoneOffset").val(),
        netbiosName: $("#netbiosName").val(), enableDHCP: $("#enableDHCP").val(), staticIPAddress: $("#staticIPAddress").val(), subnetMask: $("#subnetMask").val(),
        defaultGateway: $("#defaultGateway").val(), primaryDNSAddress: $("#primaryDNSAddress").val(), secondaryDNSAddress: $("#secondaryDNSAddress").val()
    }, function (j) {

        // hide the activity widget
        $('#loadingWidget').hide();

        $('#errorMessage').html('Please wait while the Temperature Logger reboots and applies the new settings.');
        $('#errorMessageDiv').style('display: block;');

    });
}

// Our ajax data renderer which here retrieves a text file.
// it could contact any source and pull data, however.
// The options argument isn't used in this renderer.
var ajaxDataRenderer = function (url, plot, options) {
    var ret = null;
    $.ajax({
        // have to use synchronous here, else the function 
        // will return before the data is fetched
        type: 'GET',
        async: false,
        url: url,
        success: function (text) {
            // array to return the data in
            var response = [];
            var data = [];
            var temperature;
            // split the csv file into fields that 
            // we can parse
            var fields = text.split(/\n/);

            fields.pop(fields.length - 1);

            var tempData = fields.slice(0, fields.length);

            for (var j = 0; j < tempData.length; j += 1) {

                var dataFields = tempData[j].split(',');
                temperature = parseFloat(dataFields[4]);
                data.push(temperature);
            }

            response.push(data);
            ret = response;
        }
    });
    return ret;
};

function updateHistory() {
    // show the activity widget
    $('#loadingWidget').show();

    // The url for our json data
    var jsonurl = "temperatures.txt";

    // passing in the url string as the jqPlot data argument is a handy
    // shortcut for our renderer.  You could also have used the
    // "dataRendererOptions" option to pass in the url.
    var plot2 = $.jqplot('chartdiv', jsonurl, {
        dataRenderer: ajaxDataRenderer,
        dataRendererOptions: {
            unusedOptionalUrl: jsonurl
        }
    });

    // hide the activity widget
    $('#loadingWidget').hide();
}
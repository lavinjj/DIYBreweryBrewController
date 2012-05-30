// namespace definition
// define the orderTalk namespace
var cs = cs || {};


cs.sensorData = function () {
    var self = this;
    self.timeOfReading = ko.observable('');
    self.sensorMilliVolts = ko.observable('');
    self.temperatureCelsius = ko.observable('');
    self.temperatureFahrenheit = ko.observable('');
};

// [{"sensorMilliVolts": "711.740", "timeOfReading": "11\/29\/2011 04:09:31", "temperatureFahrenheit": "70.112", "temperatureCelsius": "21.174"}]
cs.sensorDataFromWire = function (sensorData) {
    var self = this;
    self.timeOfReading = ko.observable(sensorData.timeOfReading);
    self.sensorMilliVolts = ko.observable(sensorData.sensorMilliVolts);
    self.temperatureCelsius = ko.observable(sensorData.temperatureCelsius);
    self.temperatureFahrenheit = ko.observable(sensorData.temperatureFahrenheit);
};

cs.settingsData = function () {
    var self = this;
    self.netbiosName = ko.observable('');
    self.minutesBetweenNTPUpdate = ko.observable('');
    self.ntpServerName = ko.observable('');
    self.historyFilename = ko.observable('');
    self.timeZoneOffset = ko.observable('');
    self.temperatureOffset = ko.observable('');
    self.minutesBetweenReadings = ko.observable('');
    self.enableDHCP = ko.observable('');
    self.staticIPAddress = ko.observable('');
    self.subnetMask = ko.observable('');
    self.defaultGateway = ko.observable('');
    self.primaryDNSAddress = ko.observable('');
    self.secondaryDNSAddress = ko.observable('');
    self.voltageReference = ko.observable('');
    self.padResistance = ko.observable('');
    self.resistanceRT = ko.observable('');
    self.coefficientA = ko.observable('');
    self.coefficientB = ko.observable('');
    self.coefficientC = ko.observable('');
    self.coefficientD = ko.observable('');
};

// [{"netbiosName": "NETDUINOPLUS", "minutesBetweenNTPUpdate": "600000", "ntpServerName": "pool.ntp.org", "historyFilename": "temperatures.txt", "timeZoneOffset": "0", 
// "temperatureOffset": "10", "minutesBetweenReadings": "600000", "enableDHCP": "true", "staticIPAddress": "0.0.0.0", "subnetMask": "255.255.255.0", "defaultGateway": "0.0.0.0", 
// "primaryDNSAddress": "192.168.1.1", "secondaryDNSAddress": "0.0.0.0", "voltageReference" : "3.3" , "padResistance" : "10000", "resistanceRT" : "10000",
// "coefficientA" : "0.003354016", "coefficientB" : "0.0002744032", "coefficientC" : "0.000003666944", "coefficientD" : "0.0000001375492"}]
cs.settingsDataFromWire = function (settings) {
    var self = this;
    self.netbiosName = ko.observable(settings.netbiosName);
    self.minutesBetweenNTPUpdate = ko.observable(settings.minutesBetweenNTPUpdate);
    self.ntpServerName = ko.observable(settings.ntpServerName);
    self.historyFilename = ko.observable(settings.historyFilename);
    self.timeZoneOffset = ko.observable(settings.timeZoneOffset);
    self.temperatureOffset = ko.observable(settings.temperatureOffset);
    self.minutesBetweenReadings = ko.observable(settings.minutesBetweenReadings);
    self.enableDHCP = ko.observable(settings.enableDHCP);
    self.staticIPAddress = ko.observable(settings.staticIPAddress);
    self.subnetMask = ko.observable(settings.subnetMask);
    self.defaultGateway = ko.observable(settings.defaultGateway);
    self.primaryDNSAddress = ko.observable(settings.primaryDNSAddress);
    self.secondaryDNSAddress = ko.observable(settings.secondaryDNSAddress);
    self.voltageReference = ko.observable(settings.voltageReference);
    self.padResistance = ko.observable(settings.padResistance);
    self.resistanceRT = ko.observable(settings.resistanceRT);
    self.coefficientA = ko.observable(settings.coefficientA);
    self.coefficientB = ko.observable(settings.coefficientB);
    self.coefficientC = ko.observable(settings.coefficientC);
    self.coefficientD = ko.observable(settings.coefficientD);
};

cs.viewModel = function () {
    // private data
    sensorData = ko.observable(new cs.sensorData()),
    settings = ko.observable(new cs.settingsData()),
    mashProfile = ko.observableArray([]),
    mashTemperature = ko.observable(''),
    mashStepLength = ko.observable(''),

    gridViewModel = new ko.simpleGrid.viewModel({
        data: mashProfile,
        columns: [
        { headerText: "Step Number", rowText: "step" },
        { headerText: "Temperature", rowText: "temperature" },
        { headerText: "Length", rowText: "minutes" }
        ],
        pageSize: 10
    }),

    addItem = function () {
        var step = mashProfile().length;
        mashProfile().push({ step: step, temperature: mashTemperature(), minutes: mashStepLength() });
    },

    jumpToFirstPage = function () {
        this.gridViewModel.currentPageIndex(0);
    },

    // methods
    updateCurrentTemp = function () {
        // show the activity widget
        $('#loadingWidget').show();
        // send a request to the server to get the current temperature from the sensor
        $.getJSON('temperature', '', function (j) {
            // walk the responses and check to see if there was a failure
            for (var i = 0; i < j.length; i++) {
                sensorData(new cs.sensorDataFromWire(j[i]));
            }
            // hide the activity widget
            $('#loadingWidget').hide();
        });
    },

    getSettings = function () {
        // show the activity widget
        $('#loadingWidget').show();
        // send a request to the server to retreive the config values
        $.getJSON('settings', '', function (j) {
            // walk the responses and check to see if there was a failure
            for (var i = 0; i < j.length; i++) {
                settings(new cs.settingsDataFromWire(j[i]));
            }
            // hide the activity widget
            $('#loadingWidget').hide();
        });
    },

    updateSettings = function () {
        // show the activity widget
        $('#loadingWidget').show();

        // send a request to the server to update the config values
        $.getJSON("updateSettings", { minutesBetweenReadings: settings().minutesBetweenReadings(), temperatureOffset: settings().temperatureOffset(),
            historyFilename: settings().historyFilename(), ntpServerName: settings().ntpServerName(), minutesBetweenNTPUpdate: settings().minutesBetweenNTPUpdate(),
            timeZoneOffset: settings().timeZoneOffset(), netbiosName: settings().netbiosName(), enableDHCP: settings().enableDHCP(),
            staticIPAddress: settings().staticIPAddress(), subnetMask: settings().subnetMask(), defaultGateway: settings().defaultGateway(),
            primaryDNSAddress: settings().primaryDNSAddress(), secondaryDNSAddress: settings().secondaryDNSAddress(),
            voltageReference: settings().voltageReference(), padResistance: settings().padResistance(), resistanceRT: settings().resistanceRT(),
            coefficientA: settings().coefficientA(), coefficientB: settings.coefficientB(), coefficientC: settings.coefficientC(),
            coefficientD: settings().coefficientD()
        }, function (j) {

            // hide the activity widget
            $('#loadingWidget').hide();

            $('#errorMessage').html('Please wait while the Temperature Logger reboots and applies the new settings.');
            $('#errorMessageDiv').style('display: block;');

        });
    };

    return {
        sensorData: sensorData,
        settings: settings,
        updateCurrentTemp: updateCurrentTemp,
        getSettings: getSettings,
        updateSettings: updateSettings,
        mashProfile: mashProfile,
        mashTemperature: mashTemperature,
        mashStepLength: mashStepLength,
        gridViewModel: gridViewModel,
        addItem: addItem,
        jumpToFirstPage: jumpToFirstPage
    };
} ();


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
                temperature = parseFloat(dataFields[3]);
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
    var jsonurl = 'temperatures.txt';

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

$(document).ready(function () {
    // set up the tabs on the page
    $('#tabs').tabs();
    // set up the buttons on the tabs
    $('#refreshCurrentReading').button();
    $('#refreshHistory').button();
    $('#updateSettings').button();
    $('#updateProbeSettings').button();
    $('#addMashStep').button();
    cs.viewModel.getSettings();
    cs.viewModel.updateCurrentTemp();
    ko.applyBindings(cs.viewModel);
    // pull the history
    updateHistory();
    // update the graph every 10 minutes
    setInterval('updateHistory()', 30000);
});

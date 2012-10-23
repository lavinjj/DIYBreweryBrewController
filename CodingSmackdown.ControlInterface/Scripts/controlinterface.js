// namespace definition
// define the orderTalk namespace
var cs = cs || {};


cs.sensorData = function () {
    var self = this;
    self.timeOfReading = ko.observable('');
    self.sensorMilliVolts = ko.observable('');
    self.temperatureCelsius = ko.observable('');
    self.temperatureFahrenheit = ko.observable('');
    self.isHeating = ko.observable('');
    self.currentMashStep = ko.observable('');
    self.currentMashTemp = ko.observable('');
    self.currentMashTime = ko.observable('');
    self.pidOutput = ko.observable('');
};

// [{"sensorMilliVolts": "711.740", "timeOfReading": "11\/29\/2011 04:09:31", "temperatureFahrenheit": "70.112", "temperatureCelsius": "21.174"}]
cs.sensorDataFromWire = function (sensorData) {
    var self = this;
    self.timeOfReading = ko.observable(sensorData.timeOfReading);
    self.sensorMilliVolts = ko.observable(sensorData.sensorMilliVolts);
    self.temperatureCelsius = ko.observable(sensorData.temperatureCelsius);
    self.temperatureFahrenheit = ko.observable(sensorData.temperatureFahrenheit);
    self.isHeating = ko.observable(sensorData.isHeating);
    if (sensorData.currentMashStep !== undefined) {
        self.currentMashStep = ko.observable(sensorData.currentMashStep);
    }
    else {
        self.currentMashStep = ko.observable('');
    }
    if (sensorData.currentMashTemp !== undefined) {
        self.currentMashTemp = ko.observable(sensorData.currentMashTemp);
    }
    else {
        self.currentMashTemp = ko.observable('');
    }
    if (sensorData.currentMashTime !== undefined) {
        self.currentMashTime = ko.observable(sensorData.currentMashTime);
    }
    else {
        self.currentMashTime = ko.observable('');
    }
    if (sensorData.pidOutput !== undefined) {
        self.pidOutput = ko.observable(sensorData.pidOutput);
    }
    else {
        self.pidOutput = ko.observable('');
    }
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
    self.kpValue = ko.observable('');
    self.kiValue = ko.observable('');
    self.kdValue = ko.observable('');
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
    self.kpValue = ko.observable(settings.kpValue);
    self.kiValue = ko.observable(settings.kiValue);
    self.kdValue = ko.observable(settings.kdValue);
};

cs.viewModel = function () {
    // private data
    sensorData = ko.observable(new cs.sensorData()),
    settings = ko.observable(new cs.settingsData()),
    mashProfile = ko.observableArray([]),
    mashTemperature = ko.observable(''),
    mashStepLength = ko.observable(''),
    temperatureData = ko.observableArray([]),
    heaterData = ko.observableArray([]),
    pidData = ko.observableArray([]),

    gridViewModel = new ko.simpleGrid.viewModel({
        data: this.mashProfile,
        columns: [
        { headerText: "Step", rowText: "step" },
        { headerText: "Temperature", rowText: "temperature" },
        { headerText: "Length", rowText: "minutes" }
        ],
        pageSize: 10
    }),

    addItem = function () {
        var stepNumber = mashProfile().length;
        mashProfile.push({ step: stepNumber, temperature: mashTemperature(), minutes: mashStepLength() });
    },

    jumpToFirstPage = function () {
        gridViewModel.currentPageIndex(0);
    },

    // methods
    updateCurrentTemp = function () {
        // send a request to the server to get the current temperature from the sensor
        $.getJSON('temperature', '', function (j) {
            // walk the responses and check to see if there was a failure
            for (var i = 0; i < j.length; i++) {
                sensorData(new cs.sensorDataFromWire(j[i]));

                var temperature = parseFloat(j[i].temperatureFahrenheit);
                temperatureData().push(temperature);
                var pid = parseFloat(j[i].pidOutput);
                pidData().push(pid);
                var heater = 0;
                if (j[i].isHeating === "true") {
                    heater = 100
                }
                else {
                    heater = 0
                }
                heaterData().push(heater);

                // passing in the url string as the jqPlot data argument is a handy
                // shortcut for our renderer.  You could also have used the
                // "dataRendererOptions" option to pass in the url.
                $('#chartdiv').html('');

                var plot2 = $.jqplot('chartdiv', [cs.viewModel.temperatureData(), cs.viewModel.heaterData(), cs.viewModel.pidData()], {
                    // Give the plot a title.
                    title: 'Temperature',
                    // You can specify options for all axes on the plot at once with
                    // the axesDefaults object.  Here, we're using a canvas renderer
                    // to draw the axis label which allows rotated text.
                    axesDefaults: {
                        labelRenderer: $.jqplot.CanvasAxisLabelRenderer
                    },
                    // An axes object holds options for all axes.
                    // Allowable axes are xaxis, x2axis, yaxis, y2axis, y3axis, ...
                    // Up to 9 y axes are supported.
                    axes: {
                        // options for each axis are specified in separate option objects.
                        xaxis: { label: "Time", pad: 0 },
                        yaxis: { label: "Degrees F" },
                        y2axis: { label: "Heater" },
                        y3axis: { label: "PID Output" }
                    }
                });
            }
        });
    },

    getSettings = function () {
        // show the activity widget
        $('#loadingWidget').show();
        // send a request to the server to retrieve the config values
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
            coefficientD: settings().coefficientD(), kpValue: settings.kpValue(), kiValue: settings.kiValue(), kdValue: settings.kdValue()
        }, function (j) {

            // hide the activity widget
            $('#loadingWidget').hide();

            $('#errorMessage').html('Please wait while the Temperature Logger reboots and applies the new settings.');
            $('#errorMessageDiv').style('display: block;');

        });
    },

    updateMashProfile = function () {
        var dataString = '';

        for (var i = 0; i < mashProfile().length; i++) {
            dataString = dataString + mashProfile()[i].step + ':' + mashProfile()[i].temperature + ':' + mashProfile()[i].minutes + ',';
        }


        // show the activity widget
        $('#loadingWidget').show();

        // send a request to the server to update the config values
        $.getJSON("updateMashProfile", { mashProfile: dataString }, function (j) {

            // hide the activity widget
            $('#loadingWidget').hide();

            $('#errorMessage').html('Please wait while the Temperature Logger reboots and applies the new settings.');
            $('#errorMessageDiv').style('display: block;');

        });

    };

    return {
        sensorData: sensorData,
        settings: settings,
        temperatureData: temperatureData,
        heaterData: heaterData,
        pidData: pidData,
        updateCurrentTemp: updateCurrentTemp,
        getSettings: getSettings,
        updateSettings: updateSettings,
        mashProfile: mashProfile,
        mashTemperature: mashTemperature,
        mashStepLength: mashStepLength,
        gridViewModel: gridViewModel,
        addItem: addItem,
        jumpToFirstPage: jumpToFirstPage,
        updateMashProfile: updateMashProfile
    };
} ();

$(document).ready(function () {
    // set up the tabs on the page
    $('#tabs').tabs();
    // set up the buttons on the tabs
    $('#refreshCurrentReading').button();
    $('#refreshHistory').button();
    $('#updateSettings').button();
    $('#updateProbeSettings').button();
    $('#addMashStep').button();
    $('#updateMashProfile').button();
    cs.viewModel.getSettings();
    cs.viewModel.updateCurrentTemp();
    ko.applyBindings(cs.viewModel);
    // update the graph every 30 seconds
    setInterval('cs.viewModel.updateCurrentTemp()', 30000);
});

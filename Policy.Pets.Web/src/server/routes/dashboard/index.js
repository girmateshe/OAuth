module.exports = function (app) {

    var AWS = require("aws-sdk");
    AWS.config.update({
        region: "us-west-2"
    });

    var docClient = new AWS.DynamoDB.DocumentClient();

    var baseUrl = '/api/dashboard/',
        previewurl = baseUrl + ':channelId/preview/:time',
        telemetryUrl = baseUrl + ':channelId/telemetry/:time',
        alertsUrl = baseUrl + ':channelId/alerts',
        detailUrl = baseUrl + ':channelId';

    app.get(baseUrl, function (req, res, next) {

        console.log("Querying dashboard");

        var params = {
            TableName: "RadixCIDashboardChannels"
        };

        var items;

        docClient.scan(params, function (err, data) {
            if (err) {
                console.log("Unable to query. Error:", JSON.stringify(err, null, 2));
            } else {
                console.log("Scan succeeded.");


                var channels = [];
                data.Items.forEach(function (item) {
                    channels.push({
                        href: buildChannelUrl(req, item),
                        Id: item.Id,
                        ChannelId: item.Id,
                        Name: item.Name,
                        Description: item.Description,
                        CreatedTime: item.CreatedTime,
                        LastUpdateTime: item.LastUpdateTime,
                        ClockTime: item.ClockTime,
                        AventusPublicUrl: item.AventusPublicUrl,
                        ScaleUnitId: item.ScaleUnitId,
                        Health: item.Health,
                        ChannelStatus: item.Status,
                        Telemetry: buildTelemetryUrl(req, item),
                        Preview: buildPreviewUrl(req, item),
                        Alerts: buildAlertsUrl(req, item)
                    });
                });

                res.json(channels);
            }
        });


    });

    var buildChannelUrl = function (req, item) {
        var host = req.protocol + '://' + req.get('host');

        if (item.Status != 'OnAir' || item.ClockTime == null) {
            return null;
        }

        return host + detailUrl.replace(/:channelId/g, item.Id)
    }

    var buildTelemetryUrl = function (req, item) {
        var host = req.protocol + '://' + req.get('host');

        if (item.Status != 'OnAir' || item.ClockTime == null) {
            return null;
        }

        return host + telemetryUrl.replace(/:channelId/g, item.Id).replace(/:time/g, item.ClockTime);
    }

    var buildPreviewUrl = function (req, item) {
        var host = req.protocol + '://' + req.get('host');

        if (item.Status != 'OnAir' || item.ClockTime == null) {
            return null;
        }

        return host + previewurl.replace(/:channelId/g, item.Id).replace(/:time/g, item.ClockTime);
    }

    var buildAlertsUrl = function (req, item) {
        var host = req.protocol + '://' + req.get('host');

        if (item.Status != 'OnAir' || item.ClockTime == null) {
            return null;
        }

        return host + alertsUrl.replace(/:channelId/g, item.Id);
    }

    app.get(detailUrl, function (req, res, next) {
        var channelId = req.params.channelId;
        res.send({ ChannelId: channelId });
    });

    app.get(previewurl, function (req, res, next) {
        var channelId = req.params.channelId,
            time = req.params.time;
        res.send({ ChannelId: channelId, ClockTime: time });
    });

    app.get(telemetryUrl, function (req, res, next) {
        var channelId = req.params.channelId,
            time = req.params.time;
        res.send({ ChannelId: channelId, ClockTime: time });
    });

    app.get(alertsUrl, function (req, res, next) {
        var channelId = req.params.channelId;
        res.send({ ChannelId: channelId });
    });
};
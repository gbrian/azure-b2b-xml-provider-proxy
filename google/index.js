require('@google/cloud-debug').start();
var parseString = require('xml2js').parseString;
var request = require('request');
var compression = require('compression')();

function xml2json(xml, callback){
	parseString(xml, function(err, json){ callback(json) });
}
function downloadXml(url, callback){
	request(url, function (error, response, body) {
		if (!error && response.statusCode == 200) {
			callback(body) 
		}else{
			callback("error");
		}
	})
}

exports.b2b_xml_provider_proxy = function b2b_xml_provider_proxy(req, res) {
	res.set('Content-Type', 'application/json');
	compression(req, res, function() {
		doTheJob(req, res);
	});
}
function doTheJob(req, res){
	var url = req.query.url;
	var callback = function(data){ res.send(data); };
	downloadXml(url, 
		function(xml){ 
			xml2json(xml, callback)
		});
};
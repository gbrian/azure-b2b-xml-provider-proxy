require('@google/cloud-debug').start();
exports.b2b_xml_provider_proxy = function b2b_xml_provider_proxy(req, res) {
	console.log(req);
	res.send('Hello World!...1');
};
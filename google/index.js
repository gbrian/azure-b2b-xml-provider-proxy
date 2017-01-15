require('@google/cloud-debug').start();
exports.b2b-xml-provider-proxy = function (req, res) {
	console.log(req);
	res.send('Hello World!...1');
};
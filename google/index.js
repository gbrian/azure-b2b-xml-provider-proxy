require('@google/cloud-debug').start();
exports.helloGET = function (req, res) {
	console.log(req);
	res.send('Hello World!...1');
};
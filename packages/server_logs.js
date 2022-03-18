var fs  = require('fs');
 
process.stdout.wr = process.stdout.write;
process.stdout.er = process.stderr.write;
    
process.stdout.write = function(mes, c) {
    fs.appendFile('server.log', mes + '\r\n', function (err) {
        if (err) throw err;
    });
    process.stdout.wr(mes, c)   
};
 
process.stderr.write = function(mes, c) {
    fs.appendFile('server.log', mes + '\r\n', function (err) {
        if (err) throw err;
    });
    process.stdout.er(mes, c)   
};
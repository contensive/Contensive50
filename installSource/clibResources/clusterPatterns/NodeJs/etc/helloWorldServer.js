var http = require("http");
http.createServer(function(request, response) {
  response.writeHead(200, {"Content-Type": "text/html"});
    response.write("<p>Hello World</p>");
    response.write( "<img src=\"/cclib/images/3ohguy.gif\">" )
  response.end();
}).listen(8888);
console.log( "Listening on 8888" )

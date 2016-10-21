var express = require('express');
var app = express();
var edge = require('edge');

app.use(express.static('public'));

app.get('/', function (req, res) {
    var appName = __dirname.split("\\").pop();
    getHtmlDocProxy(appName, function( error, docHtml ) {
        console.log('getHtmlDocProxy done');
        if (error) {
            console.log(', error');
        }
        res.write( docHtml );
        //console.log('result docHtml=' + docHtml);
        //res.write('Hello World!');
        //res.write('<img src="/cclib/images/3ohguy.gif">');
        res.end();
    })
});

var server = app.listen(3000, function () {
    var host = server.address().address;
    var port = server.address().port;
    console.log('Server listening on http://%s:%s', host, port);
});
//
var getHtmlDocProxy = edge.func({
    references: [
        'c:\\Windows\\Microsoft.NET\\Framework64\\v4.0.30319\\System.Data.dll'
        ,'..\\bin\\cfw.dll' 
        ,'..\\bin\\cpbase.dll' 
    ]
    ,source: function() {/*
        using System.Threading.Tasks;
        public class Startup {
            public async Task<object> Invoke(string appName) {
                string returnDocHtml = "";
                Contensive.Core.CPClass cp = new Contensive.Core.CPClass();
                if (!cp.clusterOk) {
                    // cluster fail
                } else {
                    // populate cp.context
                    //
                    // initialize application
                    cp.init( (string)appName );
                    if (!cp.appOk) {
                        // app not running
                    } else { 
                        // appSuccess, execute default addon
                        // 
                        returnDocHtml = cp.execute();
                    }
                }
                //cp.Dispose;
                return returnDocHtml;
            }
        }
    */}
})

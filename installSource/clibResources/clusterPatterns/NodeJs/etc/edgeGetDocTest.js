var edge = require('edge');
//
var getHtmlDocProxy = edge.func({
    source: function() {/*
        //async ( appName ) => {
        //    string returnDocHtml;
        //    Contensive.Core.CPClass cp = new Contensive.Core.CPClass();
        //    cp.init( (string)appName );
        //    returnDocHtml = cp.getDoc( false );
        //    cp.Dispose;
        //    return returnDocHtml;
        //}
        //
        using System.Threading.Tasks;
        public class Startup {
            public async Task<object> Invoke(string appName) {
                string returnDocHtml = "";
                Contensive.Core.CPClass cp = new Contensive.Core.CPClass();
                if (!cp.clusterOk) {
                    // cluster fail
                } else {
                    // populate cp.context
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
    */},
    references: [
        'c:\\Windows\\Microsoft.NET\\Framework64\\v4.0.30319\\System.Data.dll'
        ,'..\\..\\..\\bin\\cfw.dll' 
        ,'..\\..\\..\\bin\\cpbase.dll' 
    ]
})
//
//
//var getCp = edge.func(function () {/*
//    async (input) => { 
//        return ".NET Welcomes " + input.ToString(); 
//    }
//*/});
//
//getCp('JavaScript', function (error, result) {
//    if (error) throw error;
//    console.log('helloWorld result=' + result);
//});
//
//var nodeInitProxy = edge.func({
//    assemblyFile: 'cfw.dll',
//    typeName: 'Contensive.Core.CPClass',
//    methodName: 'nodeInit',
//   references: ['c:\\Windows\\Microsoft.NET\\Framework64\\v4.0.30319\\System.Data.dll']
//});

//nodeInitProxy('test85n', function (error, result) {
//    console.log('cfwInitProxy done');
//    if (error) {
//        console.log(', error');
//    }
//    console.log(', result=' + result);
//})
//    console.log('calling init...')
//    nodeInitProxy('test85n', function (error, result) {
//        console.log('cfwInitProxy done');
//        if (error) {
//            console.log(', error');
//        }
//        console.log(', result=' + result);
//    })
//    },5000
//)
//getHtmlDocProxy( 'test85n', function( error, docHtml ) {
//    console.log('getHtmlDocProxy done');
//    if (error) {
//        console.log(', error');
//    }
//    console.log('result docHtml=' + docHtml);
//})
    
//(function() {
    var appName = __dirname.split("\\").pop();
    console.log( "app [" + appName + "], Start countdown from 10" )
    var count=10;
    var counter=setInterval(timer, 1000); //1000 will  run it every 1 second
    function timer() {
        console.log( '\n' + count )
        count=count-1;
        if (count <= 0) {
            clearInterval(counter);
            
            getHtmlDocProxy( appName, function( error, docHtml ) {
                console.log('getHtmlDocProxy done');
                if (error) {
                    console.log(', error');
                }
                console.log('result docHtml=' + docHtml);
            })
            
        return;
        }
    }
//})();
// Option Explicit On
// Option Strict On
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using System.Web;
using System.Threading;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using System;

namespace Controllers {
    
    // 
    public class ipDaemonController {
        
        // 
        private object cmdCallbackObject;
        
        private int cmdListenPort;
        
        private string ServerLicense;
        
        private string LocalIPList;
        
        // 
        //  http listener thread and communication object
        // 
        private Thread cmdListenThread;
        
        private const bool onThread = true;
        
        private void thread_cmdListener() {
            // 
            string cmd;
            string queryString;
            string remoteIP;
            string[] prefixes;
            int prefixesCnt = 0;
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddressInfo = null;
            IPHostEntry HostEntry = Dns.GetHostEntry(System.Net.Dns.GetHostName);
            int ptr;
            HttpListenerContext context;
            HttpListenerRequest request;
            HttpListenerResponse response;
            string responseString;
            byte[] buffer;
            System.IO.Stream output;
            string Hint = "enter";
            string hintPrefixes = "";
            try {
                // 
                if (!HttpListener.IsSupported) {
                    throw new ArgumentException("This operating system does not support the required http listen methods");
                }
                else {
                    HttpListener cmdListener;
                    // 
                    object prefixes;
                    prefixes[prefixesCnt] = ("http://127.0.0.1:" 
                                + (cmdListenPort + "/"));
                    prefixesCnt++;
                    // 
                    ",building prefixes";
                    foreach (ipAddressInfo in HostEntry.AddressList) {
                        if ((ipAddressInfo.AddressFamily == AddressFamily.InterNetwork)) {
                            if ((ipAddressInfo.ToString != "127.0.0.1")) {
                                object Preserve;
                                prefixes[prefixesCnt];
                                prefixes[prefixesCnt] = ("http://" 
                                            + (ipAddressInfo.ToString + (":" 
                                            + (cmdListenPort + "/"))));
                                prefixesCnt++;
                            }
                            
                        }
                        
                    }
                    
                    if ((prefixesCnt == 0)) {
                        throw new ArgumentException("No ip addresses are available");
                    }
                    else {
                        // 
                        //  Create a listener.
                        // 
                        ",create listener";
                        cmdListener = new HttpListener();
                        for (ptr = 0; (ptr 
                                    <= (prefixesCnt - 1)); ptr++) {
                            cmdListener.Prefixes.Add(prefixes[ptr]);
                            ("," + prefixes[ptr]);
                        }
                        
                        ",start";
                        cmdListener.Start();
                        for (
                        ; true; 
                        ) {
                            context = cmdListener.GetContext();
                            request = context.Request;
                            response = context.Response;
                            cmd = request.Url.LocalPath;
                            queryString = request.Url.Query;
                            (",cmd=[" 
                                        + (cmd + ("],querystring=[" 
                                        + (queryString + "]"))));
                            if ((queryString.Length > 0)) {
                                if ((queryString.Substring(0, 1) == "?")) {
                                    queryString = queryString.Substring(1);
                                }
                                
                            }
                            
                            remoteIP = request.RemoteEndPoint.Address.ToString;
                            (",remoteIP=[" 
                                        + (remoteIP + "]"));
                            try {
                                ",callback enter";
                                responseString = cmdCallbackObject.ipDaemonCallback(cmd, queryString, remoteIP);
                                ",callback exit";
                            }
                            catch (Exception ex) {
                                // 
                                //  should never return an error to the iDaemon
                                // 
                                My.Computer.FileSystem.WriteAllText("C:\\clibIpDaemonDebug.log", ("\r\n" 
                                                + (Now.ToString() + (" " + ("Exception in callback, hintPrefixes=[" 
                                                + (hintPrefixes + ("], hint=[" 
                                                + (Hint + ("], ex=[" 
                                                + (ex.Message + ("/" 
                                                + (ex.StackTrace + "]"))))))))))), true);
                                responseString = "";
                            }
                            
                            ",set buffer from responseString";
                            if ((responseString.Length <= 0)) {
                                buffer = System.Text.Encoding.Unicode.GetBytes("");
                            }
                            else {
                                buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                            }
                            
                            ",write output from butter";
                            response.ContentLength64 = buffer.Length;
                            response.ContentType = "text/HTML";
                            output = response.OutputStream;
                            output.Write(buffer, 0, buffer.Length);
                            output.Close();
                        }
                        
                        cmdListener.Stop();
                        cmdListener.Abort();
                        // cmdListener = Nothing
                    }
                    
                }
                
            }
            catch (HttpListenerException ex) {
                // 
                // 
                // 
                My.Computer.FileSystem.WriteAllText("C:\\clibIpDaemonDebug.log", ("\r\n" 
                                + (Now.ToString() + (" " + ("HttpListenerException, hintPrefixes=[" 
                                + (hintPrefixes + ("], hint=[" 
                                + (Hint + ("], ex=[" 
                                + (ex.Message + ("/" 
                                + (ex.StackTrace + "]"))))))))))), true);
                // Throw
            }
            catch (Exception ex) {
                // 
                // 
                // 
                My.Computer.FileSystem.WriteAllText("C:\\clibIpDaemonDebug.log", ("\r\n" 
                                + (Now.ToString() + (" " + ("Exception, hintPrefixes=[" 
                                + (hintPrefixes + ("], hint=[" 
                                + (Hint + ("], ex=[" 
                                + (ex.Message + ("/" 
                                + (ex.StackTrace + "]"))))))))))), true);
                // Throw
            }
            
        }
        
        // 
        // ==========================================================================================
        //    Stop listening
        // ==========================================================================================
        // 
        public void startListening(object callbackObject, int listenPort) {
            try {
                // 
                // 
                // 
                cmdListenPort = listenPort;
                cmdCallbackObject = callbackObject;
                if (!onThread) {
                    // 
                    //  start on this thread and block
                    // 
                    this.thread_cmdListener();
                }
                else {
                    // 
                    //  start on a new thread and return
                    // 
                    cmdListenThread = new Thread(new System.EventHandler(this.thread_cmdListener));
                    cmdListenThread.Name = "cmdListen";
                    cmdListenThread.IsBackground = true;
                    cmdListenThread.Start();
                }
                
            }
            catch (Exception ex) {
                // 
                // 
                // 
                throw new ApplicationException("Error during ipDaemon.startListening");
            }
            
        }
        
        // 
        // ==========================================================================================
        //    Stop listening
        // ==========================================================================================
        // 
        public void stopListening() {
            try {
                // 
                //  abort sockets
                // 
                if (!onThread) {
                    // 
                    // 
                    // 
                }
                else if (!(cmdListenThread == null)) {
                    // 
                    // 
                    // 
                    cmdListenThread.Abort();
                }
                
            }
            catch (Exception ex) {
                // 
                // 
                // 
                throw new ApplicationException("Error during ipDaemon.stopListening");
            }
            
        }
    }
}
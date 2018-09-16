using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Contensive.Processor;


namespace Contensive.MonitorService {
    public partial class monitorService : ServiceBase {
        public monitorService() {
            InitializeComponent();
        }

        private StatusServerClass serverStatus;
        private siteCheckClass siteCheck;
        private CPClass cp;

        protected override void OnStart(string[] args) {
            // Add code here to start your service. This method should set things
            // in motion so your service can do its work.
            //
            // store auth token in a config file
            //
            cp = new CPClass();
            serverStatus = new StatusServerClass(cp.core);
            serverStatus.startListening();
            siteCheck = new siteCheckClass(cp.core);
            siteCheck.StartMonitoring();
        }

        protected override void OnStop() {
            // Add code here to perform any tear-down necessary to stop your service.
            serverStatus.stopListening();
            siteCheck.StopMonitoring();
            siteCheck = null;
            serverStatus = null;
            cp.Dispose();
            cp = null;
        }


    }
}

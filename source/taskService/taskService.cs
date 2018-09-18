
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Contensive.Processor.Controllers;
using Contensive.Processor;

namespace Contensive.WindowsServices {
    public partial class taskService : ServiceBase {
        public taskService() {
            InitializeComponent();
        }
        private taskSchedulerController taskScheduler = null;
        private taskRunnerController taskRunner = null;
        //
        protected override void OnStart(string[] args) {
            CPClass cp = new CPClass();
            try {
                //
                logController.logTrace(cp.core, "Services.OnStart enter");
                //
                if (true) {
                    //
                    // -- start scheduler
                    logController.logTrace(cp.core, "Services.OnStart, call taskScheduler.startTimerEvents");
                    taskScheduler = new taskSchedulerController();
                    taskScheduler.startTimerEvents();
                }
                if (true) {
                    //
                    // -- start runner
                    logController.logTrace(cp.core, "Services.OnStart, call taskRunner.startTimerEvents");
                    taskRunner = new taskRunnerController();
                    taskRunner.startTimerEvents();
                }
                logController.logTrace(cp.core, "Services.OnStart exit");
            } catch (Exception ex) {
                logController.handleError(cp.core, ex, "taskService.OnStart Exception");
            }
        }

        protected override void OnStop() {
            CPClass cp = new CPClass();
            try {
                //
                logController.logTrace(cp.core, "Services.OnStop enter");
                //
                if (taskScheduler != null) {
                    //
                    // stop taskscheduler
                    //
                    logController.logTrace(cp.core, "Services.OnStop, call taskScheduler.stopTimerEvents");
                    taskScheduler.stopTimerEvents();
                    taskScheduler.Dispose();
                }
                if (taskRunner != null) {
                    //
                    // stop taskrunner
                    //
                    logController.logTrace(cp.core, "Services.OnStop, call taskRunner.stopTimerEvents");
                    taskRunner.stopTimerEvents();
                    taskRunner.Dispose();
                }
                logController.logTrace(cp.core, "Services.OnStop exit");
            } catch (Exception ex) {
                logController.handleError(cp.core, ex, "taskService.OnStop Exception");
            }
        }
    }
}

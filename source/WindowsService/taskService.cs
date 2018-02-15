using Contensive.Core;
using Contensive.Core.Controllers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace WindowsService {
    public partial class taskService : ServiceBase {
        public taskService() {
            InitializeComponent();
        }
        private taskSchedulerController taskScheduler = null;
        private taskRunnerController taskRunner = null;

        protected override void OnStart(string[] args) {
            CPClass cp = new CPClass();
            try {
                //
                logController.appendLog(cp.core, "Services.OnStart enter");
                //
                if (true) {
                    //
                    // -- start scheduler
                    logController.appendLog(cp.core, "Services.OnStart, call taskScheduler.startTimerEvents");
                    taskScheduler = new taskSchedulerController();
                    taskScheduler.startTimerEvents(true, false);
                }
                if (true) {
                    //
                    // -- start runner
                    logController.appendLog(cp.core, "Services.OnStart, call taskRunner.startTimerEvents");
                    taskRunner = new taskRunnerController();
                    taskRunner.startTimerEvents();
                }
                logController.appendLog(cp.core, "Services.OnStart exit");
            } catch (Exception ex) {
                cp.core.handleException(ex, "taskService.OnStart Exception");
            }
        }

        protected override void OnStop() {
            CPClass cp = new CPClass();
            try {
                //
                logController.appendLog(cp.core, "Services.OnStop enter");
                //
                if (taskScheduler != null) {
                    //
                    // stop taskscheduler
                    //
                    logController.appendLog(cp.core, "Services.OnStop, call taskScheduler.stopTimerEvents");
                    taskScheduler.stopTimerEvents();
                    taskScheduler.Dispose();
                }
                if (taskRunner != null) {
                    //
                    // stop taskrunner
                    //
                    logController.appendLog(cp.core, "Services.OnStop, call taskRunner.stopTimerEvents");
                    taskRunner.stopTimerEvents();
                    taskRunner.Dispose();
                }
                logController.appendLog(cp.core, "Services.OnStop exit");
            } catch (Exception ex) {
                cp.core.handleException(ex, "taskService.OnStop Exception");
            }
        }
    }
}

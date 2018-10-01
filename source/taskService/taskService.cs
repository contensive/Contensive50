
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
        private TaskSchedulerController taskScheduler = null;
        private TaskRunnerController taskRunner = null;
        //
        protected override void OnStart(string[] args) {
            CPClass cp = new CPClass();
            try {
                //
                LogController.logTrace(cp.core, "Services.OnStart enter");
                //
                if (true) {
                    //
                    // -- start scheduler
                    LogController.logTrace(cp.core, "Services.OnStart, call taskScheduler.startTimerEvents");
                    taskScheduler = new TaskSchedulerController();
                    taskScheduler.startTimerEvents();
                }
                if (true) {
                    //
                    // -- start runner
                    LogController.logTrace(cp.core, "Services.OnStart, call taskRunner.startTimerEvents");
                    taskRunner = new TaskRunnerController();
                    taskRunner.startTimerEvents();
                }
                LogController.logTrace(cp.core, "Services.OnStart exit");
            } catch (Exception ex) {
                LogController.handleError(cp.core, ex, "taskService.OnStart Exception");
            }
        }

        protected override void OnStop() {
            CPClass cp = new CPClass();
            try {
                //
                LogController.logTrace(cp.core, "Services.OnStop enter");
                //
                if (taskScheduler != null) {
                    //
                    // stop taskscheduler
                    //
                    LogController.logTrace(cp.core, "Services.OnStop, call taskScheduler.stopTimerEvents");
                    taskScheduler.stopTimerEvents();
                    taskScheduler.Dispose();
                }
                if (taskRunner != null) {
                    //
                    // stop taskrunner
                    //
                    LogController.logTrace(cp.core, "Services.OnStop, call taskRunner.stopTimerEvents");
                    taskRunner.stopTimerEvents();
                    taskRunner.Dispose();
                }
                LogController.logTrace(cp.core, "Services.OnStop exit");
            } catch (Exception ex) {
                LogController.handleError(cp.core, ex, "taskService.OnStop Exception");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

using Contensive.Processor.Controllers;
using Contensive.Processor;

namespace Contensive.Workers {
    public partial class Service1 : ServiceBase {
        public Service1() {
            InitializeComponent();
        }

        protected override void OnStart(string[] args) {
        }

        protected override void OnStop() {
        }
    }
}



namespace Contensive.Workers {
    public partial class taskService : ServiceBase {
        public taskService() {
            InitializeComponent();
        }
        private TaskSchedulerController taskScheduler = null;
        private TaskRunnerController taskRunner = null;
        //
        protected override void OnStart(string[] args) {
            using (CPClass cp = new CPClass()) {
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
                    LogController.logError(cp.core, ex, "taskService.OnStart Exception");
                }
            }
        }

        protected override void OnStop() {
            using (CPClass cp = new CPClass()) {
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
                    LogController.logError(cp.core, ex, "taskService.OnStop Exception");
                }
            }
        }
    }
}

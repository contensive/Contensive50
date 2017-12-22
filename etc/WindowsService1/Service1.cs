
using System;
using System.ServiceProcess;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using Contensive.Core.Controllers;
using Contensive.Core;

namespace Contensive.TaskService {
    //
    [System.ComponentModel.DesignerCategory("Code")]
    public partial class TaskServices : ServiceBase {
        public TaskServices() {
            InitializeComponent();
        }
        private taskSchedulerController taskScheduler = null;
        private taskRunnerController taskRunner = null;
        //
        //====================================================================================================
        /// <summary>
        /// start the taskrunner and or taskScheduler
        /// </summary>
        /// <param name="args"></param>
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
                handleExceptionResume(cp.core, ex, "OnStart", "Unexpected Error");
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// stop the taskrunner and or taskScheduler
        /// </summary>
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
                handleExceptionResume(cp.core, ex, "OnStop", "Unexpected Error");
            }
        }
        //
        //======================================================================================
        //   Log a reported error
        //======================================================================================
        //
        public void handleExceptionResume(coreClass cpCore, Exception ex, string MethodName, string LogCopy) {
            logController.appendLogWithLegacyRow(cpCore, "(service)", LogCopy, "server", "Services", MethodName, -1, ex.Source, ex.ToString(), true, true, "", "", "");
        }
        //

        //[STAThread]
        //static void Main() {
        //    Application.SetCompatibleTextRenderingDefault(false);
        //    Application.Run(new Services());
        //}

    }
}
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Diagnostics;
//using System.Linq;
//using System.ServiceProcess;
//using System.Text;

//namespace WindowsService1 {
//    [System.ComponentModel.DesignerCategory("Code")]
//    public partial class Service1 : ServiceBase {
//        public Service1() {
//            InitializeComponent();
//        }

//        protected override void OnStart(string[] args) {
//        }

//        protected override void OnStop() {
//        }
//    }
//}

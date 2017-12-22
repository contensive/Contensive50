

using Contensive.Core;
using Controllers;
// 

namespace Controllers {
    
    public class taskRunnerController : IDisposable {
        
        // 
        // ==================================================================================================
        //    Code copied from workerClass to both taskScheduler and taskRunner - remove what does not apply
        // 
        //    taskScheduler queries each application from addons that need to run and adds them to the tasks queue (tasks sql table or SQS queue)
        //        taskScheduleTimer
        // 
        //    taskRunner polls the task queue and runs commands when found
        //        taskRunnerTimer
        // ==================================================================================================
        // 
        // Private cpCore As cpCoreClass
        // 
        private string runnerGuid {
        }
        
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                this.disposed = true;
                if (disposing) {
                    // 
                    //  call .dispose for managed objects
                    // 
                    //  cpCore.dispose()
                }
                
                // 
                //  cp  creates and destroys cmc
                // 
                GC.Collect();
            }
            
        }
        
        // 
        // ====================================================================================================
        // '' <summary>
        // '' Stop all activity through the content server, but do not unload
        // '' </summary>
        public void stopTimerEvents() {
            processTimer.Enabled = false;
        }
        
        // 
        // ====================================================================================================
        // '' <summary>
        // '' Process the Start signal from the Server Control Manager
        // '' </summary>
        // '' <param name="setVerbose"></param>
        // '' <param name="singleThreaded"></param>
        // '' <returns></returns>
        public bool startTimerEvents() {
            bool returnStartedOk = true;
            processTimer = new System.Timers.Timer(ProcessTimerMsecPerTick);
            processTimer.Elapsed += new System.EventHandler(this.processTimerTick);
            processTimer.Interval = ProcessTimerMsecPerTick;
            processTimer.Enabled = true;
            return returnStartedOk;
        }
        
        // 
        // ====================================================================================================
        // '' <summary>
        // '' Timer tick
        // '' </summary>
        protected void processTimerTick(object sender, EventArgs e) {
            try {
                // 
                Console.WriteLine("taskRunnerService.processTimerTick");
                // 
                if (ProcessTimerInProcess) {
                    // 
                    Console.WriteLine("taskRunnerService.processTimerTick, processTimerInProcess true, skip");
                }
                else {
                    ProcessTimerInProcess = true;
                    Using;
                    ((void)(cpCluster));
                    new CPClass();
                    if (!cpCluster.core.serverConfig.allowTaskRunnerService) {
                        Console.WriteLine("taskRunnerService.processTimerTick, allowTaskRunnerService false, skip");
                    }
                    else {
                        runTasks(cpCluster.core);
                    }
                    
                }
                
                ProcessTimerInProcess = false;
            }
            catch (Exception ex) {
                Using;
                ((void)(cp));
                new CPClass();
                cp.core.handleException(ex);
            }
            
        }
    }
}
cpSite.core.db.csSet(CS, "datecompleted", Now());
Endif (cpSite.core.db.csClose(CS)) {
    while (recordsRemaining) {
        ((Exception)(ex));
        cpClusterCore.handleException(ex);
    }
    
}

EndUsing;
NextConsole.WriteLine(("taskRunnerController.runTasks, exit (" 
                + (sw.ElapsedMilliseconds + "ms)")));
CatchException ex;
cpClusterCore.handleException(ex);
Endtry {
}

Endclass End {
}

    
    //  Do not change or add Overridable to these methods.
    //  Put cleanup code in Dispose(ByVal disposing As Boolean).
    public void Dispose() {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected override void Finalize() {
        this.Dispose(false);
        base.Finalize();
    }
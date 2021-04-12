using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using CT3DMachine.XRayControl;
using CT3DMachine.MotionControl;
using CT3DMachine.TurntableControl;
using CT3DMachine.DetectorControl;
using CT3DMachine.Tools;

namespace CT3DMachine.Cycle
{
    class TurnXRayOffTask : TimeoutSyncTask
    {
        private XRayMonitor mXRayMonitor = null;
        public TurnXRayOffTask(int timeout, XRayMonitor xRayMonitor) : base(timeout)
        {
            this.mXRayMonitor = xRayMonitor;
            this.mType = TaskType.TURN_OFF_XRAY;
        }
        protected override TOSResult innerProcess()
        {
            if (!this.mXRayMonitor.turnXRayOff()) return TOSResult.FAILED_INNER_PROC;
            while (this.mRunning)
            {
                if (!this.mXRayMonitor.XRayReady)
                {
                    return TOSResult.SUCCESS;
                }
            }
            return TOSResult.FAILED_TIMEOUT;
        }
    }
}

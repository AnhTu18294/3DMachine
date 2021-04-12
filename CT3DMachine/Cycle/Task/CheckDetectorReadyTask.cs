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

using CT3DMachine.Model;
namespace CT3DMachine.Cycle
{
    class CheckDetectorReadyTask : TimeoutSyncTask
    {
        private DetectorMonitor mDetectorMonitor = null;
        public CheckDetectorReadyTask(int timeout, DetectorMonitor detectorMonitor) : base(timeout)
        {
            this.mDetectorMonitor = detectorMonitor;
            this.mType = TaskType.CHECK_DETECTOR_READY;
        }
        protected override TOSResult innerProcess()
        {
            while (this.mRunning)
            {
                if (this.mDetectorMonitor.State == DetectorState.READY_GET_IMAGE)
                {
                    return TOSResult.SUCCESS;
                }
                Thread.Sleep(TimeSpan.FromMilliseconds(1));
            }
            return TOSResult.FAILED_TIMEOUT;
        }
    }
}

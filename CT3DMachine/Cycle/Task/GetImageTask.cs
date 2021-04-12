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
    class GetImageTask : TimeoutSyncTask
    {
        private DetectorMonitor mDetectorMonitor = null;
        private UInt16 mID = 0;
        public GetImageTask(int timeout, DetectorMonitor detectorMonitor, UInt16 id) : base(timeout)
        {
            this.mDetectorMonitor = detectorMonitor;
            this.mID = id;
            this.mType = TaskType.GET_IMAGE;
        }

        protected override TOSResult innerProcess()
        {
            if (!this.mDetectorMonitor.getImage(this.mID)) return TOSResult.FAILED_INNER_PROC;
            while (this.mRunning)
            {
                if (this.mDetectorMonitor.CurGetImage == this.mDetectorMonitor.CurResImage)
                {
                    return TOSResult.SUCCESS;
                }
                Thread.Sleep(TimeSpan.FromMilliseconds(1));
            }
            return TOSResult.FAILED_TIMEOUT;
        }
    }

}

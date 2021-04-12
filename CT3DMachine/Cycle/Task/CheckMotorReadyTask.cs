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
    class CheckMotorReadyTask : TimeoutSyncTask
    {
        private MotionMonitor mMotionMonitor = null;
        public CheckMotorReadyTask(int timeout, MotionMonitor motionMonitor) : base(timeout)
        {
            this.mMotionMonitor = motionMonitor;
            this.mType = TaskType.CHECK_MOTOR_READY;

        }
        protected override TOSResult innerProcess()
        {
            while (this.mRunning)
            {
                if (this.mMotionMonitor.ReadyMotion)
                {
                    return TOSResult.SUCCESS;
                }
                Thread.Sleep(TimeSpan.FromMilliseconds(1));
            }
            return TOSResult.FAILED_TIMEOUT;
        }
    }
}

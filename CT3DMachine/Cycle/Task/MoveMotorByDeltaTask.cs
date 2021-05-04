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
    class MoveMotorByDeltaTask : TimeoutSyncTask
    {
        private MotionMonitor mMotionMonitor = null;
        private double mRotXDelta = 0;
        private double mDetYDelta = 0;
        private double mRotCDelta = 0;
        private double mDetZDelta = 0;
        private double mXRayZDelta = 0;

        public MoveMotorByDeltaTask(int timeout, MotionMonitor motionMonitor, double rotX, double detY, double rotC, double detZ, double xRayZ) : base(timeout)
        {
            this.mMotionMonitor = motionMonitor;
            this.mRotXDelta = rotX;
            this.mDetYDelta = detY;
            this.mRotCDelta = rotC;
            this.mDetZDelta = detZ;
            this.mXRayZDelta = xRayZ;
            this.mType = TaskType.MOVE_MOTOR;
        }

        protected override TOSResult innerProcess()
        {
            this.mMotionMonitor.moveToPositionByValue(this.mRotXDelta, this.mDetYDelta, this.mRotCDelta, this.mDetZDelta, this.mXRayZDelta);
            while (this.mRunning)
            {
                if (this.mMotionMonitor.isDoneMoving())
                {
                    return TOSResult.SUCCESS;
                }
                Thread.Sleep(TimeSpan.FromMilliseconds(1));
            }
            return TOSResult.FAILED_TIMEOUT;
        }
    }
}

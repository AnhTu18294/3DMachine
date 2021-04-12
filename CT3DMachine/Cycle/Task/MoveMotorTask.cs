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
    class MoveMotorTask : TimeoutSyncTask
    {
        private MotionMonitor mMotionMonitor = null;
        private double mRotXPos = 0;
        private double mDetYPos = 0;
        private double mRotCPos = 0;
        private double mDetZPos = 0;
        private double mXRayZPos = 0;
                
        public MoveMotorTask(int timeout, MotionMonitor motionMonitor, double rotX, double detY, double rotC, double detZ, double xRayZ) : base(timeout)
        {
            this.mMotionMonitor = motionMonitor;
            this.mRotXPos = rotX;
            this.mDetYPos = detY;
            this.mRotCPos = rotC;
            this.mDetZPos = detZ;
            this.mXRayZPos = xRayZ;
            this.mType = TaskType.MOVE_MOTOR;
        }

        protected override TOSResult innerProcess()
        {
            if (!this.mMotionMonitor.goToPosition(this.mRotXPos, this.mDetYPos, this.mRotCPos, this.mDetZPos, this.mXRayZPos)) return TOSResult.FAILED_INNER_PROC;
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

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
    class FinishCycle : TimeoutSyncTask
    {
        protected override TOSResult innerProcess()
        {
            return TOSResult.FAILED_INNER_PROC;
        }
    }
}

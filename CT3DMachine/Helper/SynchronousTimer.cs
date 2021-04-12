using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CT3DMachine.Helper
{
    class SynchronousTimer
    {
        private System.Timers.Timer mTimer = new System.Timers.Timer();
        private bool mRequestStop = false;
        private Mutex mMutex = new Mutex();

        public delegate void TimerTimeoutHandler(object sender);
        public event TimerTimeoutHandler timeout;

        public SynchronousTimer()
        {
            mTimer.Interval = 1000;
            mTimer.Elapsed += onTimerElapsed;
            mTimer.AutoReset = false;
        }

        public SynchronousTimer(int interval)
        {
            mTimer.Interval = interval;
            mTimer.Elapsed += onTimerElapsed;
            mTimer.AutoReset = false;
        }

        private void onTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            mMutex.WaitOne();
            if(!mRequestStop)
            {
                mTimer.Start();
            }
            mMutex.ReleaseMutex();
            if (timeout != null)
            {
                timeout(this);
            }
        }

        public void stop()
        {
            mMutex.WaitOne();
            mRequestStop = true;
            mTimer.Stop();
            mMutex.ReleaseMutex();
        }

        public void start()
        {
            mMutex.WaitOne();
            mRequestStop = false;
            mTimer.Start();
            mMutex.ReleaseMutex();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace CT3DMachine.Cycle
{
    abstract class TimeoutSyncTask
    {
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

        public enum TaskType : byte
        {
            CHECK_DETECTOR_READY = 0x00,
            CHECK_MOTOR_READY = 0x01,
            CHECK_XRAY_READY = 0x02,
            FINISH = 0x03,
            GET_IMAGE = 0x04,
            MOVE_MOTOR = 0x05,
            STOP_MOTOR = 0x06,
            TURN_ON_XRAY = 0x07,
            TURN_OFF_XRAY = 0x08,
            STOP_DETECTOR = 0x09,
            UNKNOWN = 0xFF,
        }

        public enum TOSResult : byte
        {
            SUCCESS = 0x00,
            FAILED_TIMEOUT = 0x01,
            FAILED_INNER_PROC = 0x02,
            FAILED_OUTER_PROC = 0x03
        }

        public enum TOSState : byte
        {
            IDLE = 0x00,
            PROCESSING = 0x01
        }

        protected int mTimeout = -1; //ms;
        protected Task<TOSResult> mTask = null;
        protected bool mRunning = true;
        protected TOSState mState = TOSState.IDLE;
        protected TaskType mType = TaskType.UNKNOWN;

        public TimeoutSyncTask()
        {
            this.mRunning = false;
            this.mState = TOSState.IDLE;
        }

        public TimeoutSyncTask(int _timeout) : this()
        {
            mTimeout = _timeout;
        }

        public int Timeout
        {
            get { return mTimeout; }
            set { mTimeout = value; }
        }

        public void stop()
        {
            mRunning = false;
            while (!mTask.IsCompleted) ;
            this.mState = TOSState.IDLE;
        }

        public TaskType getType()
        {
            return this.mType;
        }
        public TOSResult execute()
        {
            TOSResult res = TOSResult.FAILED_OUTER_PROC;
            if (this.mState == TOSState.PROCESSING) return res;
            this.mState = TOSState.PROCESSING;
            this.mRunning = true;

            this.mTask = Task.Run(() => {
                return this.innerProcess();
            });

            if (this.mTask.Wait(TimeSpan.FromMilliseconds(this.mTimeout)))
            {
                res = this.mTask.Result;
            }
            else
            {
                res = TOSResult.FAILED_TIMEOUT;
            }
            this.stop();
            return res;
        }

        abstract protected TOSResult innerProcess();
    }
}

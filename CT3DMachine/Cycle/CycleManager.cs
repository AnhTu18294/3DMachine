using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using NLog;
using CT3DMachine.XRayControl;
using CT3DMachine.MotionControl;
using CT3DMachine.TurntableControl;
using CT3DMachine.DetectorControl;
using CT3DMachine.Tools;
using CT3DMachine.Notifications;

namespace CT3DMachine.Cycle
{
    public delegate void EventCycleInfoHandler(String info);
    public delegate void EventCycleErrorHandler(String info);
    public delegate void EventCycleWarningHandler(String info);

    public enum CycleState : byte
    {
        IDILE = 0x00,
        INITIALIZING = 0x01,
        INITIALIZED = 0x02,
        PROCESSING = 0x03,
        FINISHING = 0x04,
        DONE = 0x05,
        FORCE_TO_STOP = 0x06        
    }

    public enum CycleError : byte
    {

    }    

    class CycleManager
    {
        public event EventCycleInfoHandler EventCycleInfo;
        public event EventCycleErrorHandler EventCycleError;
        public event EventCycleWarningHandler EventCycleWarning;

        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();
        private XRayMonitor mXRayMonitor = null;
        private MotionMonitor mMotionMonitor = null;
        private TurnableMonitor mTurnableMonitor = null;
        private DetectorMonitor mDetectorMonitor = null;
        private NotificationManager mNotificationManager = null;

        private CycleState mState = CycleState.IDILE;
        private double mStepSize = 1;
        private double mGain = 1.5;
        private double mXRayZPos = -1.0;
        private double mDetYPos = -1.0;
        private double mDetZPos = -1.0;
        private double mTotalRotation = 360.0;
        private TurnableMonitor.SampleType mSampleType = TurnableMonitor.SampleType.SMALL;
        private List<TimeoutSyncTask> mCycleTasks = null;

        public static double Z1_t = 318.8;
        public static double Z1_f = 435.2;
        public static double Y_t = 174.85;
        public static double Y_f = 29.15;
        public static double OD = 51.9;
        public static double OS = 993;
        public static double RRot = 150;

        public static int MOTOR_READY_TIMEOUT = 1000;
        public static int FIRST_MOVE_TIMEOUT = 5000;
        public static int STEP_MOVE_TIMEOUT = 60000;
        public static int GET_IMAGE_TIMEOUT = 1000;
        public static int TURN_ON_XRAY_TIMEOUT = 5000;
        public static int TURN_OFF_XRAY_TIMEOUT = 5000;
        public static int STOP_DETECTOR_TIMEOUT = 5000;

        public static double DET_Y_SIZE = 145.728;
        public static double DET_Z_SIZE = 116.424;

        private void emitInfo(String _info)
        {
            if (this.EventCycleInfo != null)
                this.EventCycleInfo(_info);
        }

        private void emitError(String _error)
        {
            if (this.EventCycleError != null)
                this.EventCycleError(_error);
        }

        private void emitWarning(String _warning)
        {
            if (this.EventCycleWarning != null)
                this.EventCycleWarning(_warning);
        }

        public CycleManager(XRayMonitor xRayMonitor, MotionMonitor motionMonitor, TurnableMonitor turnableMonitor, DetectorMonitor detectorMonitor, NotificationManager notificationManager)
        {
            this.mXRayMonitor = xRayMonitor;
            this.mMotionMonitor = motionMonitor;
            this.mTurnableMonitor = turnableMonitor;
            this.mDetectorMonitor = detectorMonitor;
            this.mNotificationManager = notificationManager;
            mState = CycleState.IDILE;
        }

        public bool createCycleProcess()
        {
            bool res = false;
            if (this.mState != CycleState.IDILE) return res;
            TurnableMonitor.SampleType sampleType = this.mSampleType;
            this.mState = CycleState.INITIALIZING;
           

            switch (sampleType)
            {
                case TurnableMonitor.SampleType.SMALL:
                    {
                        res = this.createCycleProcessForSmallSample();
                        break;
                    }
                case TurnableMonitor.SampleType.MIDDLE:
                    {
                        res = this.createCycleProcessForMiddleSample();
                        break;
                    }
                case TurnableMonitor.SampleType.BIG:
                    {
                        res = this.createCycleProcessForBigSample();
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            if(res == false) {
                this.mCycleTasks.Clear();
                this.mState = CycleState.DONE;               
            }
            else {
                this.mState = CycleState.INITIALIZED;
            }
            this.mNotificationManager.ShowInformation("Finish Init Cycle");
            return res;
        }

        private bool executeCurrentCycle()
        {
            if (this.mState != CycleState.INITIALIZED) return false;            
            int numOfTask = this.mCycleTasks.Count;
            this.mState = CycleState.PROCESSING;
            for(int i = 0; i < numOfTask; i++)
            {
                TimeoutSyncTask.TOSResult res = this.mCycleTasks[i].execute();                
                String result = "Task " + this.mCycleTasks[i].getType().ToString() + ": " + res.ToString();
                Logger.Info("Task {0} result is {1}", i, result);
                //this.EventCycleInfo(result);
                if (this.mState == CycleState.FORCE_TO_STOP)
                {
                    this.mState = CycleState.DONE;
                    return false;
                }
            }
            this.mState = CycleState.IDILE;
            return true;
        }

        public void createCycle()
        {            
            this.mStepSize = this.mTurnableMonitor.getStep();
            this.mXRayZPos = this.mTurnableMonitor.getXRayZPos();
            this.mDetYPos = this.mTurnableMonitor.getDetYPos();
            this.mDetZPos = this.mTurnableMonitor.getDetZPos();

            
            this.mTotalRotation = this.mTurnableMonitor.getTotalRotation();
            this.mSampleType = this.mTurnableMonitor.getSampleType();
            if(this.mSampleType != TurnableMonitor.SampleType.SMALL)
            {
                if ((this.mXRayZPos < 0) || (this.mXRayZPos < 0) || (this.mXRayZPos < 0))
                {
                    this.mNotificationManager.ShowError("Không thể khởi tạo chu trình!!! \nVui lòng kiểm tra lại tham số cấu hình chu trình!");
                    return;
                }
            }
            Logger.Info("====>Create Cycle: step size = {0}, XRayZPos = {1}, total rotation = {2}, DetYPos = {3}, mDetZPos = {4} | [{5}]", 
                this.mTurnableMonitor.getStep(), this.mTurnableMonitor.getXRayZPos(), this.mTurnableMonitor.getTotalRotation(),
                this.mTurnableMonitor.getDetYPos(), this.mTurnableMonitor.getDetZPos(), this.mSampleType);
            var task = Task.Run(() =>
            {
                this.createCycleProcess();
            });
        }

        public void executeCycle()
        {
            Logger.Info("Execute Cycle");
            var task = Task.Run(() =>
            {
                this.executeCurrentCycle();
            });
        }

        public void stopCurrentCycle()
        {
            if (this.mState == CycleState.IDILE) return;
            this.mState = CycleState.FORCE_TO_STOP;
            while (this.mState != CycleState.DONE)
            {
                Logger.Info("Wait to stop the cycle");
            }
            this.mState = CycleState.IDILE;
        }
        
        private bool createCycleProcessForSmallSample()
        {
            mCycleTasks = new List<TimeoutSyncTask>();
            //Collect Cycle params from turnable  
            double stepSize = this.mStepSize;
            double gain = this.mGain;
            double totalRotation = this.mTotalRotation;
            Logger.Info("Create Cycle with: SMALL, step = {0}, gain = {1}, totalRotation = {2}", stepSize, gain, totalRotation);
            // Canculate position
            double rotX = mMotionMonitor.RotX;
            double detY = mMotionMonitor.DetY;
            double rotC = 0;
            double detZ = mMotionMonitor.DetZ;
            double xRayZ = mMotionMonitor.XRayZ;
            int part = 0;


            //// ******Check ready 3D Machine State ****** ////
            //this.mCycleTasks.Add(new CheckMotorReadyTask(MOTOR_READY_TIMEOUT, this.mMotionMonitor));
            //// Move 5 motor to start position
            //this.mCycleTasks.Add(new MoveMotorTask(FIRST_MOVE_TIMEOUT, this.mMotionMonitor, rotX, detY, rotC, detZ, xRayZ));
            //Logger.Info("Add Task MoveMotor {0}, {1}, {2}, {3}, {4}", rotX, detY, rotC, detZ, xRayZ);
            //// Turn XRay On
            //this.mCycleTasks.Add(new TurnXRayOnTask(TURN_ON_XRAY_TIMEOUT, this.mXRayMonitor));

            //// Check ready Detector
            //this.mCycleTasks.Add(new CheckDetectorReadyTask(1000, this.mDetectorMonitor));

            //// ******DO TURN SAMPLE AND IMAGE PROCESS****** ////            
            int numberOfStep = (int)Math.Ceiling(totalRotation / stepSize);

            part = 0;
            rotC = 0;
            this.mCycleTasks.Add(new MoveMotorTask(STEP_MOVE_TIMEOUT, this.mMotionMonitor, rotX, detY, rotC, detZ, xRayZ));
            for (int i = 0; i < numberOfStep; i++)
            {
                rotC = stepSize * i;
                this.mCycleTasks.Add(new MoveMotorTask(STEP_MOVE_TIMEOUT, this.mMotionMonitor, rotX, detY, rotC, detZ, xRayZ));
                Logger.Info("Add Task MoveMotor {0}, {1}, {2}, {3}, {4}", rotX, detY, rotC, detZ, xRayZ);
                this.mCycleTasks.Add(new GetImageTask(GET_IMAGE_TIMEOUT, this.mDetectorMonitor, (UInt16)(i + numberOfStep * part)));
                Logger.Info("Add Task GetImage {0}", (UInt16)(i + numberOfStep * part));

                if (this.mState == CycleState.FORCE_TO_STOP)
                {
                    return false;
                }
            }

            //// Stop Detector
            this.mCycleTasks.Add(new StopDetectorTask(STOP_DETECTOR_TIMEOUT, this.mDetectorMonitor));

            //// Turn off XRay
            this.mCycleTasks.Add(new TurnXRayOffTask(TURN_OFF_XRAY_TIMEOUT, this.mXRayMonitor));

            return true;

        }
        private bool createCycleProcessForMiddleSample()
        {
            mCycleTasks = new List<TimeoutSyncTask>();
            //Collect Cycle params from turnable  
            double stepSize = this.mStepSize;
            double totalRotation = this.mTotalRotation;
            // Canculate position
            double rotX = this.mMotionMonitor.RotX; // get current pos
            double detY = this.mDetYPos;
            double detZ = this.mDetZPos;
            double rotC = 0;
            double xRayZ = this.mXRayZPos;

            int part = 0;                    

            //// ******Check ready 3D Machine State ****** ////
            //this.mCycleTasks.Add(new CheckMotorReadyTask(1000, this.mMotionMonitor));
            //// Move 5 motor to start position
            //this.mCycleTasks.Add(new MoveMotorTask(1000, this.mMotionMonitor, rotX, detY, rotC, detZ, xRayZ));
            //// Check ready Detector
            //this.mCycleTasks.Add(new CheckDetectorReadyTask(1000, this.mDetectorMonitor));
            //// Check XRayReadyImageProcessing
            //this.mCycleTasks.Add(new CheckXRayReadyTask(1000, this.mXRayMonitor));

           
            //// ******DO TURN SAMPLE AND IMAGE PROCESS AT PART I****** ////
            int numberOfStep = (int)Math.Ceiling(totalRotation / stepSize);
            // Move detector to part I
            part = 0;
            rotC = 0;
            detY = this.mDetYPos + DET_Y_SIZE / 2.0;
            detZ = this.mDetZPos - DET_Z_SIZE / 2.0;
            this.mCycleTasks.Add(new MoveMotorTask(STEP_MOVE_TIMEOUT, this.mMotionMonitor, rotX, detY, rotC, detZ, xRayZ));
            for (int i = 0; i < numberOfStep; i++)
            {
                rotC = stepSize * i;
                this.mCycleTasks.Add(new MoveMotorTask(STEP_MOVE_TIMEOUT, this.mMotionMonitor, rotX, detY, rotC, detZ, xRayZ));
                Logger.Info("Add Task MoveMotor {0}, {1}, {2}, {3}, {4}", rotX, detY, rotC, detZ, xRayZ);
                this.mCycleTasks.Add(new GetImageTask(GET_IMAGE_TIMEOUT, this.mDetectorMonitor, (UInt16)(i + numberOfStep * part)));
                Logger.Info("Add Task GetImage {0}", (UInt16)(i + numberOfStep * part));

                if (this.mState == CycleState.FORCE_TO_STOP)
                {
                    return false;
                }
            }

            //// ******DO TURN SAMPLE AND IMAGE PROCESS AT PART II****** ////
            part = 1;
            rotC = 0;
            detY = this.mDetYPos - DET_Y_SIZE / 2.0;
            detZ = this.mDetZPos - DET_Z_SIZE / 2.0;
            this.mCycleTasks.Add(new MoveMotorTask(STEP_MOVE_TIMEOUT, this.mMotionMonitor, rotX, detY, rotC, detZ, xRayZ));
            for (int i = 0; i < numberOfStep; i++)
            {
                rotC = stepSize * i;
                this.mCycleTasks.Add(new MoveMotorTask(STEP_MOVE_TIMEOUT, this.mMotionMonitor, rotX, detY, rotC, detZ, xRayZ));
                Logger.Info("Add Task MoveMotor {0}, {1}, {2}, {3}, {4}", rotX, detY, rotC, detZ, xRayZ);
                this.mCycleTasks.Add(new GetImageTask(GET_IMAGE_TIMEOUT, this.mDetectorMonitor, (UInt16)(i + numberOfStep * part)));
                Logger.Info("Add Task GetImage {0}", (UInt16)(i + numberOfStep * part));

                if (this.mState == CycleState.FORCE_TO_STOP)
                {
                    return false;
                }
            }

            //// ******DO TURN SAMPLE AND IMAGE PROCESS AT PART III****** ////
            part = 2;
            rotC = 0;
            detY = this.mDetYPos - DET_Y_SIZE / 2.0;
            detZ = this.mDetZPos + DET_Z_SIZE / 2.0;
            this.mCycleTasks.Add(new MoveMotorTask(STEP_MOVE_TIMEOUT, this.mMotionMonitor, rotX, detY, rotC, detZ, xRayZ));
            for (int i = 0; i < numberOfStep; i++)
            {
                rotC = stepSize * i;
                this.mCycleTasks.Add(new MoveMotorTask(STEP_MOVE_TIMEOUT, this.mMotionMonitor, rotX, detY, rotC, detZ, xRayZ));
                Logger.Info("Add Task MoveMotor {0}, {1}, {2}, {3}, {4}", rotX, detY, rotC, detZ, xRayZ);
                this.mCycleTasks.Add(new GetImageTask(GET_IMAGE_TIMEOUT, this.mDetectorMonitor, (UInt16)(i + numberOfStep * part)));
                Logger.Info("Add Task GetImage {0}", (UInt16)(i + numberOfStep * part));

                if (this.mState == CycleState.FORCE_TO_STOP)
                {
                    return false;
                }
            }

            //// ******DO TURN SAMPLE AND IMAGE PROCESS AT PART IV****** ////
            part = 3;
            rotC = 0;
            detY = this.mDetYPos + DET_Y_SIZE / 2.0;
            detZ = this.mDetZPos + DET_Z_SIZE / 2.0;
            this.mCycleTasks.Add(new MoveMotorTask(STEP_MOVE_TIMEOUT, this.mMotionMonitor, rotX, detY, rotC, detZ, xRayZ));
            for (int i = 0; i < numberOfStep; i++)
            {
                rotC = stepSize * i;
                this.mCycleTasks.Add(new MoveMotorTask(STEP_MOVE_TIMEOUT, this.mMotionMonitor, rotX, detY, rotC, detZ, xRayZ));
                Logger.Info("Add Task MoveMotor {0}, {1}, {2}, {3}, {4}", rotX, detY, rotC, detZ, xRayZ);
                this.mCycleTasks.Add(new GetImageTask(GET_IMAGE_TIMEOUT, this.mDetectorMonitor, (UInt16)(i + numberOfStep * part)));
                Logger.Info("Add Task GetImage {0}", (UInt16)(i + numberOfStep * part));

                if (this.mState == CycleState.FORCE_TO_STOP)
                {
                    return false;
                }
            }

            //// Stop Detector
            this.mCycleTasks.Add(new StopDetectorTask(STOP_DETECTOR_TIMEOUT, this.mDetectorMonitor));

            //// Turn off XRay
            this.mCycleTasks.Add(new TurnXRayOffTask(TURN_OFF_XRAY_TIMEOUT, this.mXRayMonitor));

            return true;
        }
        private bool createCycleProcessForBigSample()
        {
            mCycleTasks = new List<TimeoutSyncTask>();
            //Collect Cycle params from turnable  
            double stepSize = this.mStepSize;
            double totalRotation = this.mTotalRotation;
            // Canculate position
            double rotX = this.mMotionMonitor.RotX; // get current pos
            double detY = this.mDetYPos;
            double detZ = this.mDetZPos;
            double rotC = 0;
            double xRayZ = this.mXRayZPos;
            int part = 0;
            //// ******Check ready 3D Machine State ****** ////
            //this.mCycleTasks.Add(new CheckMotorReadyTask(1000, this.mMotionMonitor));
            //// Move 5 motor to start position
            //this.mCycleTasks.Add(new MoveMotorTask(1000, this.mMotionMonitor, rotX, detY, rotC, detZ, xRayZ));
            //// Check ready Detector
            //this.mCycleTasks.Add(new CheckDetectorReadyTask(1000, this.mDetectorMonitor));
            //// Check XRayReadyImageProcessing
            //this.mCycleTasks.Add(new CheckXRayReadyTask(1000, this.mXRayMonitor));



            //// ******DO TURN SAMPLE AND IMAGE PROCESS AT PART I****** ////
            int numberOfStep = (int)Math.Ceiling(totalRotation / stepSize);
            // Move detector to part I
            part = 0;
            rotC = 0;
            detY = this.mDetYPos + DET_Y_SIZE / 2.0;
            detZ = this.mDetZPos - DET_Z_SIZE;
            this.mCycleTasks.Add(new MoveMotorTask(STEP_MOVE_TIMEOUT, this.mMotionMonitor, rotX, detY, rotC, detZ, xRayZ));
            for (int i = 0; i < numberOfStep; i++)
            {
                rotC = stepSize * i;
                this.mCycleTasks.Add(new MoveMotorTask(STEP_MOVE_TIMEOUT, this.mMotionMonitor, rotX, detY, rotC, detZ, xRayZ));
                Logger.Info("Add Task MoveMotor {0}, {1}, {2}, {3}, {4}", rotX, detY, rotC, detZ, xRayZ);
                this.mCycleTasks.Add(new GetImageTask(GET_IMAGE_TIMEOUT, this.mDetectorMonitor, (UInt16)(i + numberOfStep * part)));
                Logger.Info("Add Task GetImage {0}", (UInt16)(i + numberOfStep * part));

                if (this.mState == CycleState.FORCE_TO_STOP)
                {
                    return false;
                }
            }

            //// ******DO TURN SAMPLE AND IMAGE PROCESS AT PART VI****** ////
            part = 1;
            rotC = 0;
            detY = this.mDetYPos + DET_Y_SIZE / 2.0;
            detZ = this.mDetZPos;
            this.mCycleTasks.Add(new MoveMotorTask(STEP_MOVE_TIMEOUT, this.mMotionMonitor, rotX, detY, rotC, detZ, xRayZ));
            for (int i = 0; i < numberOfStep; i++)
            {
                rotC = stepSize * i;
                this.mCycleTasks.Add(new MoveMotorTask(STEP_MOVE_TIMEOUT, this.mMotionMonitor, rotX, detY, rotC, detZ, xRayZ));
                Logger.Info("Add Task MoveMotor {0}, {1}, {2}, {3}, {4}", rotX, detY, rotC, detZ, xRayZ);
                this.mCycleTasks.Add(new GetImageTask(GET_IMAGE_TIMEOUT, this.mDetectorMonitor, (UInt16)(i + numberOfStep * part)));
                Logger.Info("Add Task GetImage {0}", (UInt16)(i + numberOfStep * part));

                if (this.mState == CycleState.FORCE_TO_STOP)
                {
                    return false;
                }
            }
            //// ******DO TURN SAMPLE AND IMAGE PROCESS AT PART V****** ////
            part = 2;
            rotC = 0;
            detY = this.mDetYPos + DET_Y_SIZE / 2.0;
            detZ = this.mDetZPos + DET_Z_SIZE;
            this.mCycleTasks.Add(new MoveMotorTask(STEP_MOVE_TIMEOUT, this.mMotionMonitor, rotX, detY, rotC, detZ, xRayZ));
            for (int i = 0; i < numberOfStep; i++)
            {
                rotC = stepSize * i;
                this.mCycleTasks.Add(new MoveMotorTask(STEP_MOVE_TIMEOUT, this.mMotionMonitor, rotX, detY, rotC, detZ, xRayZ));
                Logger.Info("Add Task MoveMotor {0}, {1}, {2}, {3}, {4}", rotX, detY, rotC, detZ, xRayZ);
                this.mCycleTasks.Add(new GetImageTask(GET_IMAGE_TIMEOUT, this.mDetectorMonitor, (UInt16)(i + numberOfStep * part)));
                Logger.Info("Add Task GetImage {0}", (UInt16)(i + numberOfStep * part));

                if (this.mState == CycleState.FORCE_TO_STOP)
                {
                    return false;
                }
            }

            //// Stop Detector
            this.mCycleTasks.Add(new StopDetectorTask(STOP_DETECTOR_TIMEOUT, this.mDetectorMonitor));

            // Turn off XRay
            this.mCycleTasks.Add(new TurnXRayOffTask(TURN_OFF_XRAY_TIMEOUT, this.mXRayMonitor));

            return true;
        }
    }
}

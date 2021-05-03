using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CT3DMachine.Service;
using CT3DMachine.Connector;
using CT3DMachine.Codec;
using CT3DMachine.Model;
using CT3DMachine.Helper;
using NLog;
using System.Windows.Forms;
using CT3DMachine.XRayControl;
using CT3DMachine.MotionControl;
using CT3DMachine.TurntableControl;
using CT3DMachine.DetectorControl;
using CT3DMachine.Tools;
using CT3DMachine.Cycle;
using CT3DMachine.Notifications;

namespace CT3DMachine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();
        
        private XRayMonitor mXRayMonitor = null;
        private MotionMonitor mMotionMonitor = null;
        private TurnableMonitor mTurnableMonitor = null;
        private DetectorMonitor mDetectorMonitor = null;

        private CycleManager mCycleManager = null;
        private readonly NotificationManager mNotificationManager = null;

        public MainWindow()
        {
            InitializeComponent();
            machineControl.EventStart += new EventStartHandler(machineControleEventStart);
            machineControl.EventExecute += new EventExecuteHandler(machineControlEventExecute);
            machineControl.EventStop += new EventStopHandler(machineControleEventStop);

            ////Configure Notification Manager
            mNotificationManager = new NotificationManager();

            ////Detector Service
            this.mDetectorMonitor = this.ct3dControl.detectorControl;
            this.mDetectorMonitor.configureConn(1234, "127.0.0.1", 4321, "127.0.0.1");
            this.mDetectorMonitor.startService();

            ////XRay Service
            this.mXRayMonitor = this.ct3dControl.xRayControl;
            this.mXRayMonitor.configurePath(PathTool.bingPathFromAppDir("conf_deployment"), PathTool.bingPathFromAppDir("logs"));
            //this.mXRayMonitor.configurePath(PathTool.bingPathFromAppDir("conf"), PathTool.bingPathFromAppDir("logs"));
            this.mXRayMonitor.startService();

            ////Motion Service
            this.mMotionMonitor = this.ct3dControl.motionControl;
            this.mMotionMonitor.configurePort("COM5", 9600);
            this.mMotionMonitor.startService();

            ////Turnable Monitor
            this.mTurnableMonitor = this.ct3dControl.turntableControl;
            this.mMotionMonitor.RotCPositionChanged += this.mTurnableMonitor.setCurrentRotC;

            ////Create CycleManager
            mCycleManager = new CycleManager(this.mXRayMonitor, this.mMotionMonitor, this.mTurnableMonitor, this.mDetectorMonitor, this.mNotificationManager);
            mCycleManager.EventCycleInfo += new EventCycleInfoHandler(notifyInfo);
            mCycleManager.EventCycleWarning += new EventCycleWarningHandler(notifyWarning);
            mCycleManager.EventCycleError += new EventCycleErrorHandler(notifyError);

            Logger.Info("=====>XRay Config = conf_deployment");
            Logger.Info("=====>Motor Config = COM5");
        }
        
        void machineControleEventStart()
        {
            // Start creating cycle task
            mNotificationManager.ShowInformation("Start creating cycle task");
            mCycleManager.createCycle();           
        }

        void machineControlEventExecute()
        {
            mNotificationManager.ShowInformation("Executing cycle task");
            mCycleManager.executeCycle();
        }
        
        void machineControleEventStop()
        {
            // Stop all motions
            mNotificationManager.ShowInformation("Stop cycle task");
            mCycleManager.stopCurrentCycle();
        }      
        
        void notifyInfo(String _info)
        {
            mNotificationManager.ShowInformation(_info);
        }
        void notifyWarning(String _warning)
        {
            mNotificationManager.ShowWarning(_warning);
        }

        void notifyError(String _error)
        {
            mNotificationManager.ShowError(_error);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Logger.Info("-----> on Close application");
            mNotificationManager.close();
            Logger.Info("-----> close notification");
            mXRayMonitor.stopService();
            Logger.Info("-----> stop XRay Service");
            mMotionMonitor.stopService();
            Logger.Info("-----> stop Motion Service");
            mDetectorMonitor.stopService();
            Logger.Info("-----> stop Detector Service");
            System.Windows.Application.Current.Shutdown();
        }
    }
}

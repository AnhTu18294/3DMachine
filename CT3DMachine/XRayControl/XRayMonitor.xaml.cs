using System;
using System.IO;
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
using System.Windows.Threading;
using System.ComponentModel;
using XRAYWorXBase;
using XRAYWorXBase.Loader;
using NLog;
using CT3DMachine.Tools;

namespace CT3DMachine.XRayControl
{
    /// <summary>
    /// Interaction logic for XRayMonitor.xaml
    /// </summary>
    public partial class XRayMonitor : UserControl
    {
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();        
        private ITubeInterface tubeInterface = null;
        private DispatcherTimer timer = null;
        private DispatcherTimer mTimerCheckReady = null;

        private bool mNUDHighVoltageFocus = false;
        private bool mNUDEmissionCurrentFocus = false;
        private int mEmissionCurrent = 0;
        private int mHighVoltage = 0;
        private bool mIsStartup = false;

        private string mPathToConfig = PathTool.bingPathFromAppDir("conf_deployment");
        private string mPathToLogs = PathTool.bingPathFromAppDir("logs");

        public void configurePath(string pathToConfig, string pathToLogs)
        {
            mPathToConfig = pathToConfig;
            mPathToLogs = pathToLogs;
        }

        public bool IsStartup
        {
            get { return mIsRunning; }
            set { mIsStartup = value; }
        }

        private bool mXRayReady = false;
        public bool XRayReady
        {
            get { return mXRayReady; }
            set { mXRayReady = value; }
        }

        private bool mStartupState = false;
        public bool StartupState
        {
            get { return mStartupState; }
            set { mStartupState = value; }
        }

        private bool mXRayOnState = false;
        public bool XRayOnState
        {
            get { return mXRayOnState; }
            set { mXRayOnState = value; }
        }

        private bool mXRayOffState = false;
        public bool XRayOffState
        {
            get { return mXRayOffState; }
            set { mXRayOffState = value; }
        }

        private bool mIsRunning = false;
        public bool IsRunning
        {
            get { return mIsRunning; }
            set { mIsRunning = IsRunning; }
        }

        public XRayMonitor()
        {
            InitializeComponent();
            this.setDefaultState();
        }

        public bool startService()
        {
            if (this.mIsRunning) return false;
            
            if (!this.getTubeInterface())
            {
                this.mIsRunning = false;
                return false;
            }            
            if (this.timer == null)
            {
                this.timer = new DispatcherTimer();
            }
            this.timer.Interval = TimeSpan.FromMilliseconds(200);
            timer.Tick += this.syncXRayMonitorState;
            this.timer.Start();

            if (this.mTimerCheckReady == null)
            {
                this.mTimerCheckReady = new DispatcherTimer();
            }
            this.mTimerCheckReady.Interval = TimeSpan.FromSeconds(1);
            this.mTimerCheckReady.Tick += this.onCheckReadyTimerTick;

            return true;
        }

        public bool stopService()
        {
            if (!this.mIsRunning) return false;
            if(this.timer != null)
            {
                if(this.timer.IsEnabled == true) {
                    timer.Tick -= this.syncXRayMonitorState;
                    this.timer.Stop();
                }
            }
            this.unLinkTubeInterfaceEvent();
            this.mIsRunning = false;
            return true;
        }

        public bool startUpXRay()
        {
            if (this.mIsRunning == false) return false;
            if (!this.tubeInterface.StartUp.HasReadWriteAccess()) return false;
            this.mIsStartup = false;
            this.tubeInterface.StartUp.PcDemandValue = true;
            this.lbXRayNotification.Content = "Bắt đầu...";
            this.lbXRayNotification.Foreground = Brushes.Red;            
            return true;
        }

        public bool turnXRayOn()
        {
            if (this.mIsRunning == false) return false;
            if (!this.tubeInterface.XRayOn.HasReadWriteAccess()) return false;
            this.tubeInterface.XRayOn.PcDemandValue = true;
            return true;
        }

        public bool turnXRayOff()
        {
            if (this.mIsRunning == false) return false;
            if (!this.tubeInterface.XRayOff.HasReadWriteAccess()) return false;
            this.tubeInterface.XRayOff.PcDemandValue = true;
            return true;
        }

        public bool refreshXRay()
        {
            if (this.mIsRunning == false) return false;
            if (!this.tubeInterface.Refresh.HasReadWriteAccess()) return false;
            this.tubeInterface.Refresh.PcDemandValue = true;
            return true;
        }

        public bool filamentAdjustXRay()
        {
            if (this.mIsRunning == false) return false;
            if (!this.tubeInterface.Refresh.HasReadWriteAccess()) return false;
            this.tubeInterface.FilamentAdjust.PcDemandValue = true;
            return true;
        }

        public bool centerActiveXRay()
        {
            if (this.mIsRunning == false) return false;
            if (!this.tubeInterface.CenterActive.HasReadWriteAccess()) return false;
            this.tubeInterface.CenterActive.PcDemandValue = true;
            return true;
        }

        public bool startTurbopump()
        {
            if (this.mIsRunning == false) return false;
            if(!this.tubeInterface.Turbopump.HasReadWriteAccess()) return false;
            this.tubeInterface.Turbopump.Start();
            return true;
        }
        public bool stopTurbopump()
        {
            if (this.mIsRunning == false) return false;
            if (!this.tubeInterface.Turbopump.HasReadWriteAccess()) return false;
            this.tubeInterface.Turbopump.Stop();
            return true;
        }

        private void setDefaultState()
        {
            btnXRayOn.IsEnabled = false;
            btnXRayOff.IsEnabled = false;
            nudHighVoltage.IsEnabled = false;
            nudEmissionCurrent.IsEnabled = false;
            btnStartup.IsEnabled = false;
            btnRefresh.IsEnabled = false;
            btnFilamentAdj.IsEnabled = false;
            btnCenter.IsEnabled = false;
        }

        private bool getTubeInterface()
        {            
            var loader = TubeLoader.Instance;
           
            loader.SetCustomPaths(mPathToConfig, mPathToLogs, mPathToLogs);
            try
            {
                var ip = loader.DefaultIpAddress.Ip;
                this.tubeInterface = loader.GetTubeInterface(ip);
                this.tubeInterface.TubeInterfaceError += onTubeInterfaceError;
                this.tubeInterface.Initialized += onTubeInterfaceInitialized;
            }
            catch (Exception e)
            {
                Logger.Error("CanNOT get TubeInterface: {0}", e.ToString());
                return false;
            }
            return true;
        }

        private void onTubeInterfaceError(object sender, TubeInterfaceEventArgs e)
        {
            var message = new StringBuilder();
            message.AppendLine(e.ErrorType + " (" + e.ErrorCode + ") occurred.");
            message.AppendLine(e.ErrorText);
            message.AppendLine(e.ErrorInfo);
            message.AppendLine(e.ErrorHelp);
            MessageBox.Show(message.ToString(), e.ErrorType.ToString(), MessageBoxButton.OK);
        }

        private void onTubeInterfaceInitialized(object sender, EventArgs e)
        {
            this.linkTubeInterfaceEvent();
            this.mIsRunning = true;           
        }

        private void linkTubeInterfaceEvent()
        {
            this.tubeInterface.XrayReady.MonitorValueChanged += onXrayReadyStateChanged;
            this.tubeInterface.StartUp.StateChanged += onStartUpStateChanged;
        }

        private void unLinkTubeInterfaceEvent()
        {
            this.tubeInterface.XrayReady.MonitorValueChanged -= onXrayReadyStateChanged;
            this.tubeInterface.StartUp.StateChanged -= onStartUpStateChanged;
        }

        private void onXrayReadyStateChanged(object sender, EventArgs e)
        {
            this.XRayReady = this.tubeInterface.XrayReady.MonitorValue;
        }

        private void onStartUpStateChanged(object sender, EventArgs e)
        {   
            var cmd = (TubeAutoCommand)sender;
            switch (cmd.State)
            {
                case CommandStates.Acknowledged:
                    this.lbXRayNotification.Content = "Đang thực thi... (Acknowledged)";
                    this.lbXRayNotification.Foreground = Brushes.Red;
                    break;
                case CommandStates.Busy:
                    this.lbXRayNotification.Content = "Đang thực thi... (Busy)";
                    this.lbXRayNotification.Foreground = Brushes.Red;
                    break;
                case CommandStates.Warning:
                    this.lbXRayNotification.Content = "Đang thực thi... interrupted due to a warning: " + cmd.ErrorCode.ToString();
                    this.lbXRayNotification.Foreground = Brushes.Red;
                    break;
                case CommandStates.Error:
                    this.lbXRayNotification.Content = "Đang thực thi... interrupted due to an error: " + cmd.ErrorCode.ToString();
                    this.lbXRayNotification.Foreground = Brushes.Red;
                    break;
                case CommandStates.OK:
                    this.lbXRayNotification.Content = "Thực thi xong";
                    this.lbXRayNotification.Foreground = Brushes.Green;
                    this.mIsStartup = true;
                    break;
            }
        }

        private void onCheckReadyTimerTick(object sender, EventArgs e) { }

        private void syncXRayMonitorState(object sender, EventArgs e)
        {
            if (!this.mIsRunning) return;
            if (this.tubeInterface.XRayOn.HasReadWriteAccess())
            {
                this.btnXRayOn.IsEnabled = !this.tubeInterface.XRayOn.MonitorValue;
            }
            else
            {
                this.btnXRayOn.IsEnabled = false;
            }
            if (this.tubeInterface.XRayOff.HasReadWriteAccess())
            {
                this.btnXRayOff.IsEnabled = !this.tubeInterface.XRayOff.MonitorValue;
            }
            else
            {
                this.btnXRayOff.IsEnabled = false;
            }
            this.btnStartup.IsEnabled = this.tubeInterface.StartUp.HasReadWriteAccess();
            this.btnRefresh.IsEnabled = this.tubeInterface.Refresh.HasReadWriteAccess();
            this.btnFilamentAdj.IsEnabled = this.tubeInterface.FilamentAdjust.HasReadWriteAccess();
            this.btnCenter.IsEnabled = this.tubeInterface.CenterAll.HasReadWriteAccess();
            this.nudEmissionCurrent.IsEnabled = this.tubeInterface.EmissionCurrent.HasReadWriteAccess();
            this.nudHighVoltage.IsEnabled = this.tubeInterface.HighVoltage.HasReadWriteAccess();

            if (this.tubeInterface.XrayReady.HasReadAccess())
            {
                if (this.tubeInterface.XrayReady.MonitorValue)
                {
                    this.lbXRayNotification.Content = "Máy phát sẵn sàng";
                    this.lbXRayNotification.Foreground = Brushes.Green;
                }
                else
                {
                    this.lbXRayNotification.Content = "Máy phát không sẵn sàng";
                    this.lbXRayNotification.Foreground = Brushes.Red;
                }
            }

            if (this.tubeInterface.HighVoltage.HasReadAccess())
            {
                this.mHighVoltage = (int)Math.Ceiling(this.tubeInterface.HighVoltage.MonitorValue);
                this.txtHighVoltage.Text = this.mHighVoltage.ToString();
                if (!this.nudHighVoltage.IsEnabled) { this.nudHighVoltage.TextBoxCtrl.Text = this.txtHighVoltage.Text; }
            }

            if (this.tubeInterface.EmissionCurrent.HasReadAccess())
            {
                this.mEmissionCurrent = (int)Math.Ceiling(this.tubeInterface.EmissionCurrent.MonitorValue);
                this.txtEmissionCurrent.Text = this.mEmissionCurrent.ToString();
                if (!this.nudEmissionCurrent.IsEnabled) { this.nudEmissionCurrent.TextBoxCtrl.Text = this.txtEmissionCurrent.Text; }
            }

            if (this.tubeInterface.Turbopump.HasReadAccess())
            {
                int rotationSpeed = (int)Math.Ceiling(this.tubeInterface.Turbopump.RotationSpeed);
                this.txtTurbopump.Text = rotationSpeed.ToString();

                this.btnTurbopumpOn.IsEnabled = !this.tubeInterface.Turbopump.IsSwitchedOn;
                this.btnTurbopumpOff.IsEnabled = this.tubeInterface.Turbopump.IsSwitchedOn;
            }

            if (this.tubeInterface.VacuumValue.HasReadAccess()) { this.txtVacuum.Text = this.tubeInterface.VacuumValue.MonitorValue.ToString(); }
            if (this.tubeInterface.Interlock.HasReadAccess()) { if (this.tubeInterface.Interlock.MonitorValue) this.circleInterlock.Fill = Brushes.Green; else this.circleInterlock.Fill = Brushes.Red; }
            if (this.tubeInterface.VacuumOk.HasReadAccess()) { if (this.tubeInterface.VacuumOk.MonitorValue) this.circleVacuum.Fill = Brushes.Green; else this.circleVacuum.Fill = Brushes.Red; }
            if (this.tubeInterface.CoolingOk.HasReadAccess()) { if (this.tubeInterface.CoolingOk.MonitorValue) this.circleCooling.Fill = Brushes.Green; else this.circleCooling.Fill = Brushes.Red; }
        }


        //NUD Voltage and Emission Control
        private void onNUDHighVoltageValueUpdated(object sender, EventArgs e)
        {
            if (this.tubeInterface.HighVoltage.HasReadWriteAccess()) { this.tubeInterface.HighVoltage.PcDemandValue = (float)this.nudHighVoltage.Value; }
        }
        private void onNUDEmissionCurrentValueUpdated(object sender, EventArgs e)
        {
            if (this.tubeInterface.EmissionCurrent.HasReadWriteAccess()) { this.tubeInterface.EmissionCurrent.PcDemandValue = (float)this.nudEmissionCurrent.Value; }
        }
        private void onNUDHighVoltageLostFoccus(object sender, RoutedEventArgs e)
        {
            mNUDHighVoltageFocus = false;
        }
        private void onNUDEmissionCurrentLostFoccus(object sender, RoutedEventArgs e)
        {
            mNUDEmissionCurrentFocus = false;
        }
        private void onNUDHighVoltageGotFoccus(object sender, RoutedEventArgs e)
        {
            if (!mNUDHighVoltageFocus) { this.nudHighVoltage.TextBoxCtrl.Text = this.txtHighVoltage.Text; }
            mNUDHighVoltageFocus = true;
        }
        private void onNUDEmissionCurrentGotFoccus(object sender, RoutedEventArgs e)
        {
            if (!mNUDEmissionCurrentFocus) { this.nudEmissionCurrent.TextBoxCtrl.Text = this.txtEmissionCurrent.Text; }
            mNUDEmissionCurrentFocus = true;
        }

        private void onXRayOnClicked(object sender, RoutedEventArgs e)
        {
            this.turnXRayOn();
        }

        private void onXRayOffClicked(object sender, RoutedEventArgs e)
        {
            this.turnXRayOff();
        }

        private void onBtnStartUpClicked(object sender, RoutedEventArgs e)
        {
            this.startUpXRay();
        }

        private void onRefreshClicked(object sender, RoutedEventArgs e)
        {
            this.refreshXRay();
        }

        private void onFilamentAdjustClicked(object sender, RoutedEventArgs e)
        {
            this.filamentAdjustXRay();
        }

        private void onCenterClicked(object sender, RoutedEventArgs e)
        {
            this.centerActiveXRay();
        }

        private void onTurbopumpOnClicked(object sender, RoutedEventArgs e)
        {
            this.startTurbopump();
        }

        private void onTurbopumpOffClicked(object sender, RoutedEventArgs e)
        {
            this.stopTurbopump();
        }
    }
}

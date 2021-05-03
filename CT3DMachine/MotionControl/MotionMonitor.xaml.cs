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
using NLog;
using CT3DMachine.Helper;
using CT3DMachine.Service;
using CT3DMachine.Connector;
using CT3DMachine.Codec;
using CT3DMachine.Model;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using CT3DMachine.XRayControl;
using CT3DMachine.MotionControl;
using CT3DMachine.TurntableControl;

namespace CT3DMachine.MotionControl
{
    /// <summary>
    /// Interaction logic for MotionMonitor.xaml
    /// </summary>
    /// 
    public delegate void EventRotCPositionChanged(double _pos);
    public partial class MotionMonitor : System.Windows.Controls.UserControl
    {
        private const double PULSES_PER_MILLIMETRE = 800;
        private const double PULSES_PER_DEGREE = 2000;
        private const double PULSES_PER_STEP = 1;
        private const double Dsd = 2000;
        private const double RADIUS_OF_TURNTABLE = 300; //mm

        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

        public bool StartedService = false;
        public bool ReadyMotion = false;
        public bool HomedMotion = false;
        public bool EnabledMotion = false;
        private bool mReadyFlag = false;

        private double mRotXPos = 0.0;

        private bool mIsDoneMoving = false;
        public bool isDoneMoving()
        {
            return mIsDoneMoving;
        }
        public double RotX
        {
            get { return mRotXPos; }
            set { mRotXPos = value; }
        }

        private double mDetYPos = 0.0;
        public double DetY
        {
            get { return mDetYPos; }
            set { mDetYPos = value; }
        }

        private double mRotCPos = 0.0;
        public double RotC
        {
            get { return mRotCPos; }
            set { mRotCPos = value; }
        }

        private double mDetZPos = 0.0;
        public double DetZ
        {
            get { return mDetZPos; }
            set { mDetZPos = value; }
        }

        private double mXRayPos = 0.0;
        public double XRayZ
        {
            get { return mXRayPos; }
            set { mXRayPos = value; }
        }


        private string mConnPort = "COM3";
        private int mBaudrate = 9600;
        private SerialPortInput mMechanicalConn = null;
        private MechanicalService mMechanicalService = null;
        private CodecAbstract mCodec = null;
        private DispatcherTimer mTimerCheckReady = null;

        public event EventRotCPositionChanged RotCPositionChanged;
        public void configurePort(string _port, int _baudrate)
        {
            mConnPort = _port;
            mBaudrate = _baudrate;
        }

        public bool startService()
        {
            if (this.StartedService) return false;
            
            mMechanicalConn = new SerialPortInput();                    
            mCodec = new MotionCodec();           
            mMechanicalConn.SetPort(mConnPort, mBaudrate);
            mMechanicalService = new MechanicalService(mMechanicalConn, mCodec);
            mMechanicalService.passingMessageEvent += new PassingMessageHandler(handleResponseMessage);
            if (!mMechanicalService.startService())
            {
                this.StartedService = false;
                return false;
            }            

            this.StartedService = true;
            this.ReadyMotion = false;
            this.mReadyFlag = true;
            this.enableMotion();
//            this.controlHomeMotion();
            if(this.mTimerCheckReady == null)
            {
                this.mTimerCheckReady = new DispatcherTimer();
            }
            this.mTimerCheckReady.Interval = TimeSpan.FromSeconds(1);  
            this.mTimerCheckReady.Tick += this.onTimerTick;
            this.mTimerCheckReady.Start();
            return true;
        }               

        public bool stopService()
        {
            if (!this.StartedService) return false;
            this.disableMotion();
            if (mMechanicalService == null) return false;           
            mMechanicalService.stopService();            
            this.StartedService = false;
            this.ReadyMotion = false;
            return true;
        }

        public MotionMonitor()
        {
            InitializeComponent();
        }

        public void enableAllControl()
        {
            this.btnRotXUp.IsEnabled = true;
            this.btnRotXDown.IsEnabled = true;
            this.btnRotXMove.IsEnabled = true;

            this.btnDetYUp.IsEnabled = true;
            this.btnDetYDown.IsEnabled = true;
            this.btnDetYMove.IsEnabled = true;

            this.btnRotCUp.IsEnabled = true;
            this.btnRotCDown.IsEnabled = true;
            this.btnRotCMove.IsEnabled = true;

            this.btnDetZUp.IsEnabled = true;
            this.btnDetZDown.IsEnabled = true;
            this.btnDetZMove.IsEnabled = true;

            this.btnXRayZUp.IsEnabled = true;
            this.btnXRayZDown.IsEnabled = true;
            this.btnXRayZMove.IsEnabled = true;

            this.btnHomming.IsEnabled = true;
        }

        public void disableAllControl()
        {
            this.btnRotXUp.IsEnabled = false;
            this.btnRotXDown.IsEnabled = false;
            this.btnRotXMove.IsEnabled = false;

            this.btnDetYUp.IsEnabled = false;
            this.btnDetYDown.IsEnabled = false;
            this.btnDetYMove.IsEnabled = false;

            this.btnRotCUp.IsEnabled = false;
            this.btnRotCDown.IsEnabled = false;
            this.btnRotCMove.IsEnabled = false;

            this.btnDetZUp.IsEnabled = false;
            this.btnDetZDown.IsEnabled = false;
            this.btnDetZMove.IsEnabled = false;

            this.btnXRayZUp.IsEnabled = false;
            this.btnXRayZDown.IsEnabled = false;
            this.btnXRayZMove.IsEnabled = false;

            this.btnHomming.IsEnabled = false;
        }

        public bool controlHomeMotion()
        {
            if (!StartedService) return false;
            //Send message
            MechanicalCommand msg = new MechanicalCommand(MessageType.MECHANICAL_HOME_MOTOR);
            this.mMechanicalService.processInnerMessage(msg);            
            return true;
        }

        public bool enableMotion()
        {
            if (!StartedService) return false;
            //Send message
            MechanicalCommand msg = new MechanicalCommand(MessageType.MECHANICAL_ENABLE_MOTOR);
            this.mMechanicalService.processInnerMessage(msg);
            this.EnabledMotion = true;
            return true;
        }

        public bool disableMotion()
        {
            if (!StartedService) return false;
            //Send message
            MechanicalCommand msg = new MechanicalCommand(MessageType.MECHANICAL_DISABLE_MOTOR);
            this.mMechanicalService.processInnerMessage(msg);
            this.EnabledMotion = false;
            return true;
        }

        public void moveToPositionByPulse(Int32 _rotX, Int32 _detY, Int32 _rotC, Int32 _detZ, Int32 _xrayZ)
        {
//            if (!ReadyMotion) return;
            MechanicalPosition msg = new MechanicalPosition(MessageType.MECHANICAL_MOVE_MOTOR);
            msg.setFirstEnginePosition(_rotX);
            msg.setSecondEnginePosition(_detY);
            msg.setThirdEnginePosition(_rotC);
            msg.setFourthEnginePosition(_detZ);
            msg.setFifthEnginePosition(_xrayZ);

            this.mIsDoneMoving = false;
            this.mMechanicalService.processInnerMessage(msg);
        }

        public void moveToPositionByValue(double _rotX, double _detY, double _rotC, double _detZ, double _xrayZ)
        {
//            if (!ReadyMotion) return;
            this.moveToPositionByPulse(
                (Int32)(_rotX * PULSES_PER_MILLIMETRE), 
                (Int32)(_detY * PULSES_PER_MILLIMETRE), 
                (Int32)(_rotC * PULSES_PER_DEGREE), 
                (Int32)(_detZ * PULSES_PER_MILLIMETRE), 
                (Int32)(_xrayZ * PULSES_PER_MILLIMETRE));            
        }

        public bool goToPosition(double _rotX, double _detY, double _rotC, double _detZ, double _xrayZ)
        {
            Logger.Info("Move to position: {0}, {1}, {2}, {3}, {4}", _rotX, _detY, _rotC, _detZ, _xrayZ);
            if (!this.ReadyMotion) return false;
            
            double deltaRotX = _rotX - this.mRotXPos;
            double deltaDetZ = _detZ - this.mDetZPos;
            //if ((_rotX < 98) && (_detZ > 0))
            //{
            //    deltaRotX = 0;
            //    deltaDetZ = 0;
            //    Logger.Warn("Require to move restrict region RotX = {0} and DetZ = {1}", _rotX, _detZ);
            //}
            double deltaDetY = _detY - this.mDetYPos;
            double deltaRotC = _rotC - this.mRotCPos;

            double deltaXRayPos = _xrayZ - this.mXRayPos;
            
            this.moveToPositionByValue(deltaRotX, deltaDetY, deltaRotC, deltaDetZ, deltaXRayPos);
            return true;
        }

        private void handleResponseMessage(BaseMessage msg)
        {
            switch (msg.getMessageType())
            {                
                case MessageType.MECHANICAL_REQUEST_INFO:
                    {
                        Logger.Info("\nRequest Info {0}", BitConverter.ToString(BitConverter.GetBytes((ushort)msg.getMessageType())));
                        break;
                    }
                case MessageType.MECHANICAL_MOVE_MOTOR:
                    {
                        Logger.Info("\nMove Motor = {0}", BitConverter.ToString(BitConverter.GetBytes((ushort)msg.getMessageType())));
                        break;
                    }
                case MessageType.MECHANICAL_POSITION_CHANGED:
                    {
                        this.mIsDoneMoving = true;
                        Logger.Info("\nPosition Changed {0}", BitConverter.ToString(BitConverter.GetBytes((ushort)msg.getMessageType())));
                        break;
                    }
                case MessageType.MECHANICAL_HOME_MOTOR:
                    {
                        Logger.Info("\nHome Motor = {0}", BitConverter.ToString(BitConverter.GetBytes((ushort)msg.getMessageType())));
                        break;
                    }
                case MessageType.MECHANICAL_MACHINE_READY:
                    {
                        Logger.Info("\nMachine Ready = {0}", BitConverter.ToString(BitConverter.GetBytes((ushort)msg.getMessageType())));
                        break;
                    }
                case MessageType.MECHANICAL_MACHINE_BUSY:
                    {
                        Logger.Info("\nMachine Busy = {0}", BitConverter.ToString(BitConverter.GetBytes((ushort)msg.getMessageType())));
                        break;
                    }
                case MessageType.MECHANICAL_MACHINE_ERROR:
                    {
                        Logger.Info("\nMachine Error = {0}", BitConverter.ToString(BitConverter.GetBytes((ushort)msg.getMessageType())));
                        break;
                    }
                case MessageType.MECHANICAL_ENABLE_MOTOR:
                    {
                        Logger.Info("\nEnable Motor = {0}", BitConverter.ToString(BitConverter.GetBytes((ushort)msg.getMessageType())));
                        break;
                    }
                case MessageType.MECHANICAL_DISABLE_MOTOR:
                    {
                        Logger.Info("\nDisable Motor = {0}", BitConverter.ToString(BitConverter.GetBytes((ushort)msg.getMessageType())));
                        break;
                    }
                case MessageType.MECHANICAL_RT_POSITION:
                    MechanicalPosition rtPosMessage = msg as MechanicalPosition;
                    if(this.mReadyFlag == false)
                    {
                        this.mReadyFlag = true;
                        Logger.Info("ReadMotion Change From {0} --> {1}", this.ReadyMotion, true);
                        this.ReadyMotion = true;
                        this.HomedMotion = true;
                        this.setDefaultMotorPos();
                    }
                    this.mReadyFlag = true;
                    this.Dispatcher.Invoke((MethodInvoker)delegate
                    {
                        if(this.mRotCPos != (double)rtPosMessage.getThirdEnginePosition() / PULSES_PER_DEGREE)
                        {
                            if (this.RotCPositionChanged != null) this.RotCPositionChanged((double)rtPosMessage.getThirdEnginePosition() / PULSES_PER_DEGREE);
                        }

                        this.mRotXPos = (double)rtPosMessage.getFirstEnginePosition() / PULSES_PER_MILLIMETRE;
                        this.mDetYPos = (double)rtPosMessage.getSecondEnginePosition() / PULSES_PER_MILLIMETRE;
                        this.mRotCPos = (double)rtPosMessage.getThirdEnginePosition() / PULSES_PER_DEGREE;
                        this.mDetZPos = (double)rtPosMessage.getFourthEnginePosition() / PULSES_PER_MILLIMETRE;
                        this.mXRayPos = (double)rtPosMessage.getFifthEnginePosition() / PULSES_PER_MILLIMETRE;

                        this.tbRotXPos.Text = this.mRotXPos.ToString("0.00");
                        this.tbDetYPos.Text = this.mDetYPos.ToString("0.00");
                        this.tbRotCPos.Text = this.mRotCPos.ToString("0.00");
                        this.tbDetZPos.Text = this.mDetZPos.ToString("0.00");
                        this.tbXRayZPos.Text = this.mXRayPos.ToString("0.00");
                        
                        //Logger.Info("Update Pos: {0}, {1}, {2}, {3}, {4} | {5}", this.mRotXPos, this.mDetYPos, this.mRotCPos, this.mDetZPos, this.mXRayPos, this.mReadyFlag);
                    });
                    break;
                case MessageType.MECHANICAL_UNKNOWN:                    
                    //Logger.Info("Unkown Message: {0}", BitConverter.ToString(BitConverter.GetBytes((ushort)msg.getMessageType())));
                    break;
                default:
                    {
                        //Logger.Info("\nMotionCodec key = {0} does NOT match any key", BitConverter.ToString(BitConverter.GetBytes((ushort)_key)));
                        break;
                    }
            }
        }

        //Button Event Listener
        private void btnRotXUp_Click(object sender, RoutedEventArgs e)
        {        
            double deltaRotXVal = Convert.ToDouble(this.tbRotXVal.Text);
            double dRotXVal = this.mRotXPos + deltaRotXVal;
            //if ((this.mDetZPos > 0) && (dRotXVal < 98)) { deltaRotXVal = 0; }
            Logger.Info("Do RotX Up: {0}, {1}, {2}, {3}, {4}, {5}", dRotXVal, this.mRotXPos, this.mDetYPos, this.mRotCPos, this.mDetZPos, this.mXRayPos);
            this.moveToPositionByValue(deltaRotXVal, 0, 0, 0, 0);
        }

        private void btnRotXDown_Click(object sender, RoutedEventArgs e)
        {
            double deltaRotXVal = Convert.ToDouble(this.tbRotXVal.Text);
            double dRotXVal = this.mRotXPos - deltaRotXVal;
            //if ((this.mDetZPos > 0) && (dRotXVal < 98)) { deltaRotXVal = 0; }
            Logger.Info("Do RotX Up: {0}, {1}, {2}, {3}, {4}, {5}", dRotXVal, this.mRotXPos, this.mDetYPos, this.mRotCPos, this.mDetZPos, this.mXRayPos);
            this.moveToPositionByValue(-deltaRotXVal, 0, 0, 0, 0);
        }

        private void btnDetYUp_Click(object sender, RoutedEventArgs e)
        {
            double deltaDetYVal = Convert.ToDouble(this.tbDetYVal.Text);
            double dDetYVal = this.mDetYPos + deltaDetYVal;
            Logger.Info("Do DetY Up: {0}, {1}, {2}, {3}, {4}, {5}", dDetYVal, this.mRotXPos, this.mDetYPos, this.mRotCPos, this.mDetZPos, this.mXRayPos);
            this.moveToPositionByValue(0, deltaDetYVal, 0, 0, 0);
        }

        private void btnDetYDown_Click(object sender, RoutedEventArgs e)
        {
            double deltaDetYVal = Convert.ToDouble(this.tbDetYVal.Text);
            double dDetYVal = this.mDetYPos - deltaDetYVal;
            Logger.Info("Do DetY Down: {0}, {1}, {2}, {3}, {4}, {5}", dDetYVal, this.mRotXPos, this.mDetYPos, this.mRotCPos, this.mDetZPos, this.mXRayPos);
            this.moveToPositionByValue(0, -deltaDetYVal, 0, 0, 0);
        }

        private void btnRotCUp_Click(object sender, RoutedEventArgs e)
        {
            double deltaRotCVal = Convert.ToDouble(this.tbRotCVal.Text);
            double dRotCVal = this.mRotCPos + deltaRotCVal;
            Logger.Info("Do RotC Up: {0}, {1}, {2}, {3}, {4}, {5}", dRotCVal, this.mRotXPos, this.mDetYPos, this.mRotCPos, this.mDetZPos, this.mXRayPos);
            this.moveToPositionByValue(0, 0, deltaRotCVal, 0, 0);
        }

        private void btnRotCDown_Click(object sender, RoutedEventArgs e)
        {
            double deltaRotCVal = Convert.ToDouble(this.tbRotCVal.Text);
            double dRotCVal = this.mRotCPos - deltaRotCVal;
            Logger.Info("Do RotC Down: {0}, {1}, {2}, {3}, {4}, {5}", dRotCVal, this.mRotXPos, this.mDetYPos, this.mRotCPos, this.mDetZPos, this.mXRayPos);
            this.moveToPositionByValue(0, 0, -deltaRotCVal, 0, 0);
        }

        private void btnDetZUp_Click(object sender, RoutedEventArgs e)
        {
            double deltaDetZVal = Convert.ToDouble(this.tbDetZVal.Text);
            double dDetZVal = this.mDetZPos + deltaDetZVal;
            //if ((dDetZVal > 0) && (this.mRotXPos < 98)) { deltaDetZVal = 0; }
            Logger.Info("Do DetZ Up: {0}, {1}, {2}, {3}, {4}, {5}", dDetZVal, this.mRotXPos, this.mDetYPos, this.mRotCPos, this.mDetZPos, this.mXRayPos);
            this.moveToPositionByValue(0, 0, 0, deltaDetZVal, 0);
        }

        private void btnDetZDown_Click(object sender, RoutedEventArgs e)
        {
            double deltaDetZVal = Convert.ToDouble(this.tbDetZVal.Text);
            double dDetZVal = this.mDetZPos - deltaDetZVal;
            //if ((dDetZVal > 0) && (this.mRotXPos < 98)) { deltaDetZVal = 0; }
            Logger.Info("Do DetZ Down: {0}, {1}, {2}, {3}, {4}, {5}", dDetZVal, this.mRotXPos, this.mDetYPos, this.mRotCPos, this.mDetZPos, this.mXRayPos);
            this.moveToPositionByValue(0, 0, 0, -deltaDetZVal, 0);
        }

        private void btnXRayZUp_Click(object sender, RoutedEventArgs e)
        {
            double deltaXRayZVal = Convert.ToDouble(this.tbXRayZVal.Text);
            double dXRayVal = this.mXRayPos + deltaXRayZVal;
            Logger.Info("Do RotC Down: {0}, {1}, {2}, {3}, {4}, {5}", dXRayVal, this.mRotXPos, this.mDetYPos, this.mRotCPos, this.mDetZPos, this.mXRayPos);
            this.moveToPositionByValue(0, 0, 0, 0, deltaXRayZVal);
        }

        private void btnXRayZDown_Click(object sender, RoutedEventArgs e)
        {
            double deltaXRayZVal = Convert.ToDouble(this.tbXRayZVal.Text);
            double dXRayVal = this.mXRayPos - deltaXRayZVal;
            Logger.Info("Do RotC Down: {0}, {1}, {2}, {3}, {4}, {5}", dXRayVal, this.mRotXPos, this.mDetYPos, this.mRotCPos, this.mDetZPos, this.mXRayPos);
            this.moveToPositionByValue(0, 0, 0, 0, -deltaXRayZVal);
        }
        //--------------------- Move
        private void btnRotXMove_Click(object sender, RoutedEventArgs e)
        {            
            double dRotXVal = Convert.ToDouble(this.tbRotXVal.Text);
            double deltaRotX = dRotXVal - this.mRotXPos;
            //if ((this.mDetZPos > 0) && (dRotXVal < 98)) { deltaRotX = 0; }
            Logger.Info("Do RotX Down: {0}, {1}, {2}, {3}, {4}, {5}", dRotXVal, this.mRotXPos, this.mDetYPos, this.mRotCPos, this.mDetZPos, this.mXRayPos);
            this.moveToPositionByValue(deltaRotX, 0, 0, 0, 0);
        }

        private void btnDetYMove_Click(object sender, RoutedEventArgs e)
        {            
            double dDetYVal = Convert.ToDouble(this.tbDetYVal.Text);
            double deltaDetY = dDetYVal - this.mDetYPos;
            this.moveToPositionByValue(0, deltaDetY, 0, 0, 0);
        }

        private void btnRotCMove_Click(object sender, RoutedEventArgs e)
        {
            double dRotCPos = Convert.ToDouble(this.tbRotCVal.Text);
            double deltaRotC = dRotCPos - this.mRotCPos;
            this.moveToPositionByValue(0, 0, deltaRotC, 0, 0);
        }

        private void btnDetZMove_Click(object sender, RoutedEventArgs e)
        {
            double dDetZVal = Convert.ToDouble(this.tbDetZVal.Text);
            double deltaDetZ = dDetZVal - this.mDetZPos;
            //if ((dDetZVal > 0) && (this.mRotXPos < 98)) { deltaDetZ = 0; }
            this.moveToPositionByValue(0, 0, 0, deltaDetZ, 0);
        }

        private void btnXRayZMove_Click(object sender, RoutedEventArgs e)
        {
            double dXRayPos = Convert.ToDouble(this.tbXRayZVal.Text);
            double deltaXRayPos = dXRayPos - this.mXRayPos;
            this.moveToPositionByValue(0, 0, 0, 0, deltaXRayPos);
        }

        private void buttonHome_Click(object sender, RoutedEventArgs e)
        {
            this.controlHomeMotion();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            System.Windows.Controls.TextBox textBox = (System.Windows.Controls.TextBox)sender;
            string regexStr = @"^[0-9]{0,5}\.*?$";
            string previewStr = textBox.Text;
            if (previewStr.Contains(".")) { regexStr = @"^[0-9]{0,5}\.*?[0-9]{1,2}$"; }
            previewStr += e.Text;
            Regex regex = new Regex(regexStr);
            e.Handled = !regex.IsMatch(previewStr);
        }

        private void onTimerTick(object sender, EventArgs e)
        {
            this.mReadyFlag = false;
            this.mTimerCheckReady.Stop();
        }

        private void setDefaultMotorPos()
        {
            this.mRotCPos = 0.0;
            this.mRotXPos = 0.0;
            this.mDetYPos = 0.0;
            this.mDetZPos = 0.0;
            this.mXRayPos = 0.0;
        }
    }
}

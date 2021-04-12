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
using System.Text.RegularExpressions;
using CT3DMachine.Service;
using CT3DMachine.Connector;
using CT3DMachine.Codec;
using CT3DMachine.Model;
using CT3DMachine.Helper;
using System.Windows.Threading;
using System.Windows.Forms;
using NLog;
namespace CT3DMachine.DetectorControl
{
    /// <summary>
    /// Interaction logic for DetectorMonitor.xaml
    /// </summary>
    public partial class DetectorMonitor : System.Windows.Controls.UserControl
    {
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

        public bool StartedService = false;
        public bool ReadyMotion = false;

        private string mSendIP = "127.0.0.1";
        private int mSendPort = 1234;
        private string mReceiveIP = "127.0.0.1";
        private int mReceivePort = 4321;
        private UDPConnector mConn = null;
        private CodecAbstract mCodec = null;
        private DetectorService mDetectorService = null;

        private DetectorOperationMode mOperationMode = DetectorOperationMode.NORMAL;
        private DetectorState mState = DetectorState.DISCONNECTED;
        private int mFPS = 0;
        private int mNOF = 0;
        private float mBMPRate = 0;

        private UInt16 mDefaultValue = 0xFFFF;
        private UInt16 mCurGetImage = 0xFFFF;
        private UInt16 mCurResImage = 0xFFFF;

        private String mSavePath = "imgs";

        public UInt16 CurGetImage
        {
            get { return mCurGetImage; }
            set { mCurGetImage = value; }
        }

        public UInt16 CurResImage
        {
            get { return mCurResImage; }
            set { mCurResImage = value; }
        }

        public DetectorState State
        {
            get { return mState; }
            set { mState = value; }
        }

        public DetectorMonitor()
        {
            InitializeComponent();
        }

        public void configureConn(int _sendPort, string _sendIP, int _receivePort, string _receiveIP)
        {
            this.mSendPort = _sendPort;
            this.mSendIP = _sendIP;
            this.mReceivePort = _receivePort;
            this.mReceiveIP = _receiveIP;
        }

        public bool startService()
        {
            if (this.StartedService) return false;

            mConn = new UDPConnector();
            mCodec = new KLVCodec();
            mConn.Configure(mSendPort, mSendIP, mReceivePort, mReceiveIP);
            mDetectorService = new DetectorService(mConn, mCodec);

            mDetectorService.passingMessageEvent += new PassingMessageHandler(handleResponseMessage);
            if (!mDetectorService.startService())
            {
                this.StartedService = false;
                return false;
            }

            this.StartedService = true;
            return true;
        }

        public bool stopService()
        {
            if (!this.StartedService) return false;

            mDetectorService.stopService();
            this.StartedService = false;
            return true;
        }                

        private void handleResponseMessage(BaseMessage msg)
        {
            switch (msg.getMessageType())
            {
                case MessageType.DETECTOR_STATUS_MESSAGE:
                    {
                        DetectorStatus detectorStatus = msg as DetectorStatus;
                        this.mState = detectorStatus.getState();
                        this.mOperationMode = detectorStatus.getOperationMode();
                        this.mFPS = detectorStatus.getFPS();
                        this.mNOF = detectorStatus.getNOF();
                        this.mBMPRate = detectorStatus.getBMPRate();

                        this.Dispatcher.Invoke((MethodInvoker)delegate
                        {
                            switch (this.mState)
                            {
                                case DetectorState.DISCONNECTED:
                                    {
                                        lbDetectorState.Content = "Chưa kết nối";
                                        lbDetectorBMPRate.Content = "";
                                        lbDetectorFPS.Content = "";
                                        lbDetectorNumOfFrame.Content = "";
                                        break;
                                    }
                                case DetectorState.CONNECTED:
                                    {
                                        lbDetectorState.Content = "Đã kết nối";
                                        circleDetectorState.Fill = Brushes.Green;
                                        //lbDetectorBMPRate.Content = "BMPRate ( " + this.mBMPRate.ToString() + " % )";
                                        lbDetectorFPS.Content = "FPS (" + this.mFPS.ToString() + ")";
                                        lbDetectorNumOfFrame.Content = "Số ảnh chụp(" + this.mNOF.ToString() + ")";
                                        break;
                                    }
                                case DetectorState.CALIBARATING:
                                    {
                                        lbDetectorState.Content = "Đang hiệu chỉnh";
                                        circleDetectorState.Fill = Brushes.Green;
                                        //lbDetectorBMPRate.Content = "BMPRate ( " + this.mBMPRate.ToString() + " % )";
                                        lbDetectorFPS.Content = "FPS (" + this.mFPS.ToString() + ")";
                                        lbDetectorNumOfFrame.Content = "Số ảnh chụp(" + this.mNOF.ToString() + ")";
                                        break;
                                    }
                                case DetectorState.CALIBRATE_DARK_DONE:
                                    {
                                        lbDetectorState.Content = "Hiệu chỉnh tối xong";
                                        circleDetectorState.Fill = Brushes.Green;
                                        //lbDetectorBMPRate.Content = "BMPRate ( " + this.mBMPRate.ToString() + " % )";
                                        lbDetectorFPS.Content = "FPS ( " + this.mFPS.ToString() + " )";
                                        lbDetectorNumOfFrame.Content = "Số ảnh chụp(" + this.mNOF.ToString() + ")";
                                        break;
                                    }
                                case DetectorState.CALIBRATE_BRIGHT_DONE:
                                    {
                                        lbDetectorState.Content = "Hiệu chỉnh sáng xong";
                                        circleDetectorState.Fill = Brushes.Green;
                                        lbDetectorBMPRate.Content = "BMPRate ( " + this.mBMPRate.ToString() + " % )";
                                        lbDetectorFPS.Content = "FPS ( " + this.mFPS.ToString() + " )";
                                        lbDetectorNumOfFrame.Content = "Số ảnh chụp(" + this.mNOF.ToString() + ")";
                                        break;
                                    }
                                case DetectorState.READY_GET_IMAGE:
                                    {
                                        lbDetectorState.Content = "Sẵn sàng chụp ảnh";
                                        circleDetectorState.Fill = Brushes.Green;
                                        lbDetectorBMPRate.Content = "BMPRate (" + this.mBMPRate.ToString() + " %)";
                                        lbDetectorFPS.Content = "FPS ( " + this.mFPS.ToString() + " )";
                                        lbDetectorNumOfFrame.Content = "Số ảnh chụp(" + this.mNOF.ToString() + ")";
                                        break;
                                    }
                            }
                        });

                        //Logger.Info("DETECTOR_STATUS_MESSAGE: state = {0}, mode = {1}, FPS = {2}, NOF = {3}, BMP = {4}", 
                        //    detectorStatus.getState(), detectorStatus.getOperationMode(), detectorStatus.getFPS(), detectorStatus.getNOF(), detectorStatus.getBMPRate());
                        break;
                    }
                case MessageType.DETECTOR_GET_IMAGE_DONE:
                    {
                        DetectorGetImageDone res = msg as DetectorGetImageDone;
                        this.mCurResImage = res.getIndex();
                        Logger.Info("DETECTOR_GET_IMAGE_DONE: index = {0}", res.getIndex());
                        break;
                    }
                case MessageType.DETECTOR_ERROR_MESSAGE:
                    {
                        DetectorError res = msg as DetectorError;
                        Logger.Info("DETECTOR_ERROR_MESSAGE: error code = {0}", res.getErrorCode());
                        break;
                    }
                default:
                    {
                        //Logger.Info("DetectorCodec key = {0} does NOT match any key", BitConverter.ToString(BitConverter.GetBytes((ushort)_key)));
                        break;
                    }
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            System.Windows.Controls.TextBox textBox = (System.Windows.Controls.TextBox)sender;
            string regexStr = @"^[0-9]{0,5}?$";
            string previewStr = textBox.Text;
            previewStr += e.Text;
            Regex regex = new Regex(regexStr);
            e.Handled = !regex.IsMatch(previewStr);
        }
        
        private void btnConfigure_Clicked(object sender, RoutedEventArgs e)
        {
            if (!StartedService) return;
            DetectorOperationMode mode = (this.rbNormal.IsChecked == true) ? DetectorOperationMode.NORMAL : DetectorOperationMode.BINDING;
            byte fps = Convert.ToByte(this.tbFPS.Text);
            byte nof = Convert.ToByte(this.tbNumOfFrame.Text);
            DetectorConfiguration msg = new DetectorConfiguration(mode, fps, nof);
            this.mDetectorService.processInnerMessage(msg);
        }

        private void onRBNormalChecked(object sender, RoutedEventArgs e)
        {
            //if (!StartedService) return;
            //DetectorOperationMode mode = DetectorOperationMode.NORMAL;
            //byte fps = (byte)this.mFPS;
            //byte nof = (byte)this.mNOF;
            //DetectorConfiguration msg = new DetectorConfiguration(mode, fps, nof);
            //this.mDetectorService.processInnerMessage(msg);
        }

        private void onRBBindingChecked(object sender, RoutedEventArgs e)
        {
            //if (!StartedService) return;
            //DetectorOperationMode mode = DetectorOperationMode.BINDING;
            //byte fps = (byte)this.mFPS;
            //byte nof = (byte)this.mNOF;
            //DetectorConfiguration msg = new DetectorConfiguration(mode, fps, nof);
            //this.mDetectorService.processInnerMessage(msg);
        }

        public bool getImage(UInt16 _index)
        {
            //if (this.mState != DetectorState.READY_GET_IMAGE) return false;
            this.mCurGetImage = _index;
            this.mCurResImage = this.mDefaultValue;
            DetectorGetImage msg = new DetectorGetImage(_index, this.mSavePath);
            this.mDetectorService.processInnerMessage(msg);
            return true;
        }

        public bool stopDetector()
        {
            if (!StartedService) return false;
            DetectorStop msg = new DetectorStop();
            this.mDetectorService.processInnerMessage(msg);
            return true;
        }

        private void btnGetImage_Clicked(object sender, RoutedEventArgs e)
        {
            this.getImage(0);
        }

        private void btnCalibrateDark_Clicked(object sender, RoutedEventArgs e)
        {
            if (!StartedService) return;            
            DetectorCalibDark msg = new DetectorCalibDark();
            this.mDetectorService.processInnerMessage(msg);
        }

        private void btnCalibrateBright_Clicked(object sender, RoutedEventArgs e)
        {
            if (!StartedService) return;
            DetectorCalibBright msg = new DetectorCalibBright();
            this.mDetectorService.processInnerMessage(msg);
        }
    }
}

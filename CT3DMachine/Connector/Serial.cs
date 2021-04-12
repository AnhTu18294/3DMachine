using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;
using NLog;
using CT3DMachine.Model;

namespace CT3DMachine.Connector
{
    public enum DataBits
    {
        Five = 5,
        Six,
        Seven,
        Eight
    }

    class SerialPortInput: ConnectorAbstract
    {
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();
        #region Private Fields
        private SerialPort mSerialPort;
        private string mPortName = "";
        private int mBaudRate = 115200;
        private StopBits mStopBits = StopBits.One;
        private Parity mParity = Parity.None;
        private DataBits mDataBits = DataBits.Eight;

        // Read/Write state variable
        private bool gotReadWriteError = true;

        // Serial port reader task
        private Thread mReader;

        // Serial port connection watcher
        private Thread mConnectionWatcher;

        private object mAccessLock = new object();
        private bool mDisconnectRequested = false;

        #endregion       

        #region Public Members
        public override bool Connect()
        {
            if(mDisconnectRequested)
            {
                return false;
            }
            lock(mAccessLock)
            {
                Disconnect();
                Open();
                mConnectionWatcher = new Thread(ConnectionWatcherTask);
                mConnectionWatcher.Start();
            }
            return IsConnected;
        }

        public override void Disconnect()
        {
            if (mDisconnectRequested)
                return;
            mDisconnectRequested = true;
            Close();
            lock(mAccessLock)
            {
                if(mConnectionWatcher != null)
                {
                    if(!mConnectionWatcher.Join(5000))
                    {
                        mConnectionWatcher.Abort();
                    }
                    mConnectionWatcher = null;
                }
                mDisconnectRequested = false;
            }
        }

        public bool IsConnected
        {
            get { return mSerialPort != null && !gotReadWriteError && !mDisconnectRequested;}
        }

        public void SetPort(string portName, int baudRate = 115200, StopBits stopBits = StopBits.One,
            Parity parity = Parity.None, DataBits dataBits = DataBits.Eight)
        {
            if(mPortName != portName)
            {
                gotReadWriteError = true;
            }

            mPortName = portName;
            mBaudRate = baudRate;
            mStopBits = stopBits;
            mParity = parity;
            mDataBits = dataBits;
        }

        public override bool SendMessage(byte[] message)
        {
            bool success = false;
            if(IsConnected)
            {
                try
                {
                    mSerialPort.Write(message, 0, message.Length);
                    success = true;
                } catch (Exception e)
                {
                    Logger.Info(e);
                }
            }
            return success;
        }
        #endregion

        #region Private Members

        #region Serial Port handling
        private bool Open()
        {
            bool success = false;
            lock(mAccessLock)
            {
                Close();
                try
                {
                    bool tryOpen = true;
                    if(Environment.OSVersion.Platform.ToString().StartsWith("Win") == false)
                    {
                        tryOpen = (tryOpen && System.IO.File.Exists(mPortName));
                    }

                    if(tryOpen)
                    {
                        mSerialPort = new SerialPort();
                        mSerialPort.ErrorReceived += HandleErrorReceived;
                        mSerialPort.PortName = mPortName;
                        mSerialPort.BaudRate = mBaudRate;
                        mSerialPort.StopBits = mStopBits;
                        mSerialPort.Parity = mParity;
                        mSerialPort.DataBits = (int)mDataBits;

                        mSerialPort.Open();
                        success = true;
                    }
                } catch(Exception e)
                {
                    Logger.Error(e.Message);
                    Close();
                }

                if(mSerialPort != null && mSerialPort.IsOpen)
                {
                    gotReadWriteError = false;
                    mReader = new Thread(ReaderTask);
                    mReader.Start();
                    onConnectionStatusChanged(new ConnectionStatusChangedEventArgs(true));
                }
            }
            return success;
        }

        private void Close()
        {
            lock(mAccessLock)
            {
                if(mReader != null)
                {
                    if(!mReader.Join(5000))
                    {
                        mReader.Abort();
                    }
                    mReader = null;
                }

                if(mSerialPort != null)
                {
                    mSerialPort.ErrorReceived -= HandleErrorReceived;
                    if(mSerialPort.IsOpen)
                    {
                        mSerialPort.Close();
                        onConnectionStatusChanged(new ConnectionStatusChangedEventArgs(false));
                    }
                    mSerialPort.Dispose();
                    mSerialPort = null;
                }
                gotReadWriteError = true;
            }
        }

        private void HandleErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            Logger.Error(e);
        }

        #endregion

        #region Background Tasks
        private void ReaderTask()
        {
            while(IsConnected)
            {
                int msglen = 0;
                try
                {
                    msglen = mSerialPort.BytesToRead;
                    if(msglen > 0)
                    {
                        byte[] message = new byte[msglen];

                        int readbytes = 0;
                        while (mSerialPort.Read(message, readbytes, msglen - readbytes) <= 0)
                            ;
                        onMessageReceived(new MessageReceivedEventArgs(message));
                    } else
                    {
                        Thread.Sleep(100);
                    }
                } catch(Exception e)
                {
                    Logger.Error(e.Message);
                    gotReadWriteError = true;
                    Thread.Sleep(1000);
                }
            }
        }

        private void ConnectionWatcherTask()
        {
            while(!mDisconnectRequested)
            {
                if(gotReadWriteError)
                {
                    try
                    {
                        Close();
                        Thread.Sleep(1000);
                        if (!mDisconnectRequested)
                        {
                            try
                            {
                                Open();
                            }
                            catch (Exception e)
                            {
                                Logger.Error(e.Message);
                            }
                        }             
                    } catch (Exception e)
                    {
                        Logger.Error(e.Message);
                    }
                }
                if(!mDisconnectRequested)
                {
                    Thread.Sleep(1000);
                }
            }
        }
        #endregion

        #region Events Raising
        protected override void onConnectionStatusChanged(ConnectionStatusChangedEventArgs args)
        {
            //
            base.onConnectionStatusChanged(args);
        }

        protected override void onMessageReceived(MessageReceivedEventArgs args)
        {
            //
            base.onMessageReceived(args);         
        }
        #endregion

        #endregion

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using CT3DMachine.Model;
using NLog;

namespace CT3DMachine.Connector
{
    class UDPConnector : ConnectorAbstract
    {
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();
        // Serial port reader task
        private string mReceiveIP = "127.0.0.1";
        private int mReceivePort = -1;
        private IPEndPoint mReceiveIPEP = null;
        private Socket mRevSock = null;
        private Thread mReceiver = null;
        private bool mIsRevConnected = false;
        private IPEndPoint mSenderOfRev = new IPEndPoint(IPAddress.Any, 0);

        // Send 
        private string mSendIP = "127.0.0.1";
        private int mSendPort = -1;
        private IPEndPoint mSendIPEP = null;
        private UdpClient mSender = null;
        private bool mIsSendConnected = false;
        
        private object mAccessLock = new object();

        public UDPConnector()
        {

        }

        public UDPConnector(int _sendPort, string _sendIP, int _receivePort, string _receiveIP)
        {
            Configure(_sendPort, _sendIP, _receivePort, _receiveIP);
        }

        public void Configure(int _sendPort, string _sendIP, int _receivePort, string _receiveIP)
        {
            mSendPort = _sendPort;
            mSendIP = _sendIP;
            mReceivePort = _receivePort;
            mReceiveIP = _receiveIP;
        }

        public override bool Connect()
        {
            if (mIsSendConnected || mIsRevConnected)
            {
                Disconnect();
            }

            try
            {
                mSendIPEP = new IPEndPoint(IPAddress.Parse(mSendIP), mSendPort);
                mSender = new UdpClient();
                mSender.Connect(mSendIPEP);
                this.mIsSendConnected = true;
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                this.mIsSendConnected = false;
                Disconnect();
                return false;
            }

            try
            {
                mReceiveIPEP = new IPEndPoint(IPAddress.Parse(mReceiveIP), mReceivePort);
                mRevSock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                mRevSock.Bind(mReceiveIPEP);
                mReceiver = new Thread(ReceiverTask);
                mReceiver.Start();
                this.mIsRevConnected = true;
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                this.mIsRevConnected = false;
                Disconnect();
                return false;
            }

            onConnectionStatusChanged(new ConnectionStatusChangedEventArgs(true));
            return true;
        }

        public override void Disconnect()
        {
            if (mIsRevConnected)
            {
                try
                {
                    mReceiver.Abort();
                    mRevSock.Close();
                    mReceiver = null;
                    mRevSock = null;
                    mReceiveIPEP = null;
                    mIsRevConnected = false;
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message);
                    mIsRevConnected = true;
                }
            }

            if (mIsSendConnected)
            {
                try
                {
                    mSender.Close();
                    mSendIPEP = null;
                    mSender = null;
                    mIsSendConnected = false;
                }
                catch (Exception e)
                {
                    Console.Write(e.Message);
                    mIsSendConnected = true;
                }
            }
            onConnectionStatusChanged(new ConnectionStatusChangedEventArgs(false));
        }

        public override bool SendMessage(byte[] message)
        {
            if (!mIsSendConnected) return false;
            try
            {
                mSender.Send(message, message.Length);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return false;
            }
            return true;
        }

        private void ReceiverTask()
        {
            byte[] buffRecv = new byte[1024]; ;
            int recv;
            EndPoint Remote = (EndPoint)mSenderOfRev;

            while (mIsRevConnected)
            {
                try
                {
                    recv = mRevSock.ReceiveFrom(buffRecv, ref Remote);
                    byte[] revData = new byte[recv];
                    Array.Copy(buffRecv, 0, revData, 0, recv);
                    onMessageReceived(new MessageReceivedEventArgs(revData));
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message);
                }
            }
        }

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
    }
}

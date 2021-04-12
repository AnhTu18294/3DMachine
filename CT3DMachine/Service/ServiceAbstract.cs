using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using NLog;
using CT3DMachine.Model;
using CT3DMachine.Connector;
using CT3DMachine.Codec;

namespace CT3DMachine.Service
{
    public delegate void PassingMessageHandler(BaseMessage msg);

    abstract class ServiceAbstract
    {
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();
        private ConnectorAbstract mConnector;
        private CodecAbstract mCodec;

        
        public event PassingMessageHandler passingMessageEvent;

        public ServiceAbstract(ConnectorAbstract con, CodecAbstract codec)
        {
            mConnector = con;
            mCodec = codec;

            mConnector.ConnectionStatusChanged += delegate (object sender, ConnectionStatusChangedEventArgs arg)
            {
                Logger.Info("Connected = {0}", arg.Connected);
            };

            mConnector.MessageReceived += delegate (object sender, MessageReceivedEventArgs arg)
            {        
                List<BaseMessage> listMsg = mCodec.decode(arg.Data);
                foreach(BaseMessage msg in listMsg) {
                    if (msg != null)
                    {
                        processIoMessage(msg);
                    }
                }
            };
        }

        public virtual bool startService()
        {
            if(mConnector.Connect())
            {
                Logger.Info("Connect successfully!");
                return true;
            }else
            {
                return false;
            }
        }

        public virtual void stopService()
        {
            mConnector.Disconnect();
        }

        public abstract void processIoMessage(BaseMessage msg);

        public abstract void processInnerMessage(BaseMessage msg);

        public void send(BaseMessage msg)
        {
            byte[] outData = mCodec.encode(msg);
            
            if (outData != null)
            {
                if (mConnector.SendMessage(outData))
                {
                    Logger.Info("Send data success: {0}", BitConverter.ToString(outData));
                }
                else
                {
                    Logger.Error("Could not send data!");
                }
            }
        }

        public void passingMessage(BaseMessage msg)
        {
            if(passingMessageEvent != null)
            {
                passingMessageEvent(msg);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CT3DMachine.Model;

namespace CT3DMachine.Connector
{
    abstract class ConnectorAbstract
    {
        #region Public Events
        public delegate void ConnectionStatusChangedEventHandler(object sender, ConnectionStatusChangedEventArgs args);

        public event ConnectionStatusChangedEventHandler ConnectionStatusChanged;

        public delegate void MessageReceivedEventHandler(object sender, MessageReceivedEventArgs args);

        public event MessageReceivedEventHandler MessageReceived;

        #endregion

        public abstract bool Connect();

        public abstract void Disconnect();

        public abstract bool SendMessage(byte[] message);

        protected virtual void onConnectionStatusChanged(ConnectionStatusChangedEventArgs args)
        {
            Console.WriteLine(args.Connected);
            if (ConnectionStatusChanged != null)
            {
                ConnectionStatusChanged(this, args);
            }
        }

        protected virtual void onMessageReceived(MessageReceivedEventArgs args)
        {
            if(MessageReceived != null)
            {
                MessageReceived(this, args);
            }
        }
    }
}

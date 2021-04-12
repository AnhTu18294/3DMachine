using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CT3DMachine.Model
{
    class ConnectionStatusChangedEventArgs
    {
        public readonly bool Connected;

        public ConnectionStatusChangedEventArgs(bool state)
        {
            Connected = state;
        }
    }

    public class MessageReceivedEventArgs
    {
        public readonly byte[] Data;

        public MessageReceivedEventArgs(byte[] data)
        {
            Data = data;
        }
    }
}

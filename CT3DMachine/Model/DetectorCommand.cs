using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CT3DMachine.Helper;
using NLog;

namespace CT3DMachine.Model
{
    class DetectorCommand : BaseMessage
    {
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

        private byte[] mCommand = new byte[100];

        public DetectorCommand(string _cmd, MessageType messageType = MessageType.DETECTOR_COMMAND) : base(MessageType.DETECTOR_COMMAND)
        {
            byte[] bCommandData = Encoding.ASCII.GetBytes(_cmd);
            Buffer.BlockCopy(bCommandData, 0, mCommand, 0, bCommandData.Length);
        }

        public override byte[] serialize()
        {
            ByteBuffer buf = new ByteBuffer();

            buf.putBytes(mCommand, 100);
            //
            int length = buf.getTotalLength();

            byte[] payload = new byte[length];
            buf.getBytes(ref payload, length);

            return payload;
        }

        public override void deserialize(byte[] array)
        {
            ByteBuffer buf = new ByteBuffer();
            buf.putBytes(array, array.Length);

            buf.getBytes(ref mCommand, 100);
        }

        public override UInt16 length()
        {
            return 100;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CT3DMachine.Helper;
using NLog;

namespace CT3DMachine.Model
{
    class DetectorError : BaseMessage
    {
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

        private byte mCode;

        public DetectorError(MessageType messageType = MessageType.DETECTOR_STOP_MESSAGE) : base(MessageType.DETECTOR_STOP_MESSAGE)
        {
            mCode = 0x00;
        }

        public DetectorError(byte _code, MessageType messageType = MessageType.DETECTOR_STOP_MESSAGE) : base(MessageType.DETECTOR_STOP_MESSAGE)
        {
            mCode = _code;
        }

        public byte getErrorCode() { return mCode; }

        public override byte[] serialize()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.putByte(mCode);
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

            buf.getByte(ref mCode);
        }

        public override UInt16 length()
        {
            return 1;
        }
    }
}

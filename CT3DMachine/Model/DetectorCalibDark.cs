using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CT3DMachine.Helper;
using NLog;

namespace CT3DMachine.Model
{
    class DetectorCalibDark : BaseMessage
    {
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

        private byte mData;
        
        public DetectorCalibDark(MessageType messageType = MessageType.DETECTOR_CALIB_DARK_MESSAGE) : base(MessageType.DETECTOR_CALIB_DARK_MESSAGE)
        {
            mData = 0x00;
        }

        public override byte[] serialize()
        {
            ByteBuffer buf = new ByteBuffer();            
            buf.putByte(mData);
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
            
            buf.getByte(ref mData);
        }

        public override UInt16 length()
        {
            return 1;
        }
    }
}

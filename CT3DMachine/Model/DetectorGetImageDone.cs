using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CT3DMachine.Helper;
using NLog;

namespace CT3DMachine.Model
{
    class DetectorGetImageDone : BaseMessage
    {
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

        private UInt16 mIndex;

        public DetectorGetImageDone(MessageType messageType = MessageType.DETECTOR_GET_IMAGE_DONE) : base(MessageType.DETECTOR_GET_IMAGE_DONE)
        {
            mIndex = 0;
        }

        public DetectorGetImageDone(UInt16 _index, MessageType messageType = MessageType.DETECTOR_GET_IMAGE_DONE) : base(MessageType.DETECTOR_GET_IMAGE_DONE)
        {
            mIndex = _index;
        }

        public UInt16 getIndex() { return mIndex; }

        public override byte[] serialize()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.putUInt16(mIndex);
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

            buf.getUInt16(ref mIndex);
        }

        public override UInt16 length()
        {
            return 2;
        }
    }
}

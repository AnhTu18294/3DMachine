using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CT3DMachine.Helper;
using NLog;

namespace CT3DMachine.Model
{
    class DetectorGetImage : BaseMessage
    {
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();
        private UInt16 mIndex;
        private byte[] mPath = new byte[100];

        public DetectorGetImage(UInt16 _index, String _path, MessageType messageType = MessageType.DETECTOR_GET_IMAGE) : base(MessageType.DETECTOR_GET_IMAGE)
        {
            mIndex = _index;
            byte[] bPathData = Encoding.ASCII.GetBytes(_path);
            Buffer.BlockCopy(bPathData, 0, mPath, 0, bPathData.Length);
        }

        public UInt16 getIndex() { return mIndex; }

        public override byte[] serialize()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.putUInt16(mIndex);
            buf.putBytes(mPath, 100);
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
            buf.getBytes(ref mPath, 100);
        }

        public override UInt16 length()
        {
            return 102;
        }
    }
}

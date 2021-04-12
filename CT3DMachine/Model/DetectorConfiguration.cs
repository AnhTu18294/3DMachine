using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CT3DMachine.Helper;
using NLog;

namespace CT3DMachine.Model
{    
    class DetectorConfiguration : BaseMessage
    {
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();
        
        private DetectorOperationMode mOperationMode;
        private byte mFPS;
        private byte mNOF; // number of frame

        public DetectorConfiguration(MessageType messageType = MessageType.DETECTOR_CONFIGURATION_MESSAGE) : base(MessageType.DETECTOR_CONFIGURATION_MESSAGE)
        {
            mOperationMode = DetectorOperationMode.BINDING;
            mFPS = 0;
            mNOF = 0;
        }

        public DetectorConfiguration(DetectorOperationMode _mode, byte _fps, byte _nof, MessageType messageType = MessageType.DETECTOR_CONFIGURATION_MESSAGE) : base(MessageType.DETECTOR_CONFIGURATION_MESSAGE)
        {
            mOperationMode = _mode;
            mFPS = _fps;
            mNOF = _nof;
        }

        public DetectorOperationMode getOperationMode() { return mOperationMode; }
        public byte getFPS() { return mFPS; }
        public byte getNOF() { return mNOF; }

        public override byte[] serialize()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.putByte((byte)mOperationMode);
            buf.putByte(mFPS);
            buf.putByte(mNOF);
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

            byte tmp = 0xFF;
            buf.getByte(ref tmp);
            mOperationMode = (DetectorOperationMode)tmp;
            buf.getByte(ref mFPS);
            buf.getByte(ref mNOF);
        }

        public override UInt16 length()
        {
            return 3;
        }
    }
}

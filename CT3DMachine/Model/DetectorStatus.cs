using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CT3DMachine.Helper;
using NLog;

namespace CT3DMachine.Model
{
    class DetectorStatus : BaseMessage
    {
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();
       
        private DetectorState mState;
        private DetectorOperationMode mOperationMode;
        private byte mFPS;
        private byte mNOF; // number of frame
        private UInt16 mBMPRate;  

        public DetectorStatus(MessageType messageType = MessageType.DETECTOR_STATUS_MESSAGE) : base(MessageType.DETECTOR_STATUS_MESSAGE)
        {
            mState = DetectorState.DISCONNECTED;
            mOperationMode = DetectorOperationMode.BINDING;
            mFPS = 0;
            mNOF = 0;
            mBMPRate = 0;
        }

        public DetectorStatus(DetectorState _state, DetectorOperationMode _mode, byte _fps, byte _nof, UInt16 _BMPRate, MessageType messageType = MessageType.DETECTOR_STATUS_MESSAGE) : base(MessageType.DETECTOR_STATUS_MESSAGE)
        {
            mState = _state;
            mOperationMode = _mode;
            mFPS = _fps;
            mNOF = _nof;
            mBMPRate = _BMPRate;
        }

        public DetectorState getState() { return mState; }
        public DetectorOperationMode getOperationMode() { return mOperationMode; }
        public byte getFPS() { return mFPS; }
        public byte getNOF() { return mNOF; }
        public UInt16 getBMPRate() { return mBMPRate; }

        public override byte[] serialize()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.putByte((byte)mState);
            buf.putByte((byte)mOperationMode);
            buf.putByte(mFPS);
            buf.putByte(mNOF);
            buf.putUInt16(mBMPRate);
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
            mState = (DetectorState)tmp;
            buf.getByte(ref tmp);
            mOperationMode = (DetectorOperationMode)tmp;
            buf.getByte(ref mFPS);
            buf.getByte(ref mNOF);
            buf.getUInt16(ref mBMPRate);
        }
        
        public override UInt16 length()
        {
            return 6;
        }
    }
}

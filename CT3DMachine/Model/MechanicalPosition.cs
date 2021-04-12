using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CT3DMachine.Helper;

namespace CT3DMachine.Model
{
    class MechanicalPosition : BaseMessage
    {
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();
        public MechanicalPosition(MessageType messageType) : base(messageType)
        {
            
        }
        
        private Int32 mFirstEnginePosition;
        private Int32 mSecondEnginePosition;
        private Int32 mThirdEnginePosition;
        private Int32 mFourthEnginePosition;
        private Int32 mFifthEnginePosition;

        public override byte[] serialize()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.putInt32(mFirstEnginePosition);
            buf.putInt32(mSecondEnginePosition);
            buf.putInt32(mThirdEnginePosition);
            buf.putInt32(mFourthEnginePosition);
            buf.putInt32(mFifthEnginePosition);
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
            buf.getInt32(ref mFirstEnginePosition);
            buf.getInt32(ref mSecondEnginePosition);
            buf.getInt32(ref mThirdEnginePosition);
            buf.getInt32(ref mFourthEnginePosition);
            buf.getInt32(ref mFifthEnginePosition);            
        }
        
        public Int32 getFirstEnginePosition()
        {
            return mFirstEnginePosition;
        }

        public void setFirstEnginePosition(Int32 pos)
        {
            mFirstEnginePosition = pos;
        }

        public Int32 getSecondEnginePosition()
        {
            return mSecondEnginePosition;
        }

        public void setSecondEnginePosition(Int32 pos)
        {
            mSecondEnginePosition = pos;
        }

        public Int32 getThirdEnginePosition()
        {
            return mThirdEnginePosition;
        }

        public void setThirdEnginePosition(Int32 pos)
        {
            mThirdEnginePosition = pos;
        }

        public Int32 getFourthEnginePosition()
        {
            return mFourthEnginePosition;
        }

        public void setFourthEnginePosition(Int32 pos)
        {
            mFourthEnginePosition = pos;
        }

        public Int32 getFifthEnginePosition()
        {
            return mFifthEnginePosition;
        }

        public void setFifthEnginePosition(Int32 pos)
        {
            mFifthEnginePosition = pos;
        }

        public override UInt16 length()
        {
            return 20;
        }
    }
}

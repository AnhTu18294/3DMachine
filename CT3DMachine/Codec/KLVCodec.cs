using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using CT3DMachine.Model;
using CT3DMachine.Helper;

namespace CT3DMachine.Codec
{
    class KLVCodec : CodecAbstract
    {
        private const int KLV_MINIMUM_LENGTH = 7;
        private const byte SYNC1 = 0x24;
        private const byte SYNC2 = 0x40;
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();
        private List<byte> mBuffer = new List<byte>();

        public override List<BaseMessage> decode(byte[] data)
        {
            mBuffer.AddRange(data);
            Logger.Info("Detector Received: {0}", BitConverter.ToString(data));
            List<BaseMessage> listMsg = new List<BaseMessage>();            
            while (mBuffer.Count >= KLV_MINIMUM_LENGTH)
            {
                if((mBuffer[0] != SYNC1) || (mBuffer[1] != SYNC2))
                {
                    mBuffer.RemoveAt(0);
                    continue;
                }

                byte[] bKeys = mBuffer.GetRange(2, 2).ToArray();
                Array.Reverse(bKeys);
                ushort key = BitConverter.ToUInt16(bKeys, 0);

                byte[] bPLengths = mBuffer.GetRange(4, 2).ToArray();
                Array.Reverse(bPLengths);
                ushort pLength = BitConverter.ToUInt16(bPLengths, 0);

                int messageLength = KLV_MINIMUM_LENGTH + pLength;
                if(mBuffer.Count < messageLength) { break; }
                byte cs = this.checksum(mBuffer.GetRange(2, pLength + 4).ToArray());
                if (cs != mBuffer[messageLength - 1])
                {
                    mBuffer.RemoveAt(0);
                    continue;
                }
                byte[] payload = mBuffer.GetRange(6, pLength).ToArray();                
                mBuffer.RemoveRange(0, messageLength);

                BaseMessage message = this.makeMessage((MessageType)key, payload);
                if (null == message) continue;
                listMsg.Add(message);
            }

            return listMsg;
        }       

        public override byte[] encode(BaseMessage msg)
        {
            byte[] outByte;
            byte[] outCSBytes;

            ByteBuffer buf = new ByteBuffer();
            ByteBuffer bufCS = new ByteBuffer();

            buf.putByte(SYNC1);
            buf.putByte(SYNC2);

            buf.putUInt16((ushort)msg.getMessageType());
            buf.putUInt16(msg.length());

            bufCS.putUInt16((ushort)msg.getMessageType());
            bufCS.putUInt16(msg.length());

            byte[] payload = msg.serialize(); // data = payload   
                              
            buf.putBytes(payload, payload.Length);
            bufCS.putBytes(payload, payload.Length);

            outCSBytes = new byte[payload.Length + 4];
            bufCS.getBytes(ref outCSBytes, outCSBytes.Length);

            byte cs = this.checksum(outCSBytes);
            buf.putByte(cs);
            
            //
            int length = buf.getTotalLength();
            outByte = new byte[length];
            buf.getBytes(ref outByte, length);

            return outByte;
        }

        private byte checksum(byte[] data)
        {
            byte cs = 0x00;
            for(int i = 0; i < data.Length; i++)
            {
                cs += data[i];
            }
            cs = Convert.ToByte(255 - (int)cs);
            return cs;
        }

        private BaseMessage makeMessage(MessageType _key, byte[] payload)
        {
            BaseMessage message = null;

            switch (_key)
            {
                case MessageType.DETECTOR_STATUS_MESSAGE:
                    {
                        message = new DetectorStatus();
                        message.deserialize(payload);
                        break;
                    }
                case MessageType.DETECTOR_GET_IMAGE_DONE:
                    {
                        message = new DetectorGetImageDone();
                        message.deserialize(payload);
                        break;
                    }
                case MessageType.DETECTOR_ERROR_MESSAGE:
                    {
                        message = new DetectorError();
                        message.deserialize(payload);
                        break;
                    }
                default:
                    {
                        //Logger.Info("DetectorCodec key = {0} does NOT match any key", BitConverter.ToString(BitConverter.GetBytes((ushort)_key)));
                        break;
                    }
            }

            return message;
        }
    }
}

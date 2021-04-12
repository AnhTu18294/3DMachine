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
    class MotionCodec : CodecAbstract
    {
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();
        private const int MOTION_CMD_MINIMUM_LENGTH = 5;
        private const byte HEADER = 0xFE;
        private const byte FOOTER = 0xFD;
        private List<byte> mBuffer = new List<byte>();

        public override List<BaseMessage> decode(byte[] data)
        {
            mBuffer.AddRange(data);
            //Logger.Info("data len = {0}, buffer length = {1} : {2}", data.Length, mBuffer.Count, BitConverter.ToString(mBuffer.ToArray()));
            List<BaseMessage> listMsg = new List<BaseMessage>();
            while (mBuffer.Count >= MOTION_CMD_MINIMUM_LENGTH)
            {
                if ((mBuffer[0] != HEADER))
                {
                    mBuffer.RemoveAt(0);
                    continue;
                }

                ushort pLength = (ushort)mBuffer[1];                

                byte[] bKeys = mBuffer.GetRange(2, 2).ToArray();
                Array.Reverse(bKeys);
                ushort key = BitConverter.ToUInt16(bKeys, 0);                

                int messageLength = MOTION_CMD_MINIMUM_LENGTH + pLength;
                if (mBuffer.Count < messageLength) { break; }

                byte[] payload = mBuffer.GetRange(4, pLength).ToArray();
               
                if (FOOTER != mBuffer[messageLength - 1])
                {
                    mBuffer.RemoveAt(0);
                    continue;
                }
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
            ByteBuffer buf = new ByteBuffer();

            buf.putByte(HEADER);

            buf.putByte((byte)msg.length());
            buf.putUInt16((ushort)msg.getMessageType());            

            byte[] payload = msg.serialize(); // data = payload            
            buf.putBytes(payload, payload.Length);
            
            buf.putByte(FOOTER);

            //
            int length = buf.getTotalLength();
            outByte = new byte[length];
            buf.getBytes(ref outByte, length);

            return outByte;
        }
            
        private BaseMessage makeMessage(MessageType _key, byte[] payload)
        {
            BaseMessage message = null;
            switch (_key)
            {
                case MessageType.MECHANICAL_REQUEST_INFO:
                    {
                        //Logger.Info("\nRequest Info {0}", BitConverter.ToString(BitConverter.GetBytes((ushort)_key)));
                        break;
                    }
                case MessageType.MECHANICAL_MOVE_MOTOR:
                    {
                        //Logger.Info("\nMove Motor = {0}", BitConverter.ToString(BitConverter.GetBytes((ushort)_key)));
                        break;
                    }
                case MessageType.MECHANICAL_POSITION_CHANGED:
                    {
                        message = new MechanicalResponse(MessageType.MECHANICAL_POSITION_CHANGED);
                        message.deserialize(payload);
                        break;
                    }
                case MessageType.MECHANICAL_HOME_MOTOR:
                    {
                        message = new MechanicalResponse(MessageType.MECHANICAL_HOME_MOTOR);
                        message.deserialize(payload);
                        //Logger.Info("\nHome Motor = {0}", BitConverter.ToString(BitConverter.GetBytes((ushort)_key)));
                        break;
                    }
                case MessageType.MECHANICAL_MACHINE_READY:
                    {
                        message = new MechanicalResponse(MessageType.MECHANICAL_MACHINE_READY);
                        message.deserialize(payload);
                        //Logger.Info("\nMachine Ready = {0}", BitConverter.ToString(BitConverter.GetBytes((ushort)_key)));
                        break;
                    }
                case MessageType.MECHANICAL_MACHINE_BUSY:
                    {
                        message = new MechanicalResponse(MessageType.MECHANICAL_MACHINE_BUSY);
                        message.deserialize(payload);
                        //Logger.Info("\nMachine Busy = {0}", BitConverter.ToString(BitConverter.GetBytes((ushort)_key)));
                        break;
                    }
                case MessageType.MECHANICAL_MACHINE_ERROR:
                    {
                        message = new MechanicalResponse(MessageType.MECHANICAL_MACHINE_ERROR);
                        message.deserialize(payload);
                        //Logger.Info("\nMachine Error = {0}", BitConverter.ToString(BitConverter.GetBytes((ushort)_key)));
                        break;
                    }
                case MessageType.MECHANICAL_ENABLE_MOTOR:
                    {
                        //Logger.Info("\nEnable Motor = {0}", BitConverter.ToString(BitConverter.GetBytes((ushort)_key)));
                        break;
                    }
                case MessageType.MECHANICAL_DISABLE_MOTOR:
                    {
                        //Logger.Info("\nDisable Motor = {0}", BitConverter.ToString(BitConverter.GetBytes((ushort)_key)));
                        break;
                    }
                case MessageType.MECHANICAL_RT_POSITION:
                    {
                        message = new MechanicalPosition(MessageType.MECHANICAL_RT_POSITION);
                        message.deserialize(payload);
                        break;
                    }
                case MessageType.MECHANICAL_UNKNOWN:
                    {
                        //Logger.Info("Unkown Message: {0}", BitConverter.ToString(BitConverter.GetBytes((ushort)_key)));
                        break;
                    }
                default:
                    {
                        //Logger.Info("\nMotionCodec key = {0} does NOT match any key", BitConverter.ToString(BitConverter.GetBytes((ushort)_key)));
                        break;
                    }
            }
            return message;            
        }
    }
}

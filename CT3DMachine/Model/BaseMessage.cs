using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CT3DMachine.Model
{
    public enum DetectorOperationMode : byte
    {
        NORMAL = 0x00,
        BINDING = 0x01
    }

    public enum DetectorState : byte
    {
        DISCONNECTED = 0x00,
        CONNECTED = 0x01,
        CALIBARATING = 0x02,
        CALIBRATE_DARK_DONE = 0x03,
        CALIBRATE_BRIGHT_DONE = 0x04,
        READY_GET_IMAGE = 0x05
    }

    public enum MessageType : ushort
    {
        //------- MECHANICAL MESSAGE
        MECHANICAL_REQUEST_INFO = 0x0001,
        MECHANICAL_MOVE_MOTOR = 0x0002,
        MECHANICAL_POSITION_CHANGED = 0x0003,        
        MECHANICAL_HOME_MOTOR = 0x0004,
        MECHANICAL_MACHINE_READY = 0x0005,
        MECHANICAL_MACHINE_BUSY = 0x0006,
        MECHANICAL_MACHINE_ERROR = 0x0007,
        MECHANICAL_ENABLE_MOTOR = 0x0008,
        MECHANICAL_DISABLE_MOTOR = 0x0009,
        MECHANICAL_RT_POSITION = 0x000A,
        MECHANICAL_UNKNOWN = 0x000B,

        //------- DETECTOR MESSAGE
        DETECTOR_STATUS_MESSAGE = 0x0101,
        DETECTOR_CALIB_DARK_MESSAGE = 0x0102,
        DETECTOR_CALIB_BRIGHT_MESSAGE = 0x0103,
        DETECTOR_CONFIGURATION_MESSAGE = 0x0104,
        DETECTOR_GET_IMAGE = 0x0105,
        DETECTOR_GET_IMAGE_DONE = 0x0106,
        DETECTOR_STOP_MESSAGE = 0x0107,
        DETECTOR_ERROR_MESSAGE = 0x0108,
    }

    public abstract class BaseMessage
    {        
        public BaseMessage(MessageType messageType)
        {
            mMessageType = messageType;
        }

        private MessageType mMessageType;

        public MessageType getMessageType()
        {
            return mMessageType;
        }

        public abstract byte[] serialize();

        public abstract void deserialize(byte[] array);

        public abstract UInt16 length();

    }
}

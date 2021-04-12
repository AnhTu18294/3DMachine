using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CT3DMachine.Helper;

namespace CT3DMachine.Model
{
    class MechanicalResponse: BaseMessage
    {
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();
        
        public MechanicalResponse(MessageType messageType) : base(messageType)
        {
            
        }

        public override void deserialize(byte[] array)
        {

        }

        public override byte[] serialize()
        {
            byte[] payload = new byte[0];
            return payload;
        }

        public override UInt16 length()
        {
            return 0;
        }
    }
}

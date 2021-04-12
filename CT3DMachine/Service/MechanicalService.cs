using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using NLog;
using CT3DMachine.Connector;
using CT3DMachine.Codec;
using CT3DMachine.Model;

namespace CT3DMachine.Service
{
    class MechanicalService: ServiceAbstract
    {
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

        private Helper.SynchronousTimer mTimer = null;
        private UInt16 mCurrentCommand;

        public MechanicalService(ConnectorAbstract con, CodecAbstract codec): base(con, codec)
        {
            
        }

        public override void processIoMessage(BaseMessage msg)
        {
            passingMessage(msg);
        }

        public override void processInnerMessage(BaseMessage msg)
        {
            send(msg);            
        }

    }
}

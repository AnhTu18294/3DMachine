using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using CT3DMachine.Connector;
using CT3DMachine.Codec;
using CT3DMachine.Model;

namespace CT3DMachine.Service
{
    class DetectorService : ServiceAbstract
    {
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();
        public DetectorService(ConnectorAbstract con, CodecAbstract codec): base(con, codec)
        {

        }

        //Process message from Detector
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

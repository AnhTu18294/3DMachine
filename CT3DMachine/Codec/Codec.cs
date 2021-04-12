using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CT3DMachine.Model;

namespace CT3DMachine.Codec
{
    abstract class CodecAbstract
    {
        public abstract byte[] encode(BaseMessage msg);

        public abstract List<BaseMessage> decode(byte[] data);
        
    }
}

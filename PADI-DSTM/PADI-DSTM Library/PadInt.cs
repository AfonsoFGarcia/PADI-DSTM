using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PADI_DSTM
{
    public class PadInt
    {
        private iData primary;
        private iData backup;
        private int id;

        public PadInt(iData p, iData b, int i)
        {
            backup = b;
            primary = p;
            id = i;
        }

        public int Read()
        {
            if (PadiDstm.currentTid == -1) throw new TxException("Not in a transaction");
            return primary.ReadValue(PadiDstm.currentTid, id);
        }

        public void Write(int value)
        {
            if (PadiDstm.currentTid == -1) throw new TxException("Not in a transaction");
            if (value <= int.MinValue) { throw new ArgumentOutOfRangeException("value"); }
            primary.WriteValue(PadiDstm.currentTid, id, value);
        }
    }
}

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
        private int value;

        public PadInt(iData p, iData b, int i)
        {
            backup = b;
            primary = p;
            id = i;
            value = primary.ReadValue(PadiDstm.currentTid, id);

        }

        public int Read()
        {
            if (PadiDstm.currentTid == -1) throw new TxException("Not in a transaction");
            return value;
        }

        public void Write(int v)
        {
            if (PadiDstm.currentTid == -1) throw new TxException("Not in a transaction");
            if (v <= int.MinValue) { throw new ArgumentOutOfRangeException("value"); }
            value = v;
        }

        public void Flush()
        {
            if (PadiDstm.currentTid == -1) throw new TxException("Not in a transaction");
            primary.WriteValue(PadiDstm.currentTid, id, value);
        }
    }
}

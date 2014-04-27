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
        private Boolean writeLock;

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
            if (!writeLock) { writeLock = primary.GetWriteLock(PadiDstm.currentTid, id); }
            value = v;
        }

        public void Flush()
        {
            if (PadiDstm.currentTid == -1) throw new TxException("Not in a transaction");
            if (writeLock) primary.WriteValue(PadiDstm.currentTid, id, value);
        }
    }
}

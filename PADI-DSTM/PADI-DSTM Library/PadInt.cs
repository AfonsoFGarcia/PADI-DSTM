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
            try {
                return primary.ReadValue(PadiDstm.currentTid, id);
            }
            catch (Exception)
            {
                try
                {
                    return backup.ReadValue(PadiDstm.currentTid, id);
                }
                catch (Exception)
                {
                    PadiDstm.c.AbortTransaction();
                    throw new TxException("Could not locate server");
                }
            }
            
        }

        public void Write(int value)
        {
            if (PadiDstm.currentTid == -1) throw new TxException("Not in a transaction");

            try
            {
                primary.WriteValue(PadiDstm.currentTid, id, value);
            }
            catch (Exception)
            {
                primary = null;
            }

            try
            {
                backup.WriteValue(PadiDstm.currentTid, id, value);
            }
            catch (Exception)
            {
                backup = null;
            }

            if (primary == null && backup == null)
            {
                PadiDstm.c.AbortTransaction();
                throw new TxException("Could not locate servers");
            }

        }
    }
}

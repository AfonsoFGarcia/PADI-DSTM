using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PADI_DSTM
{
    public interface iData
    {
        bool Fail();
        bool Freeze();
        bool Recover();
        bool CreateObject(IntPadInt p, int tid);
        bool HasObject(int id, int tid);
        bool Status();
        int ReadValue(int tid, int id);
        void WriteValue(int tid, int id, int value);
        Boolean GetWriteLock(int tid, int id);
    }
}

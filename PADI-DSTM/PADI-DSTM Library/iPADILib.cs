using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// PADI-DSTM Common Types
namespace PADI_DSTM
{
    interface iPADILib
    {
        static bool Init();
        static bool TxBegin();
        static bool TxCommit();
        static bool TxAbort();
        static bool Status();
        static bool Fail(string URL);
        static bool Freeze(string URL);
        static bool Recover(string URL);
        static PadInt CreatePadInt(int uid);
        static PadInt AccessPadInt(int uid);
    }
}

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
        bool Init();
        bool TxBegin();
        bool TxCommit();
        bool TxAbort();
        bool Status();
        bool Fail(string URL);
        bool Freeze(string URL);
        bool Recover(string URL);
        PadInt CreatePadInt (int uid);
        PadInt AccessPadInt (int uid);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// PADI-DSTM Common Types
namespace PADI_DSTM
{
    public class PADILib : iPADILib
    {
        bool Init()
        {
            return true;
        }

        bool TxBegin()
        {
            return true;
        }

        bool TxCommit()
        {
            return true;
        }

        bool TxAbort()
        {
            return true;
        }

        bool Status()
        {
            return true;
        }

        bool Fail(string URL)
        {
            return true;
        }

        bool Freeze(string URL)
        {
            return true;
        }

        bool Recover(string URL)
        {
            return true;
        }

        PadInt CreatePadInt (int uid)
        {
            return new PadInt(uid);
        }

        PadInt AccessPadInt (int uid)
        {
            return new PadInt(uid);
        }
    }
}

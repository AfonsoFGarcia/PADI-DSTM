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
        iMaster master;

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
            PadInt p = new PadInt(uid);
            String[] URLs = master.RegisterPadInt(uid);
            
            // Espetar com o gajo nos servers

            return p;
        }

        PadInt AccessPadInt (int uid)
        {
            String[] URLs = master.GetServerURL(uid);

            // Ir buscar o gajo

            return new PadInt(uid);
        }
    }
}

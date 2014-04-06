using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PADI_DSTM
{
    public interface iMaster
    {
        int RegisterServer(String URL);
        String[] GetServerURL(int padIntID);
        String[] RegisterPadInt(int padIntID);
        int GetPadIntID();
        bool Status();
        int GetUniqueTransactionId();
    }
}

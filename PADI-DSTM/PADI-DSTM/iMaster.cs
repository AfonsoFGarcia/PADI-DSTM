using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PADI_DSTM
{
    public interface iMaster
    {
        bool RegisterServer(String URL);
        String[] GetServerURL(int padIntID, int tid);
        String[] RegisterPadInt(int padIntID, int tid);
        int GetPadIntID();
        bool Status();
        int GetUniqueTransactionId(int coordinatorID);
        int RegisterCoordinator();
        bool UnregisterCoordinator(int tid);
        string GetCoordinator(int tid);
    }
}

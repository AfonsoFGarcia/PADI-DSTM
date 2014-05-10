using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PADI_DSTM
{
    public interface iCoordinator
    {
        void JoinTransaction(int tid, string url);
    }
}

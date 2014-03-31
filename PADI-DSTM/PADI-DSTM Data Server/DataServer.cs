using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// PADI-DSTM Data Server
namespace PADI_DSTM
{
    class Program
    {
        static void Main(string[] args)
        {
        }
    }

    class Data : iData
    {
        bool freeze = false;
        bool fail = false;

        public bool Fail()
        {
            fail = true;
            return fail;
        }

        public bool Freeze()
        {
            freeze = true;
            return freeze;
        }

        public bool Recover()
        {
            freeze = false;
            fail = false;
            return !(fail || freeze);
        }
    }
}

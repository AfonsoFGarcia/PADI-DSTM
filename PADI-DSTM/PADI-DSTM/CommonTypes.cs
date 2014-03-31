using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// PADI-DSTM Common Types
namespace PADI_DSTM
{
    public class PadInt
    {
        int id;
        int value;

        public PadInt(int i)
        {
            id = i;
        }

        public int Read()
        {
            return value;
        }

        public void Write(int v)
        {
            value = v;
        }
    }
}

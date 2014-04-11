using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// PADI-DSTM Common Types
namespace PADI_DSTM
{
    [Serializable]
    public class IntPadInt
    {
        int id;
        int value;

        public IntPadInt(int i)
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

        public int GetId()
        {
            return id;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PADI_DSTM;

namespace ConsoleApplication2
{
    class Program
    {
        static void Main(string[] args)
        {
            PADILib l = new PADILib();
            if (l.Init())
            {
                l.CreatePadInt(0);
                PadInt p = l.AccessPadInt(1);

                System.Console.WriteLine(p.GetId());
                System.Console.ReadLine();
            }
        }
    }
}

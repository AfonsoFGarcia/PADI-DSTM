using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting;
using System.Net.Sockets;

// PADI-DSTM Data Server
namespace PADI_DSTM
{
    class Program
    {
        static void Main(string[] args)
        {
            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full;
            IDictionary props = new Hashtable();

            System.Console.Write("Enter port number: ");
            string port = System.Console.ReadLine();
            props["port"] = Int32.Parse(port);

            TcpChannel channel = new TcpChannel(props, null, provider);
            ChannelServices.RegisterChannel(channel, true);

            iMaster m = (iMaster)Activator.GetObject(typeof(iMaster), "tcp://localhost:8080/MasterServer");

            bool res = false;
            try
            {
                res = m.RegisterServer("tcp://localhost:"+port+"/DataServer");
            }
            catch (SocketException)
            {
                System.Console.WriteLine("Could not find master server.");
                return;
            }

            if (!res)
            {
                System.Console.WriteLine("Could not register server.");
                return;
            }

            Data so = new Data();
            RemotingServices.Marshal(so, "DataServer", typeof(Data));
            
            System.Console.WriteLine("<enter> to exit...");
            System.Console.ReadLine();
        }
    }

    public class Data : MarshalByRefObject, iData, iCoordinated {
        bool freeze = false;
        bool fail = false;
        Hashtable objects = new Hashtable();

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

        public bool CreateObject(PadInt p)
        {
            if (objects.ContainsKey(p.GetId()))
            {
                return false;
            }
            else
            {
                System.Console.WriteLine("Created PadInt " + p.GetId());
                objects.Add(p.GetId(), p);
                return true;
            }
        }

        public PadInt GetObject(int id)
        {
            return (PadInt) objects[id];
        }

        public bool Status()
        {
            System.Console.WriteLine("Failed: " + fail);
            System.Console.WriteLine("Freeze: " + freeze);
            return true;
        }

        public bool canCommit(int tid)
        {
            return true;
        }

        public bool doCommit(int tid)
        {
            return true;
        }

        public bool doAbort(int tid)
        {
            return true;
        }
    }
}

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

            TcpChannel channel = new TcpChannel();
            ChannelServices.RegisterChannel(channel, true);

            iMaster m = (iMaster)Activator.GetObject(typeof(iMaster), "tcp://localhost:8080/MasterServer");

            int port = 0;
            try
            {
                port = m.RegisterServer("DataServer");
            }
            catch (SocketException)
            {
                System.Console.WriteLine("Could not find master server.");
                return;
            }

            ChannelServices.UnregisterChannel(channel);

            if (port == 0)
            {
                System.Console.WriteLine("Could not register server.");
                return;
            }

            props["port"] = port;
            channel = new TcpChannel(props, null, provider);
            ChannelServices.RegisterChannel(channel, true);
            Data so = new Data();
            RemotingServices.Marshal(so, "DataServer", typeof(Data));
            
            System.Console.WriteLine("<enter> to exit...");
            System.Console.ReadLine();
        }
    }

    public class Data : MarshalByRefObject, iData {
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
                objects.Add(p.GetId(), p);
                return true;
            }
        }

        public PadInt GetObject(int id)
        {
            return (PadInt) objects[id];
        }
    }
}

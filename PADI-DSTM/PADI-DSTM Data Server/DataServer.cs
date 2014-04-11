using System;
using System.Collections;
using System.Collections.Generic;
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
                res = m.RegisterServer("tcp://localhost:"+port+"/Server");
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

            Data so = new Data(m, (int)props["port"]);
            RemotingServices.Marshal(so, "Server", typeof(Data));
            
            System.Console.WriteLine("<enter> to exit...");
            System.Console.ReadLine();
        }
    }

    public class Data : MarshalByRefObject, iData, iCoordinated {
        bool freeze = false;
        bool fail = false;
        Hashtable objects = new Hashtable();
        Hashtable log = new Hashtable();
        iMaster master;
        int port;

        public Data(iMaster m, int p)
        {
            master = m;
            port = p;
        }

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

        public bool CreateObject(IntPadInt p, int tid)
        {
            inTransaction(tid);
            if (objects.ContainsKey(p.GetId()))
            {
                return false;
            }
            else
            {
                Dictionary<int, int> tLog = (Dictionary<int, int>)log[tid];
                tLog.Add(p.GetId(), p.Read());
                return true;
            }
        }

        private void inTransaction(int tid)
        {
            if (!log.ContainsKey(tid))
            {
                string URL = master.GetCoordinator(tid);
                iCoordinator c = (iCoordinator)Activator.GetObject(typeof(iCoordinator), URL);
                string dURL = "tcp://localhost:"+port+"/Server";
                c.JoinTransaction(tid, dURL);
                log.Add(tid, new Dictionary<int, int>());
            }
        }

        public bool HasObject(int id, int tid)
        {
            inTransaction(tid);
            return objects.ContainsKey(id);
        }

        public bool Status()
        {
            System.Console.WriteLine("Failed: " + fail);
            System.Console.WriteLine("Freeze: " + freeze);
            return true;
        }

        public int ReadValue(int tid, int id)
        {
            inTransaction(tid);
            Dictionary<int, int> tLog = (Dictionary<int, int>)log[tid];
            if (!tLog.ContainsKey(id))
            {
                tLog.Add(id, ((IntPadInt)objects[id]).Read());
            }
            return (int)tLog[id];
        }

        public void WriteValue(int tid, int id, int value)
        {
            inTransaction(tid);
            Dictionary<int, int> tLog = (Dictionary<int, int>)log[tid];
            tLog.Add(id, value);
        }

        public bool canCommit(int tid)
        {
            return true;
        }

        public bool doCommit(int tid)
        {
            Dictionary<int, int> tLog = (Dictionary<int, int>)log[tid];
            foreach (KeyValuePair<int, int> obj in tLog)
            {
                if(!objects.ContainsKey(obj.Key)) {
                    objects.Add(obj.Key, new IntPadInt(obj.Key));
                }
                ((IntPadInt)objects[obj.Key]).Write(obj.Value);
            }
            return true;
        }

        public bool doAbort(int tid)
        {
            return true;
        }
    }
}

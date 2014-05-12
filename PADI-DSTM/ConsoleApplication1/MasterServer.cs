using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Threading;

// PADI-DSTM Master Server
namespace PADI_DSTM
{
    class Program
    {
        static void Main(string[] args)
        {
            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full;
            IDictionary props = new Hashtable();

            props["port"] = 8080;
            props["timeout"] = 750;
            TcpChannel channel = new TcpChannel(props, null, provider);
            ChannelServices.RegisterChannel(channel, true);

            RemotingConfiguration.RegisterWellKnownServiceType(typeof(Master), "MasterServer", WellKnownObjectMode.Singleton);

            System.Console.WriteLine("<enter> to exit...");
            System.Console.ReadLine();
        }
    }

    public class ServerList
    {
        public String URL;
        public int id;
        public ServerList next;
        public bool alive;
    }

    class Master : MarshalByRefObject, iMaster, iCoordinated
    {
        int nextPadInt;
        int uniqueId;
        int coordinatorId;
        int numServers;
        ServerList servers;
        ServerList lastServer;
        Hashtable padInts;
        Dictionary<int, Hashtable> transactions2PadInts;

        Hashtable coordinators;

        public Master()
        {
            nextPadInt = 0;
            numServers = 0;
            uniqueId = 0;
            coordinatorId = 0;
            servers = null;
            lastServer = null;
            padInts = new Hashtable();
            coordinators = new Hashtable();
            transactions2PadInts = new Dictionary<int, Hashtable>();

            ThreadStart ts = new ThreadStart(this.AliveThread);
            Thread t = new Thread(ts);
            t.Start();
        }

        public void AliveThread()
        {
           
         while (true)
            {
                Thread.Sleep(90000);
                ServerList p = servers;
                if (p == null)
                {
                    continue;
                }
                do
                {
                    iData d = (iData)Activator.GetObject(typeof(iData), p.URL);
                    try{
                        d.alive();
                        p.alive = true;
                        System.Console.WriteLine("I'm alive " + p.URL);
                    }
                    catch (Exception e)
                    {
                        if(d != null)
                            try
                            {
                                d.suicide();
                            }
                            catch (Exception f) { }
                        p.alive = false;
                        System.Console.WriteLine("I'm dead " + p.URL);
                    }
                    p = p.next;
                } while (p.id != servers.id);
            }
        }

        public bool RegisterServer(String URL)
        {
            lock (this)
            {
                if (servers == null)
                {
                    servers = new ServerList();
                    lastServer = servers;
                    servers.id = ++numServers;
                    servers.alive = true;
                    servers.URL = URL;
                    servers.next = servers;
                }
                else
                {
                    ServerList prov = new ServerList();
                    prov.next = lastServer.next;
                    lastServer.next = prov;
                    lastServer = prov;
                    prov.id = ++numServers;
                    prov.alive = true;
                    prov.URL = URL;
                }
                Monitor.PulseAll(this);
            }
            return true;
        }

        public String[] GetServerURL(int padIntID, int tid)
        {
           if (padInts.ContainsKey(padIntID))
           {
                 return (String[])padInts[padIntID];
           }
           else if (transactions2PadInts[tid].ContainsKey(padIntID))
           {
                return (String[])transactions2PadInts[tid][padIntID];
           }

           else return null;
        }

        public String[] RegisterPadInt(int padIntID, int tid)
        {
            inTransaction(tid);
            int serverNum = padIntID % numServers + 1;
            ServerList server = GetServer(serverNum);
            if (server == null) return null;
            String[] URLs = new String[2];
            URLs[0] = server.URL;
            server = GetNextServer(server);
            URLs[1] = server.URL;
            if (padInts.ContainsKey(padIntID)) return null;
            transactions2PadInts[tid].Add(padIntID, URLs);
            return URLs;
        }

        private void inTransaction(int tid)
        {
            if (!transactions2PadInts.ContainsKey(tid))
            {
                string URL = GetCoordinator(tid);
                iCoordinator c = (iCoordinator)Activator.GetObject(typeof(iCoordinator), URL);
                string dURL = "tcp://localhost:8080/MasterServer";
                c.JoinTransaction(tid, dURL);
                transactions2PadInts.Add(tid, new Hashtable());
            }
        }

        public ServerList GetServer(int serverNum)
        {
            ServerList s = servers;
            for (int i = 0; i < numServers; i++)
            {
                if (s.id == serverNum)
                {
                    if (s.alive == false)
                        return GetNextServer(s);
                    else
                    {
                        return s;
                    }
                }
                s = s.next;
            }
            return null;
        }

        public ServerList GetNextServer(ServerList list)
        {
            if (list.next.alive == true)
            {
                return list.next;
            }
            else
            {
                return GetNextServer(list.next);
            }

        }

        public int GetPadIntID()
        {
            if (servers == null) return -1;
            return nextPadInt++;
        }

        public bool Status()
        {
            ServerList p = servers;
            do
            {
                iData d = (iData)Activator.GetObject(typeof(iData), p.URL);
                if (d == null || !d.Status()) return false;
                p = p.next;
            } while (p.id != servers.id);

            return true;
        }

        public int GetUniqueTransactionId(int coordinatorID)
        {
            int tid = uniqueId++;
            coordinators.Add(tid, "tcp://localhost:" + coordinatorID + "/Coordinator");
            return tid;
        }

        public int RegisterCoordinator()
        {
            int tid = coordinatorId++;
            int port = 30000 + tid;

            return port;
        }
        public bool UnregisterCoordinator(int tid)
        {
            coordinators.Remove(tid);
            return true;
        }

        public string GetCoordinator(int tid)
        {
            return (string)coordinators[tid];
        }

        public bool canCommit(int tid)
        {
            return true;
        }
        public bool doCommit(int tid)
        {
            foreach (DictionaryEntry pair in transactions2PadInts[tid])
            {
                padInts.Add(pair.Key, pair.Value);
            }
            transactions2PadInts.Remove(tid);
            return true;
        }
        public bool doAbort(int tid)
        {
            transactions2PadInts.Remove(tid);
            return true;
        }
    
    }

    
}

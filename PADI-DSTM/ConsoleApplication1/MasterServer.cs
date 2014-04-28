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
            TcpChannel channel = new TcpChannel(8080);
            ChannelServices.RegisterChannel(channel, true);

            RemotingConfiguration.RegisterWellKnownServiceType(typeof(Master), "MasterServer", WellKnownObjectMode.Singleton);

            System.Console.WriteLine("<enter> to exit...");
            System.Console.ReadLine();
        }
    }

    class Master : MarshalByRefObject, iMaster
    {
        int nextPadInt;
        int uniqueId;
        int coordinatorId;
        int numServers;
        ServerList servers;
        ServerList lastServer;
        Hashtable padInts;
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
                    prov.URL = URL;
                }
                Monitor.PulseAll(this);
            }
            return true;
        }

        public String[] GetServerURL(int padIntID)
        {
            return (String[])padInts[padIntID];
        }

        public String[] RegisterPadInt(int padIntID)
        {
            int serverNum = padIntID % numServers + 1;
            ServerList server = GetServer(serverNum);
            if (server == null) return null;
            String[] URLs = new String[2];
            URLs[0] = server.URL;
            URLs[1] = server.next.URL;
            if (padInts.ContainsKey(padIntID)) return null;
            padInts.Add(padIntID, URLs);
            return URLs;
        }

        public ServerList GetServer(int serverNum)
        {
            ServerList s = servers;
            for (int i = 0; i < numServers; i++)
            {
                if (s.id == serverNum) return s;
                s = servers.next;
            }
            return null;
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
    }

    public class ServerList
    {
        public String URL;
        public int id;
        public ServerList next;
    }
}

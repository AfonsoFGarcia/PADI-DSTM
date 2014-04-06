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
        int numServers;
        ServerList servers;
        ServerList lastServer;
        Hashtable padInts;

        public Master()
        {
            nextPadInt = 0;
            numServers = 0;
            servers = null;
            lastServer = null;
            padInts = new Hashtable();
        }

        public int RegisterServer(String URL)
        {
            int id = 0;
            lock (this)
            {
                if (servers == null)
                {
                    servers = new ServerList();
                    lastServer = servers;
                    servers.id = ++numServers;
                    servers.URL = "tcp://localhost:" + (lastServer.id + 8080) + "/" + URL;
                    servers.next = servers;
                }
                else
                {
                    ServerList prov = new ServerList();
                    prov.next = lastServer.next;
                    lastServer.next = prov;
                    lastServer = prov;
                    prov.id = ++numServers;
                    prov.URL = "tcp://localhost:" + (lastServer.id + 8080) + "/" + URL;
                }
                id = lastServer.id + 8080;
                System.Console.WriteLine("Registered server " + lastServer.URL);
                Monitor.PulseAll(this);
            }
            return id;
        }

        public String[] GetServerURL(int padIntID)
        {
            return (String[]) padInts[padIntID];
        }

        public String[] RegisterPadInt(int padIntID)
        {
            int serverNum = padIntID.GetHashCode() % numServers;
            ServerList server = GetServer(serverNum);
            String[] URLs = new String[2];
            URLs[0] = server.URL;
            URLs[1] = server.next.URL;
            padInts.Add(padIntID, URLs);
            return new String[2];
        }

        public ServerList GetServer(int serverNum)
        {
            ServerList s = servers;
            while (servers.id != serverNum)
            {
                s = servers.next;
            }
            return s;
        }

        public int GetPadIntID()
        {
            if (servers == null) return -1;
            return nextPadInt++;
        }
    }

    public class ServerList
    {
        public String URL;
        public int id;
        public ServerList next;
    }
}

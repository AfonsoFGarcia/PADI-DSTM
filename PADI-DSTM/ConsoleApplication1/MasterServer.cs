using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// PADI-DSTM Master Server
namespace PADI_DSTM
{
    class Program
    {
        static void Main(string[] args)
        {
        }
    }

    class Master : iMaster
    {
        int nextPadInt = 0;
        int numServers = 0;
        ServerList servers = null;
        ServerList lastServer = null;
        Hashtable padInts = new Hashtable();

        bool RegisterServer(String URL)
        {
            if (servers == null)
            {
                servers = new ServerList();
                lastServer = servers;
                servers.URL = URL;
                servers.id = numServers++;
                servers.next = servers;
            }
            else
            {
                ServerList prov = new ServerList();
                prov.next = lastServer.next;
                lastServer.next = prov;
                prov.URL = URL;
                prov.id = numServers++;
            }
            return true;
        }

        String[] GetServerURL(int padIntID)
        {
            return (String[]) padInts[padIntID];
        }

        String[] RegisterPadInt(int padIntID)
        {
            int serverNum = padIntID.GetHashCode() % numServers;
            ServerList server = GetServer(serverNum);
            String[] URLs = new String[2];
            URLs[0] = server.URL;
            URLs[1] = server.next.URL;
            padInts.Add(padIntID, URLs);
            return new String[2];
        }

        ServerList GetServer(int serverNum)
        {
            ServerList s = servers;
            while (servers.id != serverNum)
            {
                s = servers.next;
            }
            return s;
        }

        int GetPadIntID()
        {
            if (servers == null) return -1;
            return nextPadInt++;
        }
    }

    class ServerList
    {
        public String URL;
        public int id;
        public ServerList next;
    }
}

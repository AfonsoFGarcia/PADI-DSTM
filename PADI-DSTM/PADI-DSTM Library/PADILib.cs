using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;

// PADI-DSTM Common Types
namespace PADI_DSTM
{
    public class PadiDstm
    {
        static iMaster master;
        public static Coordinator c;
        static TcpChannel channel;
        public static int currentTid = -1;
        static Dictionary<string, iData> dataServers = new Dictionary<string, iData>();

        static public bool Init()
        {
            channel = new TcpChannel();
            ChannelServices.RegisterChannel(channel, true);
            master = (iMaster)Activator.GetObject(typeof(iMaster), "tcp://localhost:8080/MasterServer");
            c = new Coordinator(master, channel);
            return master != null;
        }

        static public bool TxBegin()
        {
            c.CreateTransaction();
            currentTid = c.tid;
            return true;
        }

        static public bool TxCommit()
        {
            try
            {
                c.CommitTransaction();
                return true;
            }
            catch (TxException e)
            {
                return false;
            }
            finally
            {
                currentTid = -1;
            }
        }

        static public bool TxAbort()
        {
            try
            {
                c.AbortTransaction();
                return true;
            }
            catch (TxException e)
            {
                return false;
            }
            finally
            {
                currentTid = -1;
            }
        }

        static public bool Status()
        {
            return master.Status();
        }

        private static iData getServer(string URL)
        {
            if (dataServers.ContainsKey(URL))
            {
                return dataServers[URL];
            }
            else
            {
                iData d = (iData)Activator.GetObject(typeof(iData), URL);
                dataServers.Add(URL, d);
                return d;
            }
        }

        static public bool Fail(string URL)
        {
            iData d = getServer(URL);
            return !(d == null || !d.Fail());
        }

        static public bool Freeze(string URL)
        {
            iData d = getServer(URL);
            return !(d == null || !d.Freeze());
        }

        static public bool Recover(string URL)
        {
            iData d = getServer(URL);
            return !(d == null || !d.Recover());
        }

        static public PadInt CreatePadInt(int uid)
        {
            if (currentTid == -1) throw new TxException("Not in a transaction");
            IntPadInt p = new IntPadInt(uid);
            String[] URLs = master.RegisterPadInt(uid);

            if (URLs == null) return null;

            iData d1 = getServer(URLs[0]);
            iData d2 = getServer(URLs[1]);


            try
            {
                if (!d1.CreateObject(p, currentTid))
                {
                    System.Console.WriteLine("Object already exists.");
                    return null;
                }
            }
            catch (Exception)
            {
                System.Console.WriteLine("Could not locate server");
                return null;
            }

            if (URLs[0] != URLs[1])
            {
                try
                {
                    d2.CreateObject(p, currentTid);
                }
                catch (Exception)
                {
                    System.Console.WriteLine("Could not locate server");
                    return null;
                }
            }

            PadInt r = new PadInt(d1, d2, uid);

            return r;
        }

        static public PadInt AccessPadInt(int uid)
        {
            if (currentTid == -1) throw new TxException("Not in a transaction");
            String[] URLs = master.GetServerURL(uid);

            if (URLs == null) return null;

            iData d1 = getServer(URLs[0]);
            iData d2 = getServer(URLs[1]);

            try
            {
                if (!d1.HasObject(uid, currentTid))
                {
                    System.Console.WriteLine("Object does not exists.");
                    return null;
                }
            }
            catch (Exception)
            {
                d1 = null;
            }

            
            if (URLs[0] != URLs[1])
            {
                try
                {
                    if (!d2.HasObject(uid, currentTid))
                    {
                        System.Console.WriteLine("Object does not exists.");
                        return null;
                    }
                 }
                 catch (Exception)
                 {
                    d2 = null;
                 }
            }

            if (d1 == null && d2 == null)
            {
                System.Console.WriteLine("Could not locate server");
                return null;
            }

            PadInt r = new PadInt(d1, d2, uid);

            return r;
        }
    }
}

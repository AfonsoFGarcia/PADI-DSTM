using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Collections;

// PADI-DSTM Common Types
namespace PADI_DSTM
{
    public class PadiDstm
    {
        static iMaster master;
        static Coordinator c;
        static TcpChannel channel;
        public static int currentTid = -1;
        static Dictionary<string, iData> dataServers = new Dictionary<string, iData>();
        static Stack transactionPadInt = new Stack();
        static int clientID;
        static int transactionID;

        static private int GetNewTransactionID()
        {
            int retVal = clientID * 1000 + transactionID++;
            transactionID = transactionID % 1000;
            if (transactionID == 0)
            {
                clientID = master.GetUniqueTransactionId();
            }
            return retVal;
        }

        static public bool Init()
        {
            channel = new TcpChannel();
            ChannelServices.RegisterChannel(channel, true);

            master = (iMaster)Activator.GetObject(typeof(iMaster), "tcp://localhost:8080/MasterServer");
            clientID = master.GetUniqueTransactionId();
            transactionID = 0;
            return master != null;
        }

        static public bool TxBegin()
        {
            c = new Coordinator(GetNewTransactionID());
            currentTid = c.tid;
            c.CreateTransaction();
            return true;
        }

        static public bool TxCommit()
        {
            try
            {
                while (transactionPadInt.Count > 0)
                {
                    PadInt var = (PadInt) transactionPadInt.Pop();
                    var.Flush();
                }

                c.CommitTransaction();
                return true;
            }
            catch (TxException e)
            {
                return false;
            }
            finally
            {
                c = null;
                currentTid = -1;
            }
        }

        static public bool TxAbort()
        {
            try
            {
                while (transactionPadInt.Count > 0)
                {
                    transactionPadInt.Pop();
                }
                c.AbortTransaction();
                return true;
            }
            catch (TxException e)
            {
                return false;
            }
            finally
            {
                c = null;
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

            if (d1 == null)
            {
                System.Console.WriteLine("Could not locate server");
                return null;
            }

            if (!d1.CreateObject(p, currentTid))
            {
                System.Console.WriteLine("Object already exists.");
                return null;
            }

            c.JoinTransaction(currentTid, URLs[0]);

            if (URLs[0] != URLs[1])
            {
                if (d2 == null)
                {
                    System.Console.WriteLine("Could not locate server");
                    return null;
                }

                d2.CreateObject(p, currentTid);
                c.JoinTransaction(currentTid, URLs[1]);
            }

            PadInt r = new PadInt(d1, d2, uid);

            transactionPadInt.Push(r);
            return r;
        }

        static public PadInt AccessPadInt(int uid)
        {
            if (currentTid == -1) throw new TxException("Not in a transaction");
            String[] URLs = master.GetServerURL(uid);

            if (URLs == null) return null;

            iData d1 = getServer(URLs[0]);
            iData d2 = getServer(URLs[1]);

            if (d1 == null)
            {
                if (URLs[0] != URLs[1])
                {
                    if (d2 == null)
                    {
                        System.Console.WriteLine("Could not locate server");
                        return null;
                    }

                    if (d2.HasObject(uid, currentTid))
                    {
                        System.Console.WriteLine("Object does not exists.");
                        return null;
                    }
                    c.JoinTransaction(currentTid, URLs[1]);
                }
                else
                {
                    System.Console.WriteLine("Could not locate server");
                    return null;
                }
            }

            if (!d1.HasObject(uid, currentTid))
            {
                System.Console.WriteLine("Object does not exists.");
                return null;
            }
            c.JoinTransaction(currentTid, URLs[0]);

            PadInt r = new PadInt(d1, d2, uid);
            transactionPadInt.Push(r);
            return r;
        }
    }
}

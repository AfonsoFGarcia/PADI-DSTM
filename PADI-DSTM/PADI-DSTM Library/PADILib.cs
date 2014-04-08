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
        static Coordinator c;
        static TcpChannel channel;

        static public bool Init()
        {
            channel = new TcpChannel();
            ChannelServices.RegisterChannel(channel, true);

            master = (iMaster)Activator.GetObject(typeof(iMaster), "tcp://localhost:8080/MasterServer");
            System.Console.WriteLine("Initialized Lib");
            return master != null;
        }

        static public bool TxBegin()
        {
            c = new Coordinator();
            c.CreateTransaction();
            System.Console.WriteLine("Begin TX");
            return true;
        }

        static public bool TxCommit()
        {
            c.CommitTransaction();
            c = null;
            System.Console.WriteLine("Commit TX");
            return true;
        }

        static public bool TxAbort()
        {
            c.AbortTransaction();
            c = null;
            return true;
        }

        static public bool Status()
        {
            return master.Status();
        }

        static public bool Fail(string URL)
        {
            iData d = (iData)Activator.GetObject(typeof(iData), URL);
            return !(d == null || !d.Fail());
        }

        static public bool Freeze(string URL)
        {
            iData d = (iData)Activator.GetObject(typeof(iData), URL);
            return !(d == null || !d.Freeze());
        }

        static public bool Recover(string URL)
        {
            iData d = (iData)Activator.GetObject(typeof(iData), URL);
            return !(d == null || !d.Recover());
        }

        static public PadInt CreatePadInt(int uid)
        {
            PadInt p = new PadInt(uid);
            String[] URLs = master.RegisterPadInt(uid);

            if (URLs == null) return null;

            System.Console.WriteLine(URLs[0]);
            System.Console.WriteLine(URLs[1]);

            iData d1 = (iData)Activator.GetObject(typeof(iData), URLs[0]);
            
            if (d1 == null) {
                System.Console.WriteLine("Could not locate server");
                return null;
            }

            if (!d1.CreateObject(p))
            {
                System.Console.WriteLine("Object already exists.");
                return null;
            }

            if (URLs[0] != URLs[1])
            {
                iData d2 = (iData)Activator.GetObject(typeof(iData), URLs[1]);
                if (d2 == null)
                {
                    System.Console.WriteLine("Could not locate server");
                    return null;
                }

                d2.CreateObject(p);
            }

            return p;
        }

        static public PadInt AccessPadInt(int uid)
        {
            String[] URLs = master.GetServerURL(uid);
            PadInt p = null;

            if (URLs == null) return null;

            iData d1 = (iData)Activator.GetObject(typeof(iData), URLs[0]);

            if (d1 == null)
            {
                if (URLs[0] != URLs[1])
                {
                    iData d2 = (iData)Activator.GetObject(typeof(iData), URLs[1]);
                    if (d2 == null)
                    {
                        System.Console.WriteLine("Could not locate server");
                        return null;
                    }

                    if ((p = d2.GetObject(uid)) == null)
                    {
                        System.Console.WriteLine("Object does not exists.");
                        return null;
                    }
                }
                else
                {
                    System.Console.WriteLine("Could not locate server");
                    return null;
                }
            }

            if ((p = d1.GetObject(uid)) == null)
            {
                System.Console.WriteLine("Object does not exists.");
                return null;
            }

            return p;
        }
    }
}

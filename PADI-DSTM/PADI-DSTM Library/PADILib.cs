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
    public class PADILib : iPADILib
    {
        iMaster master;

        public bool Init()
        {
            TcpChannel channel = new TcpChannel();
            ChannelServices.RegisterChannel(channel, true);

            master = (iMaster)Activator.GetObject(typeof(iMaster), "tcp://localhost:8080/MasterServer");
            System.Console.WriteLine("Initialized Lib");
            return master != null;
        }

        public bool TxBegin()
        {
            return true;
        }

        public bool TxCommit()
        {
            return true;
        }

        public bool TxAbort()
        {
            return true;
        }

        public bool Status()
        {
            return master.Status();
        }

        public bool Fail(string URL)
        {
            iData d = (iData)Activator.GetObject(typeof(iData), URL);
            return !(d == null || !d.Fail());
        }

        public bool Freeze(string URL)
        {
            iData d = (iData)Activator.GetObject(typeof(iData), URL);
            return !(d == null || !d.Freeze());
        }

        public bool Recover(string URL)
        {
            iData d = (iData)Activator.GetObject(typeof(iData), URL);
            return !(d == null || !d.Recover());
        }

        public PadInt CreatePadInt(int uid)
        {
            PadInt p = new PadInt(uid);
            String[] URLs = master.RegisterPadInt(uid);

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

        public PadInt AccessPadInt(int uid)
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

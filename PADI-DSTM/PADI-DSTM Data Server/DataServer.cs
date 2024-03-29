﻿using System;
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
                res = m.RegisterServer("tcp://localhost:" + port + "/Server");
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

    public class Data : MarshalByRefObject, iData, iCoordinated
    {
        bool freeze = false;
        bool fail = false;
        Hashtable objects = new Hashtable();
        Hashtable log = new Hashtable();
        Dictionary<int, String> padIntUrls = new Dictionary<int, String>();
        Dictionary<int, bool> canCommitState = new Dictionary<int, bool>();
        Dictionary<string, iCoordinator> coordinators = new Dictionary<string, iCoordinator>();
        iMaster master;
        iCoordinator c;
        int port;

        public Data(iMaster m, int p)
        {
            master = m;
            port = p;
        }

        public bool Fail()
        {
            while (freeze) { }
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
            return !freeze;
        }

        public bool alive()
        {
            while (freeze) { }
            return true;
        }

        public void suicide()
        {
            System.Environment.Exit(-1);
        }

        public bool CreateObject(IntPadInt p, int tid)
        {
            while (freeze) { }

            inTransaction(tid);
            if (!canCommitState[tid]) return false;
            if (objects.ContainsKey(p.GetId()))
            {
                return false;
            }
            else
            {
                Dictionary<int, int> tLog = (Dictionary<int, int>)log[tid];
                tLog.Add(p.GetId(), p.Read());
                padIntUrls.Add(p.GetId(), p.GetOther());
                return true;
            }
        }

        private void inTransaction(int tid)
        {
            if (!log.ContainsKey(tid))
            {
                string URL = master.GetCoordinator(tid);
                c = (iCoordinator)Activator.GetObject(typeof(iCoordinator), URL);
                string dURL = "tcp://localhost:" + port + "/Server";
                c.JoinTransaction(tid, dURL);
                log.Add(tid, new Dictionary<int, int>());
                canCommitState.Add(tid, true);
            }
        }

        private bool setLock(int l, int uid, int tid)
        {
            IntPadInt obj = (IntPadInt)objects[uid];
            if (obj == null) return true;
            return obj.setLock(l, tid);
        }

        public bool HasObject(int id, int tid)
        {
            while (freeze) { }
            inTransaction(tid);
            if (!canCommitState[tid]) return false;
            return objects.ContainsKey(id);
        }

        public bool Status()
        {
            while (freeze) { }
           
            System.Console.WriteLine("----------STATUS----------");
            System.Console.WriteLine("Failed: " + fail);
            System.Console.WriteLine("Freeze: " + freeze);
            System.Console.WriteLine();

            foreach (Object obj in objects.Values)
            {
                System.Console.Write("PadInt ");
                System.Console.Write(((IntPadInt)obj).GetId());
                System.Console.Write(": ");
                System.Console.WriteLine(((IntPadInt)obj).Read());
            }

            return true;
        }

        public int ReadValue(int tid, int id)
        {
            while (freeze) { }
            inTransaction(tid);
            if (!canCommitState[tid]) return int.MinValue;
            if (!setLock((int)IntPadInt.Locks.READ, id, tid))
            {
                canCommitState[tid] = false;
                return int.MinValue;
            }
            Dictionary<int, int> tLog = (Dictionary<int, int>)log[tid];
            if (!tLog.ContainsKey(id))
            {
                tLog.Add(id, ((IntPadInt)objects[id]).Read());
            }
            return (int)tLog[id];
        }

        public void WriteValue(int tid, int id, int value)
        {
            while (freeze) { }
            inTransaction(tid);
            if (!canCommitState[tid]) return;
            if (!setLock((int)IntPadInt.Locks.WRITE, id, tid))
            {
                canCommitState[tid] = false;
                return;
            }
            Dictionary<int, int> tLog = (Dictionary<int, int>)log[tid];
            if (tLog.ContainsKey(id))
            {
                tLog[id] = value;
            }
            else
            {
                tLog.Add(id, value);
            }
        }

        public bool canCommit(int tid)
        {
            while (freeze) { }
            return canCommitState[tid];
        }

        public bool doCommit(int tid)
        {
            Dictionary<int, int> tLog = (Dictionary<int, int>)log[tid];
            foreach (KeyValuePair<int, int> obj in tLog)
            {
                if (!objects.ContainsKey(obj.Key))
                {
                    objects.Add(obj.Key, new IntPadInt(obj.Key, padIntUrls[obj.Key]));
                }
                ((IntPadInt)objects[obj.Key]).Write(obj.Value);
                setLock((int)IntPadInt.Locks.FREE, obj.Key, tid);
            }
            c = null;
            log.Remove(tid);
            return true;
        }

        public bool doAbort(int tid)
        {
            Dictionary<int, int> tLog = (Dictionary<int, int>)log[tid];
            foreach (KeyValuePair<int, int> obj in tLog)
            {
                setLock((int)IntPadInt.Locks.FREE, obj.Key, tid);
            }
            c = null;
            log.Remove(tid);
            return true;
        }
    }
}

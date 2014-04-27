using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

// PADI-DSTM Common Types
namespace PADI_DSTM
{
    [Serializable]
    public class IntPadInt
    {

        public enum Locks { FREE, OBW, OBR, OBRAWP, RFW }
        public enum Type { FREE, READ, WRITE }
        public Dictionary<int, Type> lockType = new Dictionary<int, Type>();
        private int id;
        private int value;
        int t;
        int RR = 0;
        int RW = 0;
        int WW = 0;

        Object readMonitor = new Object();
        Object writeMonitor = new Object();
        Object lockMonitor = new Object();

        public IntPadInt(int i)
        {
            id = i;
            t = (int) Locks.FREE;
        }

        public int Read()
        {
            return value;
        }

        public void Write(int v)
        {
            value = v;
        }

        public int GetId()
        {
            return id;
        }

        public bool setLock(int t, int tid)
        {
            if (t == (int)Type.READ)
            {
                if (lockType.ContainsKey(tid) && lockType[tid] != Type.FREE) { 
                 
                }
                else
                {
                    lockType[tid] = (Type) t;
                    System.Console.WriteLine("Calling waitToRead for transaction " + tid);
                    return waitToRead();
                }
            }
            else if (t == (int)Type.WRITE)
            {
                if (lockType.ContainsKey(tid) && lockType[tid] == Type.WRITE)
                {

                }
                else if (lockType.ContainsKey(tid) && lockType[tid] == Type.READ)
                {
                    System.Console.WriteLine("Calling doneRead for transaction " + tid);
                    doneReading();
                    lockType[tid] = Type.WRITE;
                    System.Console.WriteLine("Calling waitToWrite for transaction " + tid);
                    return waitToWrite();
                }
                else
                {
                    lockType[tid] = Type.WRITE;
                    System.Console.WriteLine("Calling waitToWrite for transaction " + tid);
                    return waitToWrite();
                }
            }
            else
            {
                if (lockType.ContainsKey(tid) && lockType[tid] == Type.WRITE)
                {
                    lockType[tid] = Type.FREE;
                    System.Console.WriteLine("Calling doneWriting for transaction " + tid);
                    doneWriting();
                }
                else if (lockType.ContainsKey(tid) && lockType[tid] == Type.READ)
                {
                    lockType[tid] = Type.FREE;
                    System.Console.WriteLine("Calling doneReading for transaction " + tid);
                    doneReading();
                }
            }
            return true;
        }

        public bool waitToWrite()
        {
            DateTime begin = DateTime.Now;
            while (true)
            {
                Monitor.Enter(lockMonitor);
                if (t == (int)Locks.FREE)
                {
                    t = (int)Locks.OBW;
                    Monitor.PulseAll(lockMonitor);
                    Monitor.Exit(lockMonitor);
                    return true;
                }
                else if (t == (int)Locks.RFW)
                {
                    t = (int)Locks.OBW;
                    Monitor.PulseAll(lockMonitor);
                    Monitor.Exit(lockMonitor);
                    return true;
                }
                else if (t == (int)Locks.OBR || t == (int)Locks.OBRAWP)
                {
                    WW++;
                    Monitor.PulseAll(lockMonitor);
                    Monitor.Exit(lockMonitor);
                    Monitor.Wait(writeMonitor);
                }
                else
                {
                    Monitor.PulseAll(lockMonitor);
                    Monitor.Exit(lockMonitor);
                }
            }
        }

        public bool waitToRead()
        {
            DateTime begin = DateTime.Now;
            while (true)
            {        
                Monitor.Enter(lockMonitor);
                if (t == (int)Locks.FREE)
                {
                    t = (int)Locks.OBR;
                    RR = 1;
                    Monitor.PulseAll(lockMonitor);
                    Monitor.Exit(lockMonitor);
                    return true;
                }
                else if (t == (int)Locks.OBR)
                {
                    RR++;
                    Monitor.PulseAll(lockMonitor);
                    Monitor.Exit(lockMonitor);
                    return true;
                }
                else if (t == (int)Locks.OBW || t == (int)Locks.RFW || t == (int)Locks.OBRAWP)
                {
                    RW++;
                    Monitor.PulseAll(lockMonitor);
                    Monitor.Exit(lockMonitor);
                    Monitor.Wait(readMonitor);
                }
                else
                {
                    Monitor.PulseAll(lockMonitor);
                    Monitor.Exit(lockMonitor);
                }
            }
        }

        public bool doneWriting()
        {
            lock (lockMonitor)
            {
                    if (WW == 0)
                    {
                        t = (int)Locks.FREE;
                        Monitor.PulseAll(lockMonitor);
                        return true;
                    }
                    else if (WW > 0)
                    {
                        t = (int)Locks.RFW;
                    }

                    while (t == (int)Locks.RFW && WW > 0)
                    {
                        Monitor.Pulse(writeMonitor);
                        WW--;
                    }
                    while (t == (int)Locks.FREE || t == (int)Locks.OBR || RW > 0)
                    {
                        Monitor.PulseAll(readMonitor);
                        RW = 0;
                    }
                    Monitor.PulseAll(lockMonitor);
                    return true;
                
            }
        }

        public bool doneReading()
        {
            lock (lockMonitor)
            {

                RR--;

                if (RR == 0 && WW == 0)
                {
                    t = (int)Locks.FREE;
                }
                else if (RR == 0 && WW > 0)
                {
                    t = (int)Locks.RFW;
                }
                else if (t == (int)Locks.OBR || t == (int)Locks.OBRAWP || t == (int)Locks.FREE)
                {
                }
                else if (t == (int)Locks.RFW || WW > 0)
                {
                    Monitor.Pulse(writeMonitor);
                    WW--;
                }
                Monitor.PulseAll(lockMonitor);
                return true;
            }
        }
    }
}

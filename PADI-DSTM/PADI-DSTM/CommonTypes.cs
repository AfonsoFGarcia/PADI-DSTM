using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// PADI-DSTM Common Types
namespace PADI_DSTM
{
    [Serializable]
    public class IntPadInt
    {

        public enum Locks { FREE, READ, WRITE }
        private Dictionary<int, int> locks = new Dictionary<int, int>();
        private int id;
        private int value;
        private Object thisLock;
        private Boolean flag;
        public IntPadInt(int i)
        {
            id = i;
            thisLock = new Object();
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
            DateTime begin = DateTime.Now;
            flag = !flag;
            while ((DateTime.Now - begin).TotalMilliseconds < (flag ? 1000 : 0) + new Random().Next(500, 2000))
            {
                lock (thisLock)
                {
                    if (t == (int)Locks.WRITE)
                    {
                        if (locks.Count == 0)
                        {
                            locks.Add(tid, t);
                            return true;
                        }
                        else if (locks.Count == 1)
                        {
                            if (locks.ContainsKey(tid))
                            {
                                locks[tid] = t;
                                return true;
                            }
                        }
                    }
                    else if (t == (int)Locks.READ)
                    {
                        if (locks.Count == 1)
                        {
                            if (locks.ContainsKey(tid))
                            {
                                return true;
                            }
                            if (locks.Values.First() == (int)Locks.READ)
                            {
                                locks.Add(tid, t);
                                return true;
                            }
                        }
                        else if (!locks.ContainsKey(tid))
                        {
                            locks.Add(tid, t);
                            return true;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else if (t == (int)Locks.FREE)
                    {
                        locks.Remove(tid);
                        return true;
                    }
                }
            }

            printState(t);
            return false;
        }

        void printState(int t)
        {
            int rl = 0;
            int wl = 0;
            lock (thisLock)
            {
                foreach (int l in locks.Values)
                {
                    if (l == (int)Locks.READ)
                    {
                        rl++;
                    }
                    else wl++;


                }
            }
            System.Console.WriteLine("Tried lock of type " + (t == (int)Locks.READ ? "READ" : "WRITE"));
            System.Console.WriteLine("Read locks: " + rl);
            System.Console.WriteLine("Write locks: " + wl);
        }

    }

}

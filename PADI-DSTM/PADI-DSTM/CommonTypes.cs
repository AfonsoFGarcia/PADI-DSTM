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
            while ((DateTime.Now - begin).TotalSeconds < 10)
            {
                lock (thisLock)
                {
                    if (t == (int)Locks.WRITE)
                    {
                        if (locks.Count == 0)
                        {
                            if (locks.ContainsKey(tid))
                            {
                                locks[tid] = t;
                            }
                            else
                            {
                                locks.Add(tid, t);
                            }
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

                            int value = locks.Values.First();

                            if (value == (int)Locks.READ)
                            {
                                locks.Add(tid, t);
                                return true;
                            }

                        }
                        else
                        {
                            if (locks.ContainsKey(tid))
                            {
                                locks[tid] = t;
                            }
                            else
                            {
                                locks.Add(tid, t);
                            }
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
            return false;
        }
    }
}

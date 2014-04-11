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
        Dictionary<int, int> locks = new Dictionary<int, int>();
        int id;
        int value;

        public IntPadInt(int i)
        {
            id = i;
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
            while ((DateTime.Now - begin).TotalMilliseconds < 1000)
            {
                lock (this)
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
                            foreach (KeyValuePair<int, int> val in locks)
                            {
                                if (val.Key == tid)
                                {
                                    locks.Add(tid, t);
                                    return true;
                                }
                            }
                        }
                    }

                    if (t == (int)Locks.READ)
                    {
                        if (locks.Count == 1)
                        {
                            foreach (KeyValuePair<int, int> val in locks)
                            {
                                if (val.Value == (int)Locks.READ)
                                {
                                    locks.Add(tid, t);
                                    return true;
                                }
                                else if (val.Key == tid)
                                {
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            locks.Add(tid, t);
                        }
                    }

                    if (t == (int)Locks.FREE)
                    {
                        locks.Remove(tid);
                        return true;
                    }
                    return true;
                }
            }
            return false;
        }
    }
}

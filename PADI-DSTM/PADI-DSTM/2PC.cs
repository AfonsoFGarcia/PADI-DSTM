using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
namespace PADI_DSTM
{
    public class Coordinator
    {
        public int tid;
        public ArrayList transactionMembers;

        public int CreateTransaction()
        {
            transactionMembers = new ArrayList();
            iMaster m = (iMaster)Activator.GetObject(typeof(iMaster), "tcp://localhost:8080/MasterServer");
            if (m == null) throw new TxException("Could not get a transaction id");
            return tid = m.GetUniqueTransactionId();
        }
        public void CommitTransaction()
        {
            System.Console.WriteLine("For 1 begin");
            iCoordinated member;
            for (int i = 0; i < transactionMembers.Count; i++)
            {
                member = (iCoordinated)transactionMembers[i];
                if (!member.canCommit(tid))
                {
                    AbortTransaction();
                    throw new TxException("Transaction aborted");
                }

            }
            System.Console.WriteLine("For 2 begin");
            for (int i = 0; i < transactionMembers.Count; i++)
            {
                member = (iCoordinated)transactionMembers[i];
                member.doCommit(tid);
            }
            System.Console.WriteLine("End");
        }
        public void AbortTransaction()
        {
            iCoordinated member;
            for (int i = 0; i < transactionMembers.Count; i++)
            {
                member = (iCoordinated)transactionMembers[i];
                member.doAbort(tid);
            }
        }
        public void JoinTransaction(int tid, string url)
        {
            if(tid != this.tid) throw new TxException("Incorrect transaction id");
            iCoordinated member = (iCoordinated)Activator.GetObject(typeof(iCoordinated), url);
            transactionMembers.Add(member);
        }
    }

    public interface iCoordinated
    {
        bool canCommit(int tid);
        bool doCommit(int tid);
        bool doAbort(int tid);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;

namespace PADI_DSTM
{
    public class Coordinator : MarshalByRefObject, iCoordinator
    {
        public int coordinatorID;
        public int tid;
        public ArrayList transactionMembers;
        public iMaster master;

        public Coordinator(iMaster m, TcpChannel channel)
        {
            master = m;
            coordinatorID = master.RegisterCoordinator();
            ChannelServices.UnregisterChannel(channel);
            channel = new TcpChannel(coordinatorID);
            ChannelServices.RegisterChannel(channel, true);
            string name = "Coordinator";
            RemotingServices.Marshal(this, name, typeof(Coordinator));
        }

        public int CreateTransaction()
        {
            tid = master.GetUniqueTransactionId(coordinatorID);
            transactionMembers = new ArrayList();
            return tid;
        }
        public void CommitTransaction()
        {
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
            for (int i = 0; i < transactionMembers.Count; i++)
            {
                member = (iCoordinated)transactionMembers[i];
                member.doCommit(tid);
            }
            master.UnregisterCoordinator(coordinatorID);
        }
        public void AbortTransaction()
        {
            iCoordinated member;
            for (int i = 0; i < transactionMembers.Count; i++)
            {
                member = (iCoordinated)transactionMembers[i];
                member.doAbort(tid);
            }
            master.UnregisterCoordinator(tid);
        }
        public void JoinTransaction(int tid, string url)
        {
            if (tid != this.tid) throw new TxException("Incorrect transaction id");
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

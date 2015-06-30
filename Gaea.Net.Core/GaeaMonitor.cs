using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Gaea.Net.Core
{
    public class GaeaMonitor
    {

        private long accept_postcounter = 0;
        private long accept_responsecounter = 0;
        private long acceptCreateCounter = 0;
        private long acceptDestroyCounter = 0;
        private long acceptRequestGetCounter = 0;
        private long acceptRequestReleaseCounter = 0;

        private long send_postcounter = 0;
        private long send_responsecounter = 0;
        private long send_cancelcounter = 0;

        private long sendRequestCreateCounter = 0;
        private long sendRequestDestoryCounter = 0;

        private long sendRequestGetCounter = 0;
        private long sendRequestReleaseCounter = 0;




        private long sendSize = 0;



        private long recvPostCounter = 0;


        private long recvResponseCounter = 0;


        private long recvSize = 0;


        private long onlineCounter = 0;
 



        public long AcceptPostCounter { get { return accept_postcounter; } }

        public long AcceptResponseCounter { get { return accept_responsecounter; } }

        public long AcceptCreateCounter { get { return acceptCreateCounter; } }

        public long AcceptDestroyCounter { get { return acceptDestroyCounter; } }

        public long AcceptRequestGetCounter { get { return acceptRequestGetCounter; } }



        public long AcceptRequestReleaseCounter { get { return acceptRequestReleaseCounter; } }

        public long SendPostCounter { get { return send_postcounter; } }

        public long SendResponseCounter { get { return send_responsecounter; } }

        public long SendCancelCounter { get { return send_cancelcounter; } }

        public long SendSize      {  get { return sendSize; }  }

        public long OnlineCounter     {          get { return onlineCounter; }       }
        public long RecvPostCounter { get { return recvPostCounter; }    }
        public long RecvResponseCounter{ get { return recvResponseCounter; } }
        
        public long RecvSize { get { return recvSize; } }

        public long SendRequestCreateCounter   {         get { return sendRequestCreateCounter; }     }

        public long SendRequestDestoryCounter   {        get { return sendRequestDestoryCounter; }  }

        public long SendRequestGetCounter { get { return sendRequestGetCounter; } }

        public long SendRequestReleaseCounter { get { return sendRequestReleaseCounter; } }

        
        public void IncOnline()
        {
            Interlocked.Increment(ref onlineCounter);
        }

        public void DecOnline()
        {
            Interlocked.Decrement(ref onlineCounter);
        }

        public void IncSendRequestCreateCounter()
        {
            Interlocked.Increment(ref sendRequestCreateCounter);
        }

        public void IncSendRequestDestroyCounter()
        {
            Interlocked.Increment(ref sendRequestDestoryCounter);
        }

        public void IncSendRequestGetCounter()
        {
            Interlocked.Increment(ref sendRequestGetCounter);
        }

        public void IncSendRequestReleaseCounter()
        {
            Interlocked.Increment(ref sendRequestReleaseCounter);
        }

        public void IncAcceptPostCounter()
        {
            Interlocked.Increment(ref accept_postcounter);
        }

        public void IncAcceptCreateCounter()
        {
            Interlocked.Increment(ref acceptCreateCounter);
        }

        public void IncAcceptGetCounter()
        {
            Interlocked.Increment(ref acceptRequestGetCounter);
        }

        public void IncAcceptDestroyCounter()
        {
            Interlocked.Increment(ref acceptDestroyCounter);
        }

        public void IncAcceptRequestReleaseCounter()
        {
            Interlocked.Increment(ref acceptRequestReleaseCounter);
        }

        public void IncAcceptResponseCounter()
        {
            Interlocked.Increment(ref accept_responsecounter);
        }

        public void IncSendPostCounter()
        {
            Interlocked.Increment(ref send_postcounter);
        }

        public void IncSendResponseCounter()
        {
            Interlocked.Increment(ref send_responsecounter);
        }
        public void IncSendCancelCounter()
        {
            Interlocked.Increment(ref send_cancelcounter);
        }

        public void IncSendSize(long size)
        {
            Interlocked.Add(ref sendSize, size);
        }


        public void IncRecvPostCounter()
        {
            Interlocked.Increment(ref recvPostCounter);
        }

        public void IncRecvResponseCounter()
        {
            Interlocked.Increment(ref recvResponseCounter);
        }

        public void IncRecvSize(long size)
        {
            Interlocked.Add(ref recvSize, size);
        }

        public void Reset()
        {
            accept_postcounter = 0;
            accept_responsecounter = 0;
            send_postcounter = 0;
            send_responsecounter = 0;
            send_cancelcounter = 0;
            sendSize = 0;
            recvPostCounter = 0;
            recvResponseCounter = 0;
            recvSize = 0;
            sendRequestDestoryCounter = 0;
            sendRequestCreateCounter = 0;
            sendRequestGetCounter = 0;
            sendRequestReleaseCounter = 0;
        }
    }
}

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
        private long send_postcounter = 0;
        private long send_responsecounter = 0;
        private long send_cancelcounter = 0;

        private long send_size = 0;
        private long recv_postcounter = 0;
        private long recv_responsecounter = 0;
        private long recv_size = 0;


        public void IncAcceptPostCounter()
        {
            Interlocked.Increment(ref accept_postcounter);
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
            Interlocked.Add(ref send_size, size);
        }


        public void IncRecvPostCounter()
        {
            Interlocked.Increment(ref recv_postcounter);
        }

        public void IncRecvResponseCounter()
        {
            Interlocked.Increment(ref recv_responsecounter);
        }

        public void IncRecvSize(long size)
        {
            Interlocked.Add(ref recv_size, size);
        }

        public void Reset()
        {
            accept_postcounter = 0;
            accept_responsecounter = 0;
            send_postcounter = 0;
            send_responsecounter = 0;
            send_cancelcounter = 0;
            send_size = 0;
            recv_postcounter = 0;
            recv_responsecounter = 0;
            recv_size = 0;
        }
    }
}

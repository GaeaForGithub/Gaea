using Gaea.Net.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gaea.Samples.Echo
{
    public class GaeaEchoSocketContext:GaeaSocketContext
    {
        public override void OnRecvBuffer(System.Net.Sockets.SocketAsyncEventArgs e)
        {
            Debug.WriteLine(String.Format("{0}接收到数据, 长度:{1}", RawSocket.Handle, e.BytesTransferred));
            PostSendRequest(e.Buffer, 0, e.BytesTransferred, true);
            Thread.Sleep(100);
        }
    }
}

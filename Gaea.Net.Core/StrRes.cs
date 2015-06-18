using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gaea.Net.Core
{
    public class StrRes
    {
        public const string STR_AcceptException = "[{0}]:响应Accept请求出现了异常, 错误代码:{1}";
        public const string STR_ReceiveException = "[{0}]:响应接受时出现异常, 接收长度:{1}, 错误代码:{2}";
        public const string STR_ReceiveContextIsOff = "[{0}]:响应接受时, 发现Context已经请求断开, 接收到的数据将被放弃!";
        public const string STR_SendContextIsOff = "[{0}]:继续处理发送请求时, 发现Context已经请求断开, 剩余发送缓存队列将被清理({1})!";
        public const string STR_SendContextException = "[{0}]:响应投递的异步请求时出现了异常, 处理字节:{1}, 错误代码:{2}";
        public const string STR_ServerOff = "[{0}]:服务已经停止";

        public const string STR_WaitContextRelease = "[{0}]:即将进入等待所有连接断开时间，如果连接逻辑出现阻塞，或者造成线程阻塞，将无法正常停止...";


    
    }
}

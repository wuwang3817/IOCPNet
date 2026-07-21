using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PENet
{
    //基于IOCP封装的异步套接字通信 服务端
    public class IOCPServer
    {
        Socket skt;
        SocketAsyncEventArgs saea;
        public int backlog = 100;
        IOCPTokenPool pool;
        List<IOCPToken> tokenLst;

        public IOCPServer()
        {
            saea = new SocketAsyncEventArgs();
            saea.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
        }

        public void StartAsServer(string ip, int port, int maxConnCout)
        {
            pool = new IOCPTokenPool(maxConnCout);
            for(int i = 0; i < maxConnCout; i++)
            {
                IOCPToken token = new IOCPToken { tokenID = i };
                pool.Push(token);
            }
            tokenLst = new List<IOCPToken>();
            IPEndPoint pt = new IPEndPoint(IPAddress.Parse(ip), port);
            skt = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            skt.Bind(pt);
            skt.Listen(backlog);
            IOCPTool.ColorLog(IOCPColor.Green, "Server Start");
            StartAccept();
        }

        void StartAccept()
        {
            bool suspend = skt.AcceptAsync(saea);
            if (!suspend)
            {
                ProcessAccept();
            }
        }

        void ProcessAccept()
        {

        }

        void IO_Completed(object sender, SocketAsyncEventArgs saea)
        {
            ProcessAccept();
        }
    }
}

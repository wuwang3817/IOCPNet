using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace PENet
{
    //基于IOCP封装的异步套接字通信 服务端
    public class IOCPServer
    {
        Socket skt;
        SocketAsyncEventArgs saea;

        int curConnCount = 0;
        public int backlog = 100;
        Semaphore acceptSeamapore;
        IOCPTokenPool pool;
        List<IOCPToken> tokenLst;

        public IOCPServer()
        {
            saea = new SocketAsyncEventArgs();
            saea.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
        }

        public void StartAsServer(string ip, int port, int maxConnCout)
        {
            curConnCount = 0;
            acceptSeamapore=new Semaphore(maxConnCout, maxConnCout);
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
            acceptSeamapore.WaitOne();
            bool suspend = skt.AcceptAsync(saea);
            if (!suspend)
            {
                ProcessAccept();
            }
        }

        void ProcessAccept()
        {
            Interlocked.Increment(ref curConnCount);
            IOCPToken token=pool.Pop();
            lock(tokenLst)
            {
                tokenLst.Add(token);
            }
            token.InitToken(saea.AcceptSocket);
            token.onTokenClose = OnTokenClose;
            IOCPTool.ColorLog(IOCPColor.Green, "Client Online, Allocate tokenID:{0}", token.tokenID);
            StartAccept();
        }

        void OnTokenClose(int tokenID)
        {
            int index = -1;
            for(int i=0;i<tokenLst.Count; i++)
            {
                if(tokenLst[i].tokenID==tokenID)
                {
                    index=i;
                    break;
                }
            }
            if (index != -1)
            {
                pool.Push(tokenLst[index]);
                lock (tokenLst)
                {
                    tokenLst.RemoveAt(index);
                }
                Interlocked.Decrement(ref curConnCount);
                acceptSeamapore.Release();
            }
            else
            {
                IOCPTool.Error("Token:{0} cannot find in server tokenLst.", tokenID);
            }
        }
        public List<IOCPToken> GetTokenLst()
        {
            return tokenLst;
        }

        void IO_Completed(object sender, SocketAsyncEventArgs saea)
        {
            ProcessAccept();
        }
    }
}

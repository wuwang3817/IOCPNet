using System;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;

namespace PENet
{
    [Serializable]
    //网络数据协议消息体
    public abstract class IOCPMsg { }

    //基于IOCP封装的异步套接字通信
    public class IOCPNet<T,K> where T: IOCPToken<K>,new() where K: IOCPMsg,new()
    {
        Socket skt;
        SocketAsyncEventArgs saea;

        public IOCPNet()
        {
            saea = new SocketAsyncEventArgs();
            saea.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
        }

        #region Client
        public T token;
        public void StartAsClient(string ip, int port)
        {
            IPEndPoint pt = new IPEndPoint(IPAddress.Parse(ip), port);
            skt = new Socket(pt.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            saea.RemoteEndPoint = pt;
            IOCPTool.ColorLog(IOCPColor.Green, "Client Start");
            StartConnect();
        }
        void StartConnect()
        {
            bool suspend = skt.ConnectAsync(saea);
            if (!suspend)
            {
                ProcessConnect();
            }
            else
            {
                IOCPTool.Log("连接挂起");
            }
        }

        //异步事件挂起：连接没有建立成功，等待连接建立后在创建连接管理类，开始数据收发
        //异步事件没有挂起来：连接建立成功，创建连接管理类，开始数据收发
        void ProcessConnect()
        {
            token = new T();
            token.InitToken(skt);
            //TODO: 连接成功后，创建连接管理类，开始数据收发
            IOCPTool.Log("连接成功");
        }

        public void CloseClient()
        {
            if (token != null)
            {
                token.CloseToken();
                token = null;
            }
            if (skt != null)
            {
                skt = null;
            }
        }
        #endregion

        #region Server
        int curConnCount = 0;
        public int backlog = 100;
        Semaphore acceptSeamapore;
        IOCPTokenPool<T,K> pool;
        List<T> tokenLst;
        public void StartAsServer(string ip, int port, int maxConnCout)
        {
            curConnCount = 0;
            acceptSeamapore=new Semaphore(maxConnCout, maxConnCout);
            pool = new IOCPTokenPool<T,K>(maxConnCout);
            for(int i = 0; i < maxConnCout; i++)
            {
                T token = new T { tokenID = i };
                pool.Push(token);
            }
            tokenLst = new List<T>();
            IPEndPoint pt = new IPEndPoint(IPAddress.Parse(ip), port);
            skt = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            skt.Bind(pt);
            skt.Listen(backlog);
            IOCPTool.ColorLog(IOCPColor.Green, "Server Start");
            StartAccept();
        }
        void StartAccept()
        {
            saea.AcceptSocket = null;
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
            T token=pool.Pop();
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
        public void CloseServer()
        {
           for(int i=0;i<tokenLst.Count; i++)
           {
                tokenLst[i].CloseToken();
           }
           tokenLst = null;
           if (skt!=null)
           {
                skt.Close();
                skt = null;
            }
        }
        public List<T> GetTokenLst()
        {
            return tokenLst;
        }
        #endregion

        void IO_Completed(object sender, SocketAsyncEventArgs saea)
        {
            switch(saea.LastOperation)
            {
                case SocketAsyncOperation.Accept:
                    ProcessAccept();
                    break;
                case SocketAsyncOperation.Connect:
                    ProcessConnect();
                    break;
                default:
                    IOCPTool.Warn("The last operation completed on the socket was not a accept or connect op");
                    break;
            }
        }
    }
}

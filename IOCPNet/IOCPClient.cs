using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PENet
{
    //基于IOCP封装的异步套接字通信 客户端
    public class IOCPClient
    {
        Socket skt;
        public IOCPToken token;
        SocketAsyncEventArgs saea;
        
        
        public IOCPClient()
        {
            saea = new SocketAsyncEventArgs();
            saea.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
        }
        public void StartAsClient(string ip,int port)
        {
            IPEndPoint pt=new IPEndPoint(IPAddress.Parse(ip), port);
            skt = new Socket(pt.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            saea.RemoteEndPoint=pt;
            IOCPTool.ColorLog(IOCPColor.Green, "Client Start");
            StartConnect();
        }

        void StartConnect()
        {
            bool suspend=skt.ConnectAsync(saea);
            if(!suspend)
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
            token= new IOCPToken();
            token.InitToken(skt);
            //TODO: 连接成功后，创建连接管理类，开始数据收发
            IOCPTool.Log("连接成功");
        }

        public void CloseClient()
        {
            if(token!=null)
            {
                token.CloseToken();
                token=null;
            }
            if(skt!=null)
            {
                skt=null;
            }
        }

        void IO_Completed(object sender,SocketAsyncEventArgs saea)
        {

            ProcessConnect();
        }
    }
}

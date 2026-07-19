using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace PENet
{
    //IOCP连接会话Token
    public enum TokenState
    {
        None,
        Connected,
        Disconnected,
    }
    public class ICOPToken
    {
        private Socket skt;
        public TokenState tokenState= TokenState.None;
        private SocketAsyncEventArgs rcvSAEA;

        public ICOPToken() 
        {
            rcvSAEA=new SocketAsyncEventArgs();
            rcvSAEA.Completed+=new EventHandler<SocketAsyncEventArgs>(IO_Completed);
        }

        public void InitToken(Socket skt)
        {
            this.skt=skt;
            tokenState= TokenState.Connected;
            OnConnected();

            StartAsyncRcv();
        }

        void StartAsyncRcv()
        {
            bool suspend=skt.ReceiveAsync(rcvSAEA);
            if(!suspend)
            {
                ProcessRcv();
            }
        }

        void ProcessRcv()
        {

        }
        void IO_Completed(object sender, SocketAsyncEventArgs saea)
        {
            ProcessRcv();
        }

        void OnConnected()
        {
            IOCPTool.Log("Connect Success");
        }
    }
}

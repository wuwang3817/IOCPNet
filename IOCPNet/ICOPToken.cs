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
        public int tokenID;
        private Socket skt;
        private List<byte> readLst=new List<byte>();
        public TokenState tokenState= TokenState.None;
        private SocketAsyncEventArgs rcvSAEA;

        public ICOPToken() 
        {
            rcvSAEA=new SocketAsyncEventArgs();
            rcvSAEA.Completed+=new EventHandler<SocketAsyncEventArgs>(IO_Completed);
            rcvSAEA.SetBuffer(new byte[2048], 0, 2048);
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
            if (rcvSAEA.BytesTransferred > 0 && rcvSAEA.SocketError == SocketError.Success)
            {
                byte[] bytes = new byte[rcvSAEA.BytesTransferred];
                Buffer.BlockCopy(rcvSAEA.Buffer, 0, bytes, 0, rcvSAEA.BytesTransferred);
                readLst.AddRange(bytes);
                ProcessByteLst();
                StartAsyncRcv();
            }
            else
            {
                IOCPTool.Warn("Token:{0} Close:{1}",tokenID,rcvSAEA.SocketError.ToString());
                CloseToken();
            }
        }

        void ProcessByteLst()
        {
            byte[] buff=IOCPTool.SplitLogicBytes(ref readLst);
            if(buff!=null)
            {
            }
        }
        void IO_Completed(object sender, SocketAsyncEventArgs saea)
        {
            ProcessRcv();
        }

        public void CloseToken()
        {

        }

        void OnConnected()
        {
            IOCPTool.Log("Connect Success");
        }
    }
}

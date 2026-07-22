using System;
using System.Net.Sockets;
using System.Collections.Generic;


namespace PENet
{
    //IOCP连接会话Token
    public enum TokenState
    {
        None,
        Connected,
        Disconnected,
    }
    public class IOCPToken
    {
        public int tokenID;
        private Socket skt;
        private List<byte> readLst=new List<byte>();
        private Queue<byte[]> cacheQue=new Queue<byte[]>();
        private bool isWrite = false;
        public Action<int> onTokenClose;
        public TokenState tokenState= TokenState.None;
        private SocketAsyncEventArgs rcvSAEA;
        private SocketAsyncEventArgs sndSAEA;

        public IOCPToken() 
        {
            rcvSAEA=new SocketAsyncEventArgs();
            sndSAEA=new SocketAsyncEventArgs();
            rcvSAEA.Completed+=new EventHandler<SocketAsyncEventArgs>(IO_Completed);
            sndSAEA.Completed+=new EventHandler<SocketAsyncEventArgs>(IO_Completed);
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
                IOCPMsg msg=IOCPTool.Deserialize(buff);
                OnReceiveMsg(msg);
                ProcessByteLst();
            }
        }

        public bool SendMsg(IOCPMsg msg)
        {
            byte[] bytes = IOCPTool.PackLenInfo(IOCPTool.Serialize(msg));
            return SendMsg(bytes);
        }

        public bool SendMsg(byte[] bytes)
        {
            if(tokenState!=TokenState.Connected)
            {
                IOCPTool.Warn("Connection is break,cannot send net msg.");
                return false;
            }
            if(isWrite)
            {
                cacheQue.Enqueue(bytes);
                return true;
            }
            isWrite=true;
            sndSAEA.SetBuffer(bytes, 0, bytes.Length);
            bool suspend=skt.SendAsync(sndSAEA);
            if(!suspend)
            {
                ProcessSend();
            }
            return true;
        }
        void ProcessSend()
        {
            if (sndSAEA.SocketError == SocketError.Success)
            {
                isWrite=false;
                if(cacheQue.Count>0)
                {
                    byte[] item=cacheQue.Dequeue();
                    SendMsg(item);
                }
                IOCPTool.Log("Send Success");
            }
            else
            {
                IOCPTool.Warn("Process Send Error:{0}", sndSAEA.SocketError.ToString());
                CloseToken();
            }
        }

        void IO_Completed(object sender, SocketAsyncEventArgs saea)
        {
            switch(saea.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessRcv();
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend();
                    break;
                default:
                    IOCPTool.Warn("The last operation completed on the socket was not a receive or send operation.");
                    break;
            }
            ProcessRcv();
        }

        public void CloseToken()
        {
            if (skt != null)
            {
                tokenState=TokenState.Disconnected;
                onTokenClose?.Invoke(tokenID);
                OnDisConnected();
                readLst.Clear();
                cacheQue.Clear();
                isWrite=false;
                try
                {
                    skt.Shutdown(SocketShutdown.Send);
                }
                catch (Exception e)
                {
                    IOCPTool.Error("Shutdown Socket Error:{0}",e.ToString());
                }
                finally
                {
                    skt.Close();
                    skt = null;
                }
            }
        }

        void OnConnected()
        {
            IOCPTool.Log("Connect Success");
        }

        void OnReceiveMsg(IOCPMsg msg)
        {
            IOCPTool.Log("收到数据"+msg.hellomsg);
        }

        void OnDisConnected()
        {
            IOCPTool.Log("DisConnected");
        }
    }
}

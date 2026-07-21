using System;
using System.Collections.Generic;
using System.Text;

namespace PENet
{
    //IOCP会话连接缓存池
    public class IOCPTokenPool
    {
        Stack<IOCPToken> stk;
        public int Size=> stk.Count;
        public IOCPTokenPool(int capacity)
        {
            stk=new Stack<IOCPToken>(capacity);
        }
        public IOCPToken Pop()
        {
            lock(stk)
            {
                return stk.Pop();
            }
        }

        public void Push(IOCPToken token)
        {
            if(token==null)
            {
                IOCPTool.Error("push token to pool cannot be null");
            }
            lock(stk)
            {
                stk.Push(token);
            }
        }
    }
}

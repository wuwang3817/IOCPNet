
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace PENet
{
    /// <summary>
    /// 工具
    /// </summary>
    public class IOCPTool
    {
        #region LOG
        public static void ConsoleLog(string msg,IOCPColor color)
        {
            int threadID=Thread.CurrentThread.ManagedThreadId;
            msg=string.Format("Thread:{0} {1}",threadID,msg);
            switch (color)
            {
                case IOCPColor.Red:
                    Console.ForegroundColor= ConsoleColor.DarkRed;
                    Console.WriteLine(msg);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case IOCPColor.Green:
                    Console.ForegroundColor= ConsoleColor.Green;
                    Console.WriteLine(msg);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case IOCPColor.Blue:
                    Console.ForegroundColor= ConsoleColor.Blue;
                    Console.WriteLine(msg);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case IOCPColor.Cyan:
                    Console.ForegroundColor= ConsoleColor.Cyan;
                    Console.WriteLine(msg);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case IOCPColor.Magenta:
                    Console.ForegroundColor= ConsoleColor.Magenta;
                    Console.WriteLine(msg);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case IOCPColor.Yellow:
                    Console.ForegroundColor= ConsoleColor.Yellow;
                    Console.WriteLine(msg);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case IOCPColor.None:
                default:
                    break;
            }

        }
        #endregion
    }
    public enum IOCPColor
    {
        None,
        Red,
        Green,
        Blue,
        Cyan,
        Magenta,
        Yellow,
    }
}

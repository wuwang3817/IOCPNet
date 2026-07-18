
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
        public static Action<string> LogFunc;
        public static Action<IOCPColor,string> ColorLogFunc;
        public static Action<string> WarnFunc;
        public static Action<string> ErrorFunc;
        public static void Log(string msg,params object[] args)
        {
            msg = string.Format(msg, args);
            if (LogFunc != null)
            {
                LogFunc(msg);
            }
            else
            {
                ConsoleLog(msg, IOCPColor.None);
            }
        }
        public static void ColorLog(IOCPColor color,string msg, params object[] args)
        {
            msg = string.Format(msg, args);
            if (ColorLogFunc != null)
            {
                ColorLogFunc(color,msg);
            }
            else
            {
                ConsoleLog(msg, color);
            }
        }
        public static void Warn(string msg, params object[] args)
        {
            msg = string.Format(msg, args);
            if (WarnFunc != null)
            {
                WarnFunc(msg);
            }
            else
            {
                ConsoleLog(msg, IOCPColor.Yellow);
            }
        }
        public static void Error(string msg, params object[] args)
        {
            msg = string.Format(msg, args);
            if (ErrorFunc != null)
            {
                ErrorFunc(msg);
            }
            else
            {
                ConsoleLog(msg, IOCPColor.Red);
            }
        }
        
        private static void ConsoleLog(string msg,IOCPColor color)
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

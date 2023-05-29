using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using System.Runtime.CompilerServices;
namespace Framework
{
    public enum ELogger
    {
        Log,
        Wraing,
        Error
    }
    public sealed class L
    {
        private static readonly string LOGFORMAT = @"[{0}]---->{1}";
        private static object _logLock = new object();
        public static void Log(string title, Object msg,ELogger logType = ELogger.Log)
        {
            Print(string.Format(LOGFORMAT, title, msg.ToString()),logType);
        }

        public static void Log(string title, string msg, ELogger logType = ELogger.Log)
        {
            Print(string.Format(LOGFORMAT, title, msg), logType);
        }


        public static void Log(string msg, ELogger logType = ELogger.Log)
        {
            StackFrame frame = new StackFrame(1,true);
            Log($"{Path.GetFileNameWithoutExtension(frame.GetFileName())}-----{frame.GetMethod()}", msg);
        }

        private static void Print(string msg,ELogger logType)
        {
            lock (_logLock)
            {
                switch (logType)
                {
                    case ELogger.Log: UnityEngine.Debug.Log(msg); break;
                    case ELogger.Error: UnityEngine.Debug.LogError(msg); break;
                    case ELogger.Wraing: UnityEngine.Debug.LogWarning(msg); break;
                }
            }
           
        }

        

    }

}

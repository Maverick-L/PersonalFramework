using System.Collections;
using System.Collections.Generic;
using Framework;
using System;
using Framework.FSM;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using Framework.Net.Client;

namespace Framework.Net.Client
{
    /// <summary>
    /// 协议发送标记
    /// </summary>
    public enum EFlags
    {
        eEncrypt = 1 << 0,           //数据是经过加密的
        eCompress = 1 << 1,          //数据是经过压缩的
        eContinue = 1 << 2,          //有后续数据
        eNeedAck = 1 << 3,           //数据需要确认
        eAck = 1 << 4,               //确认包
        eTestHalfBack = 1 << 3,      //测试半包
        eTestStickyBag = 1 << 4,     //测试粘包
    }
    /// <summary>
    /// 协议传入头，使用cmd+act的组合对应一个具体的协议
    /// </summary>
    public struct Head
    {
        public uint length;     //数据总长度        占位: 4字节
        public ushort error;    //错误码           占位: 2字节
        public byte cmd;        //命令-用来区别隶属                  占位：1字节
        public byte act;        //动作-用来确定具体的协议            占位：1字节
        public ushort index;    //序号            占位：2字节
        public byte flags;      //标记            占位：1字节
        public byte bcc;        //bcc             占位：1字节

        public static readonly int LENGTH = 12;
        public static readonly int MAX_SEND_SIZE = 1024 * 1024;
        public static readonly int MAX_RECIVE_SIZE = 64 * 1024;
    }

    public struct Package
    {
        public Head head;
        public int length;
        public byte[] data;
    }

    public class SocketClient : IDisposable
    {
        private bool _isdispose = false;
        protected Thread _sendThread;
        protected Thread _reciveThread;
        protected Socket _socket;
        protected EConnectState _clientState = EConnectState.DisConnected;
        protected string _ip;
        protected int _port;
        /// <summary>
        /// 链接中的异步参数
        /// </summary>
        protected IAsyncResult _connectingResult;
       // protected ManualResetEvent _sendEvent = new ManualResetEvent(false);
        protected Queue<Package> _reciveQueue;
        protected Queue<Package> _sendQueue;
        protected ushort _index;
        protected object _sendLock = new object();
        protected object _reciveLock = new object();
        public bool IsConnected { get { return _socket != null && _socket.Connected && (_clientState == EConnectState.Connected); } }
        public bool IsConnecting { get { return _clientState == EConnectState.Connecting; } }

        protected volatile bool _close =false;

        #region Thread
        public void InitThread()
        {
            _sendThread = new Thread(SendThread);
            _sendThread.IsBackground = true;

            _reciveThread = new Thread(ReciveThread);
            _reciveThread.IsBackground = true;
            _sendThread.Start();
            _reciveThread.Start();
        }

        public virtual void SendThread() { }
        public virtual void ReciveThread() { }

        #endregion

        public virtual void Connect(string ip,int port,bool nodelay = false,bool blocking = true)
        {
            throw new NotImplementedException();
        }
        public virtual void DisConnect(string error =null)
        {
            try
            {
                if (error != null)
                {
                    L.Log("Socket 错误", string.Format("ip:{0},post:{1},error:{2}", _ip, _port, error));
                }
                if (IsConnected)
                {
                    _socket.Shutdown(SocketShutdown.Both); //终止接受和发送
                }
                else if (IsConnecting)
                {
                    _socket.EndConnect(_connectingResult);
                }

                if(_socket != null)
                {
                    _socket.Close();
                    _socket = null;
                }
                _clientState = EConnectState.DisConnected;
                _index = 0;
            }
            catch
            {

            }
        }

        #region Dispose
        ~SocketClient()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool isdispose)
        {
            if (_isdispose)
            {
                return;
            }
            if (IsConnected)
            {
                DisConnect();
            }
            _close = true;
            if (_sendThread != null)
            {
                _sendThread.Join();
                _sendThread = null;
            }
            if (_reciveThread != null)
            {
                _reciveThread.Join();
                _reciveThread = null;
            }
            _isdispose = true;
        }
        #endregion
    }

}

namespace Framework.Net
{
    public enum EConnectState
    {
        /// <summary>
        /// 断链
        /// </summary>
        DisConnected,
        /// <summary>
        /// 链接
        /// </summary>
        Connected,
        /// <summary>
        /// 链接中
        /// </summary>
        Connecting,
    }

   
}




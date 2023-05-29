using Framework.Net.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
namespace Framework.Net.Server
{
    public class ServerManager
    {
        private Socket _accpetSocket;
        /// <summary>
        /// 端口号  -> Socket链接
        /// </summary>
        private Dictionary<string, Socket> _linkMap = new Dictionary<string, Socket>();
        /// <summary>
        /// 用来确认uuid -> 端口号
        /// </summary>
        private Dictionary<int, string> _uuidToPoint = new Dictionary<int, string>();

        private Dictionary<string, Thread> _threadMap = new Dictionary<string, Thread>();

        private object _linkLock = new object();
        private int uuid = 0;
        private volatile bool _close =false;
        public void Initlization()
        {
            IPAddress address = IPAddress.Parse("127.0.0.1");
            IPEndPoint point = new IPEndPoint(address, 9999);
            _accpetSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            try
            {
                _accpetSocket.Bind(point);
                _accpetSocket.Listen(1);

                Thread thread = new Thread(Accpet);
                thread.IsBackground = true;
                thread.Start(_accpetSocket);
                _threadMap.Add("accpetThread", thread);
            }
            catch (Exception ex)
            {

            }
        }

        public void OnDestory()
        {
            lock (_linkLock)
            {
                _close = true;
                var linkSocket = _linkMap.Values.GetEnumerator();
                while (linkSocket.MoveNext())
                {
                    if (linkSocket.Current.Connected)
                    {
                        linkSocket.Current.Shutdown( SocketShutdown.Both);
                        linkSocket.Current.Close();
                    }
                }
              
                if(_accpetSocket != null)
                {
                    if (_accpetSocket.Connected)
                    {
                        _accpetSocket.Shutdown(SocketShutdown.Both);
                        _accpetSocket.Close();
                    }
                }
               // _accpetSocket.Disconnect(true);
                _linkMap.Clear();
                _threadMap.Clear();
                _uuidToPoint.Clear();
                uuid = 0;
            }
        }

        /// <summary>
        /// 监听端口链接信息
        /// </summary>
        public void Accpet(Object o)
        {
            Socket socket = o as Socket;
            try
            {
                while (!_close)
                {
                    Socket accpet = socket.Accept();
                    string point = accpet.RemoteEndPoint.ToString();
                    accpet.Blocking = true;
                    UnityEngine.Debug.Log( point + "链接成功");
                    Thread thread = new Thread(ReciveFromClient);
                    thread.IsBackground = true;
                    thread.Start(point);
                    lock(_linkLock)
                    {
                        _linkMap.Add(point,accpet);
                        _uuidToPoint.Add(uuid++, point);
                        _threadMap.Add(point,thread);
                    }
                }
            }catch(Exception ex)
            {
                UnityEngine.Debug.LogError(ex.Message);
            }
        }

        public void ReciveFromClient(object point)
        {
            byte[] buffer = new Byte[Head.MAX_RECIVE_SIZE];
            Package package = new Package();
            int recive = 0;
            int state = 0;
            Socket _socket = _linkMap[(string)point];
            while (!_close)
            {
                bool IsConnected = _socket.Connected;
                //因为有半包的情况下，所以有可能一次发送的数据都无法填充一个Head
                //第一步是获取一个头大小的数据
                //第二部是通过头数据的大小获取到Package完整的大小
                SocketError error;
                //处理接受头大小的数据.
                if (IsConnected && state == 0)
                {
                    if (_socket.Blocking || _socket.Available >= Head.LENGTH - recive)
                    {
                        int ret = _socket.Receive(buffer, recive, Head.LENGTH - recive, SocketFlags.None, out error);
                        if (ret > 0)
                        {
                            recive += ret;
                            if (recive == Head.LENGTH)
                            {
                                recive = 0;
                                state = 1;
                                package = new Package();
                                unsafe
                                {
                                    fixed (byte* src = buffer)
                                    {
                                        package.head = *((Head*)src);
                                    }
                                }
                                
                            }

                        }
                    }

                }
                if (IsConnected && state == 1)
                {
                    if (_socket.Blocking || _socket.Available >= package.head.length - recive)
                    {
                        int ret = _socket.Receive(buffer, recive, (int)package.head.length - recive, SocketFlags.None, out error);
                        if (ret > 0)
                        {
                            recive += ret;
                            if (recive == package.head.length)
                            {
                                L.Log($"接受到的数据大小:{package.head.length + Head.LENGTH}");
                                recive = 0;
                                package.data = new byte[package.head.length];
                                Array.Copy(buffer, 0, package.data, 0, package.head.length);
                                Send(_socket, package);
                               
                                state = 0;

                            }
                        }

                    }
                }
                Thread.Sleep(1);
            }
        }

        public void Send(Socket _socket,Package package)
        {
            byte[] buffer = new byte[Head.LENGTH + package.head.length];
            unsafe
            {
                fixed (byte* dest = buffer)
                {
                    
                    *((Head*)dest) = package.head;
                }
                Array.Copy(package.data, 0, buffer, Head.LENGTH, package.head.length);
            }
            if ((EFlags)package.head.flags == EFlags.eTestHalfBack)
            {
                int half = (int)(package.head.length + Head.LENGTH) / 2;
                byte[] buffer1 = new byte[half];
                byte[] buffer2 = new byte[half];
                Array.Copy(buffer, 0, buffer1, 0, half);
                Array.Copy(buffer, half, buffer2, 0, (package.head.length + Head.LENGTH) - half);
                _socket.Send(buffer1);
                Thread.Sleep(10000);
                _socket.Send(buffer2);
            }
            else if ((EFlags)package.head.flags == EFlags.eTestStickyBag)
            {
                int half = (int)(package.head.length + Head.LENGTH) / 2;
                byte[] buffer1 = new byte[half + (package.head.length + Head.LENGTH)];
                byte[] buffer2 = new byte[(package.head.length + Head.LENGTH) - half];
                Array.Copy(buffer, 0, buffer1, 0, (package.head.length + Head.LENGTH));
                Array.Copy(buffer, 0, buffer1, (package.head.length + Head.LENGTH), half);
                Array.Copy(buffer, half, buffer2,0 , (package.head.length + Head.LENGTH) - half);
                _socket.Send(buffer1);
                Thread.Sleep(10000);
                _socket.Send(buffer2);
            }
            else
            {
                _socket.Send(buffer);
            }
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using Framework;
using System.Net;
using System.Net.Sockets;
using System;
using System.Threading;
namespace Framework.Net.Client
{
    public class TCPScoketClient : SocketClient
    {
        public override void Connect(string ip, int port, bool nodelay = false, bool blocking = true)
        {
            if (IsConnected)
            {
                DisConnect();
            }
            this._port = port;
            this._ip = ip;
            _sendQueue = new Queue<Package>();
            _reciveQueue = new Queue<Package>();
            //这里不判断是不是ipv6  只使用使用ipv4
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.SendBufferSize = Head.MAX_SEND_SIZE; //底层缓冲池最大发送大小
            _socket.ReceiveBufferSize = Head.MAX_RECIVE_SIZE;//底层缓冲池最大接收大小
            _socket.NoDelay = nodelay;
            _socket.Blocking = blocking;

            _connectingResult = _socket.BeginConnect(this._ip, this._port, OnConnect, _socket);
            _clientState = EConnectState.Connecting;
        }

        public void Send(byte[] data,EFlush flush)
        {
            lock (_sendLock)
            {
                Package package = new Package();
                package.data = data;
                package.head = new Head();
                package.head.length = (uint)data.Length;
                NetUtil.EFlushToFlush(ref package, flush);
                _sendQueue.Enqueue(package);
               // _sendEvent.Set();
            }
        }

        public void Send(byte[] data,EFlush flush,EFlags flag)
        {
            lock (_sendLock)
            {
                Package package = new Package();
                package.data = data;
                package.head = new Head();
                package.head.length = (uint)data.Length;
                NetUtil.EFlushToFlush(ref package, flush);
                package.head.flags = (byte)flag;
                _sendQueue.Enqueue(package);
                // _sendEvent.Set();
            }
        }

        /// <summary>
        /// 为了防止有主线程回调，和开启一个新的线程接受数据，这里不直接处理
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <returns></returns>
        public void Recive(List<Package> allRecive)
        {
            lock (_reciveLock)
            {
                while(_reciveQueue.Count != 0)
                {
                    allRecive.Add(_reciveQueue.Dequeue());
                }
            }
        }

        public override void SendThread()
        {
            Package sendPackage = new Package();
            bool isSending = false;
            int sendResule = 0;
            byte[] sendBuffer = new byte[0];
            while (!_close)
            {
                if (IsConnected && _socket.Blocking)
                {
                    //中断，等待Set
                  //  _sendEvent.WaitOne();
                }
                if (IsConnected)
                {
                    if (!isSending && _sendQueue.Count > 0)
                    {
                        lock (_sendLock)
                        {
                            sendPackage = _sendQueue.Dequeue();
                            NetUtil.Encode(ref sendPackage);
                            sendBuffer = new byte[sendPackage.head.length + Head.LENGTH];
                            unsafe
                            {
                                fixed (byte* dest = sendBuffer)
                                {
                                    sendPackage.head.index = _index++;
                                    *((Head*)dest) = sendPackage.head;
                                }
                                Array.Copy(sendPackage.data, 0, sendBuffer, Head.LENGTH, sendPackage.head.length);
                            }
                        }
                        isSending = true;
                        sendResule = 0;
                    }
                    else
                    {
                        //当当前没有需要上传的内容，而且队列中也没有内容的时候回复中断状态
                        if (!isSending && _socket.Blocking) 
                        {
                          //  _sendEvent.Reset();
                        }
                    }
                }
                if(IsConnected && isSending)
                {
                    try
                    {
                        SocketError error;
                        int ret = _socket.Send(sendBuffer, sendResule, sendBuffer.Length, SocketFlags.None, out error);
                        if(ret > 0)
                        {
                            sendResule += ret;
                            if(sendResule == sendBuffer.Length)
                            {
                                isSending = false;
                            }
                            L.Log("Client Send :buffer:" + ret, ELogger.Log);
                        }
                        else if ((_socket != null && _socket.Blocking) || error != SocketError.WouldBlock)
                        {
                            DisConnect(error.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        DisConnect(ex.ToString());
                    }
                }
                Thread.Sleep(1);
            }
        }

        public override void ReciveThread()
        {
            byte[] buffer = new Byte[Head.MAX_RECIVE_SIZE];
            Package package = new Package();
            int recive = 0;
            int state = 0;
            while (!_close)
            {
                //因为有半包的情况下，所以有可能一次发送的数据都无法填充一个Head
                //第一步是获取一个头大小的数据
                //第二部是通过头数据的大小获取到Package完整的大小
                SocketError error;
                //处理接受头大小的数据.
                if(IsConnected && state == 0)
                {
                    if (_socket.Blocking || _socket.Available >= Head.LENGTH - recive)
                    {
                        int ret = _socket.Receive(buffer, recive, Head.LENGTH - recive, SocketFlags.None, out error);
                        if (ret > 0)
                        {
                            recive += ret;
                            L.Log($"客户端接受到的头数据包大小{recive}");
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
                                if (package.head.length == 0)
                                {
                                    //这个数据包完全接受成功，无数据数据包
                                    package.data = null;
                                    package.length = 0;
                                    state = 0;
                                    lock (_reciveLock)
                                    {
                                        var decodeState = NetUtil.Decode(ref package);
                                        if (decodeState == EDecodeState.Success)
                                        {
                                            _reciveQueue.Enqueue(package);
                                        }
                                        else if (decodeState == EDecodeState.Fail)
                                        {
                                            throw new Exception("解码错误");
                                        }
                                    }
                                }
                            }
                        }
                        //如果在不是阻塞状态，或者报错不是等待阻塞则直接退出链接
                        else if ((_socket != null && _socket.Blocking) || error != SocketError.WouldBlock)
                        {
                            DisConnect(error.ToString());
                        }
                    }

                }
                if(IsConnected && state == 1)
                {
                    if(_socket.Blocking || _socket.Available >= package.head.length - recive)
                    {
                        int ret = _socket.Receive(buffer, recive, (int)package.head.length - recive, SocketFlags.None, out error);
                        if(ret > 0)
                        {
                            recive += ret;
                            L.Log($"client接受到数据长度:{recive}");
                            if (recive == package.head.length)
                            {
                                recive = 0;
                                package.data = new byte[package.head.length];
                                Array.Copy(buffer, 0, package.data, 0, package.head.length);

                                var decodeState = NetUtil.Decode(ref package);
                                if (decodeState == EDecodeState.Success)
                                {
                                    lock (_reciveLock)
                                    {
                                        _reciveQueue.Enqueue(package);
                                        L.Log($"Client Recive Finsh Count:{package.head.length + Head.LENGTH}");
                                    }
                                }
                                else if (decodeState == EDecodeState.Fail)
                                {
                                    throw new Exception("解码错误");
                                }
                                state = 0;
                               
                            }
                        }
                        else if((_socket!=null && _socket.Blocking) || error != SocketError.WouldBlock)
                        {
                            DisConnect(error.ToString());
                        }
                            
                    }
                }
                


                Thread.Sleep(1);
            }
        }

        private void OnConnect(IAsyncResult iar)
        {
            var socket = iar.AsyncState as Socket;
            try
            {
                socket.EndConnect(iar);
                _connectingResult = null;
                _clientState = EConnectState.Connected;
                L.Log($"连接成功：{socket.RemoteEndPoint.ToString()}",ELogger.Log);
            }
            catch (System.Exception e)
            {
                L.Log("TCPScoketClient", e.Message);
                _socket = null;
                _clientState = EConnectState.DisConnected;
            }
        }
    }
}
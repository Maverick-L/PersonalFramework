using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System;
using UnityEngine;
using Framework.Message;
namespace Framework.Net.Client
{
    public class MessageNetAttrubity : MessageAttrubity
    {
        public MessageNetAttrubity(Net.EFlush messageType)
        {
            this.messageType = messageType;
        }
    }

    public class NetManager :MonoBehaviour
    {
        private static NetManager _instance;
        public static NetManager getInstance => _instance;

        private List<TCPScoketClient> _tcpMap = new List<TCPScoketClient>();
        private void Awake()
        {
            _instance = this;
        }

        public TCPScoketClient GetTCPSocketClient()
        {
            TCPScoketClient client = new TCPScoketClient();
            _tcpMap.Add(client);
            return client;
        }

        private void Update()
        {
            for(int i = 0; i < _tcpMap.Count; i++)
            {
                var client = _tcpMap[i];
                if (client.IsConnected)
                {
                    List<Package> s2cList = new List<Package>();
                    client.Recive(s2cList);
                    for(int j = 0; j < s2cList.Count; j++)
                    {
                        var flush = NetUtil.FlushToEFlush(s2cList[j]);
                        NetUtil.SerializeAndSendMessage(s2cList[j].data, flush);
                    }
                }
            }
        }

        private void OnDestroy()
        {
            for(int i = 0; i < _tcpMap.Count; i++)
            {
                _tcpMap[i].Dispose();
            }
            _tcpMap.Clear();
        }
    }
}


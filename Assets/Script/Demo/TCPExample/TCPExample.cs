using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;
using Framework.Net;
using Framework.Net.Client;
using UnityEngine.UI;
using Framework.Message;
using System.Net.Sockets;

public class TCPExample : MonoBehaviour,IMessage
{
    public Text sendText;
    public Text ReciveText;
    public Button clickSend;
    public InputField sendInput;
    public InputField sendCount;
    public Button clickLink;
    public InputField linkAddress;
    public InputField linkPort;
    public Toggle nodely;
    public Toggle blocking;
    public Toggle testHalfPackage;
    public Toggle testStickyBag;

    private TCPScoketClient _tcp;
    private Framework.Net.Server.ServerManager _serverManager = new Framework.Net.Server.ServerManager();
    private void Awake()
    {
        MessageManager.instance.Regist(this);
        _serverManager.Initlization();

        clickSend.onClick.AddListener(ClickSend);
        clickLink.onClick.AddListener(ClickLink);
        linkAddress.text = "127.0.0.1";
        linkPort.text = "9999";
        sendCount.text = "1";
    }

    private void Start()
    {
        _tcp = NetManager.getInstance.GetTCPSocketClient();
        _tcp.InitThread();
    }

    private void OnDestroy()
    {
       _serverManager.OnDestory();
        _tcp.Dispose();
        MessageManager.instance.UnRegist(this);
    }
    private void ClickSend()
    {
        if (string.IsNullOrEmpty(sendInput.text))
        {
            return;
        }
        if (string.IsNullOrEmpty(sendCount.text))
        {
            return;
        }
        if(!testHalfPackage.isOn && !testStickyBag.isOn)
        {
            for (int i = 0; i < int.Parse(sendCount.text); i++)
            {
                TestC2S testC2S = new TestC2S() { Index = i, Val = sendInput.text };
                byte[] net = NetUtil.UnSerialize<TestC2S>(testC2S);
                _tcp.Send(net, Framework.Net.EFlush.TestC2S);
                sendText.text += $"\nindex:{testC2S.Index}--------val:{testC2S.Val}";
            }
        }
        if (testHalfPackage.isOn)
        {
            TestC2S testC2S = new TestC2S() { Index = 10000, Val = "这里是半包测试内容" };
            byte[] net = NetUtil.UnSerialize<TestC2S>(testC2S);
            _tcp.Send(net, Framework.Net.EFlush.TestC2S, EFlags.eTestHalfBack);
            sendText.text += $"\nindex:{testC2S.Index}--------val:{testC2S.Val}";
        }
        if (testStickyBag.isOn)
        {
            TestC2S testC2S = new TestC2S() { Index = 100000, Val = "这里是粘包测试测试内容1" };
            byte[] net = NetUtil.UnSerialize<TestC2S>(testC2S);
            _tcp.Send(net, Framework.Net.EFlush.TestC2S, EFlags.eTestStickyBag);
            sendText.text += $"\nindex:{testC2S.Index}--------val:{testC2S.Val}";
        }

    }

    private void ClickLink()
    {
        if (string.IsNullOrEmpty(linkAddress.text))
        {
            return;
        }
        if (string.IsNullOrEmpty(linkPort.text))
        {
            return;
        }

        if (_tcp.IsConnected)
        {
            _tcp.DisConnect();
        }

        _tcp.Connect(linkAddress.text,int.Parse(linkPort.text),nodely.isOn,blocking.isOn);
    }

    [MessageNetAttrubity(Framework.Net.EFlush.TestC2S)]
    public void TestS2CMessage(params object[] data)
    {
        TestC2S value = data[0] as TestC2S;
        ReciveText.text += $"\nindex:{value.Index}-------val:{value.Val}";
    }
}


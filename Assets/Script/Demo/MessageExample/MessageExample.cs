using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Message;
using Framework;

namespace Framework.Message.Example
{
    public class MessageBaseWithMonoBehaviour : UnityEngine.MonoBehaviour, IMessage
    {
        protected virtual void OnEnable()
        {
            MessageManager.instance.Regist(this);
        }

        protected virtual void OnDisable()
        {
            MessageManager.instance.UnRegist(this);
        }

        protected virtual void OnDestory()
        {
            MessageManager.instance.ClearCache(this);
        }
    }

    public class MessageExample : MessageBaseWithMonoBehaviour
    {

        public bool sendMessage = false;
        public bool createOrRemove = false;
        public MessageExampleType sendType = MessageExampleType.Test1;
        public int sendTypeMap;
        public int createCount = 0;
        public int removeCount = 0;
        private List<MessageExampleNoMono> _map = new List<MessageExampleNoMono>();
        private void Update()
        {
            if (createOrRemove)
            {
                if (createCount != 0)
                {
                    var startTime = System.DateTime.Now;

                    for (int i = 0; i < createCount; i++)
                    {
                        _map.Add(new MessageExampleNoMono());
                    }
                    createCount = 0;
                    var entTime = System.DateTime.Now;
                    L.Log((entTime - startTime).TotalSeconds.ToString());
                }
                if (removeCount != 0)
                {
                    for (int i = _map.Count - 1; i > 0; i--)
                    {
                        MessageExampleNoMono a = _map[i];
                        a.Dispose();
                        _map.RemoveAt(i);
                    }
                    removeCount = 0;
                }
            }

            if (sendMessage)
            {
                MessageManager.instance.Send(sendType, "11");
                if (sendTypeMap != 0)
                {
                    for (int i = 1; i <= (int)MessageExampleType.Test4; i = i << 1)
                    {
                        if ((i & sendTypeMap) == i)
                        {
                            MessageManager.instance.Send((MessageExampleType)i, 22);
                        }
                    }
                    sendTypeMap = 0;
                }
                sendMessage = false;
            }

        }


        [MessageExampleAttribute(MessageExampleType.Test1)]
        public void Test1(params object[] data)
        {
            global::Framework.L.Log("Test1被调用");
        }

        [MessageExampleAttribute(MessageExampleType.Test2)]
        public void Test2(params object[] data)
        {
            Framework.L.Log($"Test2被调用{data[0]}");
        }

    }

    public class MessageExampleNoMono : MessageBase
    {
        [MessageExampleAttribute(MessageExampleType.Test3)]
        public void Test3(params object[] data)
        {
            global::Framework.L.Log("Test3被调用");

        }
        [MessageExampleAttribute(MessageExampleType.Test4)]

        public void Test4(params object[] data)
        {
            global::Framework.L.Log("Test4被调用");

        }

        [MessageExample(MessageExampleType.Test5)]
        public void Test5(params object[] data)
        {

        }


    }

    public enum MessageExampleType
    {
        Test1 = 1,
        Test2 = 2,
        Test3 = 4,
        Test4 = 8,
        Test5 = 16,  //性能自定
    }

    public class MessageExampleAttribute : MessageAttrubity
    {
        public MessageExampleAttribute(MessageExampleType exampleType)
        {
            messageType = exampleType;
        }
    }
}
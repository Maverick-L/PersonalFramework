using System.Collections;
using System.Collections.Generic;
using System;

namespace Framework.Message{

    
    public class MessageManager
    {
        private class MessageCacheData
        {
            public Dictionary<Enum,MessageCallback> map;
        }

        private class MessageCallbackEvent
        {
            public event MessageCallback callbackEvent;

            public void Call(params object[] data)
            {
                callbackEvent?.Invoke(data);
            }
        }

        private static MessageManager _instence;
        public static MessageManager instance {
            get { if (_instence == null)
                {

                    _instence = new MessageManager();
                }
                return _instence;
            }
        }
        public delegate void MessageCallback(params object[]value);
        public event MessageCallback callback;
        private Dictionary<Enum, MessageCallbackEvent> _messageMap;
        private Dictionary<IMessage, MessageCacheData> _cache;

        public MessageManager()
        {
            _cache = new Dictionary<IMessage, MessageCacheData>();
            _messageMap = new Dictionary<Enum, MessageCallbackEvent>();
        }
        /// <summary>
        /// 注册方案，采用反射获取到所有的拥有MessageAttrubity特性的方法，将其生成Delegate的回调
        /// </summary>
        /// <param name="message"></param>
        public void Regist(IMessage message)
        {
            Dictionary<Enum, MessageCallback> map;
            if (_cache.ContainsKey(message))
            {
                map = _cache[message].map;
            }
            else
            {
                map = new Dictionary<Enum, MessageCallback>();
                var classType = message.GetType();
                var methodes = classType.GetMethods();
                foreach (var method in methodes)
                {
                    var allAttrubity = method.GetCustomAttributes(typeof(MessageAttrubity), false);
                    if (allAttrubity.Length > 0)
                    {
                        MessageAttrubity att = allAttrubity[0] as MessageAttrubity;
                        MessageCallback callback = Delegate.CreateDelegate(typeof(MessageCallback), message, method.Name, false, false) as MessageCallback;
                        map.Add(att.messageType, callback);
                    }
                }
                _cache.Add(message, new MessageCacheData() { map = map });
            }
            foreach(var data in map)
            {
                if (!_messageMap.ContainsKey(data.Key))
                {
                    _messageMap.Add(data.Key, new MessageCallbackEvent());
                }
                _messageMap[data.Key].callbackEvent += data.Value;
            }
        }

        /// <summary>
        /// 注册方案2：不需要继承IMessage ,使用回调的方式注册，同时，取消注册的时候也需要调用UnRegist<TMessage>，无法自动取消注册
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="messageType"></param>
        /// <param name="callbackFunc"></param>
        public void Regist<TMessage>(TMessage messageType , MessageCallback callbackFunc) where TMessage :Enum
        {
            if (!_messageMap.ContainsKey(messageType))
            {
                _messageMap.Add(messageType, new MessageCallbackEvent());
            }
            _messageMap[messageType].callbackEvent += callbackFunc;
        }

        public void UnRegist(IMessage message)
        {
            
            if (_cache.ContainsKey(message))
            {
                foreach (var data in _cache[message].map)
                {
                    _messageMap[data.Key].callbackEvent -= data.Value;
                }
            }
        }

        public void UnRegist<TMessage>(TMessage messageType,MessageCallback callbackFunc) where TMessage : Enum
        {
            if (_messageMap.ContainsKey(messageType))
            {
                _messageMap[messageType].callbackEvent -= callbackFunc;
            }
        }

        public void ClearCache(IMessage message)
        {
            UnRegist(message);
            _cache.Remove(message);
        }

        public void Send(Enum enumType,params object[] data)
        {
            if (_messageMap.ContainsKey(enumType))
            {
                _messageMap[enumType].Call(data);
            }
        }
    }

}
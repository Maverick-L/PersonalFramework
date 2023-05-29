using System.Collections;
using System.Collections.Generic;
using System;

namespace Framework.Message
{
    public interface IMessage
    {
       
    }

    public class MessageBase : IDisposable , IMessage
    {
        protected bool _isdispose = false;

        public MessageBase()
        {
            MessageManager.instance.Regist(this);
        }
        ~MessageBase()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
        }

        protected void Dispose(bool isdispose)
        {
            if (_isdispose)
            {
                return;
            }
            MessageManager.instance.ClearCache(this);
            _isdispose = true;
        }
    }


    [AttributeUsage(AttributeTargets.Method)]
    public class MessageAttrubity : Attribute
    {
        public Enum messageType;
        public MessageAttrubity() { }
    }

}
using System.Collections;
using System.Collections.Generic;
using System;
using Framework.Net.Client;
using System.Reflection;
using Google.Protobuf;
namespace Framework.Net
{


    public enum EDecodeState
    {
        Success,//成功
        Fail,//失败
        Ignore,//忽略
    }

    public class NetUtil
    {

        public static EDecodeState Decode(ref Package package)
        {
            return EDecodeState.Success;
        }

        public static void Encode(ref Package package)
        {
        }

        public static void EFlushToFlush(ref Package package, EFlush flush)
        {
            package.head.act = (byte)((int)flush / (1 << 8));
            package.head.cmd = (byte)((int)flush % (1 << 8));
        }

        public static EFlush FlushToEFlush(Package package)
        {
            return (EFlush)(package.head.act * (1 << 8) + package.head.cmd);
        }

        public static void SerializeAndSendMessage(byte[] buffer, EFlush flush)
        {
            IMessage instance = Serialize(buffer, flush);
            if(instance != null)
            {
                Framework.Message.MessageManager.instance.Send(flush, instance);

            }
        }
        
        public static IMessage Serialize(byte[] buffer, EFlush flush)
        {
            try
            {
                string messageName = flush.ToString();
                Type message = Assembly.GetExecutingAssembly().GetType(messageName);
                IMessage instance = Activator.CreateInstance(message) as IMessage;
                Serialize(instance, buffer);
                return instance;
            }
            catch (Exception ex)
            {
                L.Log($"EFlush-->[{flush.ToString()}]Serialize失败----{ex.Message}");
            }
            return null;
        }

        public static void Serialize<T>(T data, byte[] buffer) where T : Google.Protobuf.IMessage
        {
            CodedInputStream input = new CodedInputStream(buffer);
            data.MergeFrom(input);
        }

        public static byte[] UnSerialize<T>(T c2s) where T : Google.Protobuf.IMessage
        {
            byte[] buffer = new byte[c2s.CalculateSize()];
            CodedOutputStream outPutStream = new CodedOutputStream(buffer);
            c2s.WriteTo(outPutStream);
            return buffer;
        }
    }
}

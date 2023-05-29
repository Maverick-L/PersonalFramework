using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Framework.Message;
namespace Framework.MVC
{

    /// <summary>
    /// 继承当前接口的Model会被ModelManager获取，只有Struct类型可以继承
    /// </summary>
    public interface IBaseModelStruct 
    {
         void SendMessage();
    }


    public class ModelManager : MessageBase
    {
        //实现纯数据的Model，采用ECS中Commponent的思想
        private  Dictionary<Type, long> _modelMemoryIndex;
        private  byte[] _modelMemory;

        public  void OnInitModel()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            var allType = assembly.GetTypes();
            _modelMemoryIndex = new Dictionary<Type, long>();
            long memory = 0;
            for (int i = 0; i < allType.Length; i++)
            {
                if (CheckType(allType[i]))
                {
                    _modelMemoryIndex.Add(allType[i], memory);
                    memory += Marshal.SizeOf(allType[i]);
                }
            }
            _modelMemory = new byte[memory];
        }

        public void OnDestoryModel()
        {
            _modelMemory = null;
            _modelMemoryIndex?.Clear();
            Dispose();
            GC.Collect();
        }

        /// <summary>
        /// 值类型获取，不会自动存储，需要配合Set方法使用
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <returns></returns>
        public  TModel Get<TModel>() where TModel : IBaseModelStruct
        {
            if (!CheckType(typeof(TModel)))
            {
                throw new Exception("GetModel错误---"+typeof(TModel).Name);
            }

            IntPtr ptr = GetModelMemoryPtr<TModel>();
            TModel model = Marshal.PtrToStructure<TModel>(ptr);
            return model;
        }

        public  void Set<TModel>(TModel value) where TModel : IBaseModelStruct
        {
            if (!CheckType(typeof(TModel)))
            {
                throw new Exception("SetModel错误");
            }
            IntPtr ptr = GetModelMemoryPtr<TModel>();
            Marshal.StructureToPtr<TModel>(value, ptr, true);
            value.SendMessage();
        }

        public void Set(Type type, IBaseModelStruct value)
        {
            if (!CheckType(type))
            {
                throw new Exception("SetModel错误");
            }
            IntPtr ptr = GetModelMemoryPtr(type);
            Marshal.StructureToPtr(value, ptr, true);
            value.SendMessage();
        }
        /// <summary>
        /// 通过消息进行修改
        /// </summary>
        /// <param name="data"></param>
        [MVCMessageAttrubite(EMVCMessageEnum.ModeifyModelMessage)]
        public void ModeifyModelMessage(params object[] data)
        {
            Set(data[0] as Type, data[1] as IBaseModelStruct);
        }

        private IntPtr GetModelMemoryPtr(Type type)
        {
            if(_modelMemory == null)
            {
                throw new Exception("Model No Init");
            }

            IntPtr ptr;
            unsafe
            {
                fixed (byte* start = &_modelMemory[_modelMemoryIndex[type]])
                {
                    ptr = (IntPtr)start;
                }
            }
            return ptr;
        }

        private IntPtr GetModelMemoryPtr<TModel>() {

            return GetModelMemoryPtr(typeof(TModel));
        }

        private bool CheckType(Type t)
        {
            return t != typeof(IBaseModelStruct) && t.GetInterface("IBaseModelStruct") != null && t.IsValueType;
        }
    }
}
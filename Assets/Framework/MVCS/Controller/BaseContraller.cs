using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Message;
using System.Reflection;
using System;

namespace Framework.MVC
{
    public abstract class BaseContraller : MessageBase
    {
        /// <summary>
        /// 常驻Control
        /// </summary>
        private static Dictionary<int, BaseContraller> _resident;

        public static TContraller Get<TContraller>() where TContraller :BaseContraller ,new()
        {
            var att = typeof(TContraller).GetCustomAttribute<ResidentContorAttribute>();
            if(att != null)
            {
                return _resident[att.GetHashCode()] as TContraller;
            }
            TContraller contraller = new TContraller();
            contraller.OnInitlization();
            return contraller;

        }

        public static void OnInitContraller()
       {
            var attrutes = Assembly.GetExecutingAssembly().GetCustomAttributes<ResidentContorAttribute>().GetEnumerator();
            while (attrutes.MoveNext())
            {
                var att = attrutes.Current;
                if(att.GetType().BaseType == typeof(BaseContraller))
                {
                    _resident.Add(att.GetHashCode(), Activator.CreateInstance(att.GetType()) as BaseContraller);
                }
            }

        }

        public static void OnDestroy()
        {
            _resident.Clear();
        }

        /// <summary>
        /// 修改对应的Model参数
        /// </summary>
        protected  void ModifyModelData<TModel>(TModel model) where TModel :IBaseModelStruct
        {
            ModelManager.instance.Set<TModel>(model);
        }

        protected  TModel GetModelData<TModel>() where TModel : IBaseModelStruct
        {
            return ModelManager.instance.Get<TModel>();
        }

        public abstract void OnInitlization();

        public virtual void OnDestory() {
            Dispose();
        }

    }
}


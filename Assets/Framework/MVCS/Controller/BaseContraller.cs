using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Message;
namespace Framework.MVC
{
    public abstract class BaseContraller : MessageBase
    {
        public static TContraller Get<TContraller>() where TContraller :BaseContraller ,new()
        {
            TContraller contraller = new TContraller();
            contraller.OnInitlization();
            return contraller;

        }

        /// <summary>
        /// 修改对应的Model参数
        /// </summary>
        protected static void ModifyModelData<TModel>(TModel model) where TModel :IBaseModelStruct
        {
            ModelManager.instance.Set<TModel>(model);
        }

        protected static TModel GetModelData<TModel>() where TModel : IBaseModelStruct
        {
            return ModelManager.instance.Get<TModel>();
        }

        public abstract void OnInitlization();

        public virtual void OnDestory() {
            Dispose();
        }

    }
}


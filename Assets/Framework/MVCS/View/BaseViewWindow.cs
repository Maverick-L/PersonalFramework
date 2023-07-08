using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.MVC
{
    [RequireComponent(typeof(WindowConfigMono))]
    public class BaseViewWindow : MonoBehaviour
    {
        protected WindowConfigMono _windowConf;
        public int viewID { internal set; get; }
        public BaseViewMediator mediator { get; internal set; }

        public WindowConfig viewConfig
        {
            get
            {
                if(_windowConf == null)
                {
                    _windowConf = gameObject.GetComponent<WindowConfigMono>();
                }
        
                return _windowConf.viewConfig;
            } 
           
        }

        /// <summary>
        /// 反射调用MonoBehaviour中的方法
        /// </summary>
        /// <param name="findPath">查询目标的路径0</param>
        /// <param name="monoClass">继承MonoBehaviour的类名,或者mono的类名</param>
        /// <param name="methodName">方法的名字</param>
        /// <param name="value">参数</param>
        public void ReflectCallMonoFunc(string findPath,string monoClass,string methodName,params object[] value)
        {
            Transform target = string.IsNullOrEmpty(findPath) ? transform : transform.Find(findPath);
            Component component = target.GetComponent(monoClass);
            component.GetType().GetMethod(methodName).Invoke(null, value);
        }

        #region Unity
        public virtual void Awake()
        {

        }

        public virtual void Start()
        {

        }

        public virtual void OnEnable()
        {

        }

        public virtual void OnDisable()
        {

        }

        public virtual void OnDestroy()
        {

        }

        public virtual void Update()
        {

        }
        #endregion
    }
}
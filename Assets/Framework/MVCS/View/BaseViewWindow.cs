using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.MVC
{
    [RequireComponent(typeof(WindowConfigMono))]
    public class BaseViewWindow : MonoBehaviour
    {
        protected int _viewID;
        protected WindowConfigMono _windowConf;
        public int setViewID { set { _viewID = value; } }
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
        public int viewID => _viewID;
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
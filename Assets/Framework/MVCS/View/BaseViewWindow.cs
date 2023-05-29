using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.MVC
{
    [System.Serializable]
    public struct WindowConfig
    {
        public EWindow window;
        public EWindow parentWindow;
        public EWindowType windowType;
        public string viewMediatorName;
        public int fixedOrderLayer;
        public bool cache;
        public int viewID;
        public AnimationClip openAnimation;
        public AnimationClip loopAnimation;
        public AnimationClip closeAnimation;
        public BaseViewMediator viewMediator;
    }
    public class BaseViewWindow : MonoBehaviour
    {
        [SerializeField]
        private WindowConfig _viewConfig;
        public WindowConfig viewConfig => _viewConfig;

        public BaseViewMediator setMediator { set { _viewConfig.viewMediator = value; } }
        public int setViewID { set { _viewConfig.viewID = value; } }
    }
}
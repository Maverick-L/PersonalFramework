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
        public AnimationClip openAnimation;
        public AnimationClip loopAnimation;
        public AnimationClip closeAnimation;
    }

    public sealed class WindowConfigMono : MonoBehaviour
    {
        [SerializeField]
        private WindowConfig _viewConfig;
        public WindowConfig viewConfig => _viewConfig;

    }
}
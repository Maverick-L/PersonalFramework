using UnityEngine;
using System.Collections;

namespace Framework
{
    public class WindowAnimationBehaviour : MonoBehaviour
    {
        public virtual void OnOpen(System.Action callback)
        {
            callback();
        }

        public virtual void OnClose(System.Action callback)
        {
            callback();
        }
    }
}

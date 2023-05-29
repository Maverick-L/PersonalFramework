using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sword.Resource
{
    public class ABInspector : MonoBehaviour
    {
        public string targetABName = null;
        public AssetBundle target = null;

        private void OnGUI()
        {
            if (GUI.Button(new Rect(0, 0, 100, 100), "Load Target AB"))
            {
                if (target != null)
                {
                    target.Unload(true);
                }
                if (!string.IsNullOrEmpty(targetABName))
                {
                  // target = AssetBundle.LoadFromFile(ResourceUtil.ABResourcePath) + "/" + targetABName);
                }
            }
        }
    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    [ExecuteInEditMode]
    public class RaycastFilter : MaskableGraphic
    {


#if UNITY_EDITOR
        static Vector2[] s_showRect = new Vector2[]
        {
            new Vector2(-0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(-0.5f, -0.5f),
            new Vector2(0.5f, -0.5f),
        };
        static List<Color> s_showColor = new List<Color>
        {
            new Color(0, 1, 0, 0.5f),
            new Color(1, 0, 0, 0.5f),
        };

        [SerializeField]
        protected int _colorType = 0;
        public int ColorType
        {
            get { return _colorType; }
        }

        public void SetColor(int colorType)
        {
            useLegacyMeshGeneration = false;
            _colorType = colorType;

            if (colorType != 1)
            {
                Debug.LogError(colorType);
            }
        }

        static public void AddToAllChild(Transform main)
        {
            List<Selectable> buttons = new List<Selectable>();
            buttons.AddRange(main.GetComponentsInChildren<Selectable>(true));

            System.Type[] addComponents = new System.Type[]
            {
                    typeof(RectTransform),
                    typeof(RaycastFilter),
            };

            RectTransform cloneObj;
            RaycastFilter temp;
            MaskableGraphic[] masks;

            foreach (var item in buttons)
            {
                masks = item.gameObject.GetComponentsInChildren<MaskableGraphic>(true);

                foreach (MaskableGraphic target in masks)
                {
                    if (target.gameObject.tag == "ShowBlock")
                        continue;
                    target.gameObject.tag = "ShowBlock";

                    Debug.Log(target.name);

                    if (target.raycastTarget && !(target is RaycastFilter))
                    {
                        cloneObj = (new GameObject("ShowBlock", addComponents)).GetComponent<RectTransform>();
                        cloneObj.SetParent(target.transform);
                        cloneObj.anchoredPosition3D = Vector3.zero;
                        cloneObj.anchorMin = Vector2.zero;
                        cloneObj.anchorMax = Vector2.one;
                        cloneObj.sizeDelta = Vector2.zero;
                        cloneObj.localScale = Vector2.one;
                        LayoutRebuilder.MarkLayoutForRebuild(cloneObj);
                        temp = cloneObj.gameObject.GetComponent<RaycastFilter>();
                        temp.SetColor(1);
                        temp.raycastTarget = false;
                    }
                }
            }
        }
#endif

        protected RaycastFilter()
        {
            useLegacyMeshGeneration = false;
        }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            toFill.Clear();
        }
    }
}
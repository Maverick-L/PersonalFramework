using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 后期优化模式：采用Manager控制同一个摄像机，只有一个Canvas组件
/// </summary>
public class HUDController : MonoBehaviour, IControl
{
    private static readonly string HUD_ROOT_NAME = "HUDController";
    private static  Transform s_hudRoot;
    public static  Transform get_HUD_ROOT { get { if (s_hudRoot) return s_hudRoot; return s_hudRoot = new GameObject(HUD_ROOT_NAME).transform; } }

    public GameObject target;
    /// <summary>
    /// 负责渲染target的摄像机;
    /// </summary>
    public Camera renderCamera;
    private Dictionary<string,Transform> _hudTarget;

    public void OnInitlization()
    {
        if(target == null)
        {
            return;
        }
        transform.parent = get_HUD_ROOT;
        Canvas canvas = transform.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = renderCamera;
        canvas.sortingOrder =1;
        canvas.planeDistance = 38;

        //初始化完成之后先更新一次位置
        OnRenderUpdate(); 
    }

    /// <summary>
    /// 初始化HUD设置
    /// </summary>
    /// <param name="target">跟随目标</param>
    /// <param name="render">渲染摄像机</param>
    /// <param name="myHUDRootPath">此物体下面的那个root节点跟随位置</param>
    public void OnInitData(GameObject target,Camera render,string tranName,params string[] myHUDRootPath)
    {
        this.target = target;
        this.renderCamera = render;
        this._hudTarget =new Dictionary<string, Transform>();
        for (int i = 0; i < myHUDRootPath.Length; i++)
        {
            var hud = transform.Find(myHUDRootPath[i]);
            if (hud)
            {
                this._hudTarget.Add(myHUDRootPath[i],hud);
            }
        }
        transform.name = target.name + tranName + "_HUD";

    }


    public void OnLogicUpdate()
    {
    }

    public void OnRenderUpdate()
    {
        if (_hudTarget == null)
        {
            return;
        }
        var screenPos = RectTransformUtility.WorldToScreenPoint(renderCamera, target.transform.position);
        Vector3 rectPos;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(transform as RectTransform,screenPos,renderCamera,out rectPos))
        {
            if (!gameObject.activeSelf) gameObject.SetActive(true);
            foreach(var trans in _hudTarget.Values)
            {
                trans.position = rectPos;
            }
        }
        else
        {
            gameObject.SetActive(false);
        }
    }



    public void OnDestoryControl()
    {
    }


    private void OnDestroy()
    {
        OnDestoryControl();
    }

    #region UI修改
    public void SetText(string path,string str ,string hudTargetPath = "")
    {
        Text text;
        if(GetControl<Text>(path,hudTargetPath,out text))
        {
            text.text = str;
        }
    }

    public void SetActive(string path,bool enable, string hudTargetPath = "")
    {
        Transform trans;
        if(GetControl<Transform>(path, hudTargetPath, out trans))
        {
            trans.gameObject.SetActive(enable);
        }
    }

    public void SetRotation(string path,Vector3 eulers,string hudTargetPath = "")
    {
        Transform trans;
        if (GetControl<Transform>(path, hudTargetPath, out trans))
        {
            trans.rotation = Quaternion.Euler(eulers);
        }
    }

    private bool GetControl<T>(string path, string hudTargetPath,out T target) where T : Component
    {
        target = null;
        if (!string.IsNullOrEmpty(hudTargetPath))
        {
            if (_hudTarget.ContainsKey(hudTargetPath))
            {
                target = _hudTarget[hudTargetPath].Find(path).GetComponent<T>();
            }
            else
            {
                return false;
            }
        }
        else
        {
            foreach (var root in _hudTarget.Values)
            {
                var go = root.Find(path);
                if (go != null)
                {
                    target = go.GetComponent<T>();
                }
            }
        }
        return target != null;
      
    }
    #endregion
}

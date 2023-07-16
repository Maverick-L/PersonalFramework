using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Framework.MVC
{
    /// <summary>
    /// ViewManagerLoader
    /// </summary>
    public partial class ViewManager
    {
        /// <summary>
        /// 加载完成的窗口，未实例化的GameObject
        /// </summary>
        private Dictionary<EWindow, AssetLoader<GameObject>> _windowObj = new Dictionary<EWindow, AssetLoader<GameObject>>();

        private Dictionary<EWindow, int> _windowObjRefreshCount = new Dictionary<EWindow, int>();

        /// <summary>
        /// 加载预制件资源
        /// </summary>
        /// <param name="window"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private IEnumerator CoLoadWindow(WindowItem item)
        {
            GameObject obj;
            if (_windowObj.ContainsKey(item.eWindow))
            {
                obj = _windowObj[item.eWindow].Current;
            }
            else
            {
                //加载界面资源
                string path = "Assets/" + MVCUtil.PREFAB_PATH + "/" + item.eWindow.ToString() + ".prefab";
                AssetLoader<GameObject> loader = new AssetLoader<GameObject>(path, true);
                while(loader.finsh)
                {
                    yield return null;
                }
                obj = loader.Current;
                _windowObj.Add(item.eWindow, loader);
                _windowObjRefreshCount.Add(item.eWindow, 0);
            }
            //加载完成
            GameObject go = GameObject.Instantiate<GameObject>(obj);
            go.gameObject.SetActive(false);
            Canvas canvas = go.GetComponent<Canvas>();
            canvas.worldCamera = _uiCamera;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            go.layer = LayerMask.NameToLayer("UI");
            go.name = item.eWindow.ToString();
            GameObject.DontDestroyOnLoad(go);
            item.window = go.GetComponent<BaseViewWindow>();
            _windowObjRefreshCount[item.eWindow]++;
            yield return null;
        }

        /// <summary>
        /// 清理Obj缓存内容
        /// </summary>
        /// <param name="window"></param>
        private void ClearObjCache(EWindow window = EWindow.None)
        {
            if(window == EWindow.None)
            {
                var allObjKey = _windowObj.Keys.GetEnumerator();
                while (allObjKey.MoveNext())
                {
                    var key = allObjKey.Current;
                    ClearObj(_windowObj[key]);
                }
                _windowObj.Clear();
                _windowObjRefreshCount.Clear();
            }
            else
            {
                _windowObjRefreshCount[window]--;
                if(_windowObjRefreshCount[window] == 0)
                {
                    ClearObj(_windowObj[window]);
                    _windowObj.Remove(window);
                    _windowObjRefreshCount.Remove(window);
                }
            }
        }
        /// <summary>
        /// 移除obj
        /// </summary>
        /// <param name="obj"></param>
        private void ClearObj(AssetLoader<GameObject> obj)
        {
            obj = null;
        }

    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;
using System;
using UnityEngine.UI;

namespace Framework.MVC
{
    public enum EWindowType
    {
        //normal界面
        Normal = 1<<0,
        //弹窗
        Popup = 1<<1,
        //固定界面弹窗
        Fixed = 1<<2,
    }
    public class ViewManager :MonoBehaviour
    {
        public static ViewManager instance;
        //ui Camera
        private Camera _uiCamera;
        //缓存
        private readonly int cacheMax = 5;
        private Dictionary<EWindow, BaseViewWindow> _cacheMap = new Dictionary<EWindow, BaseViewWindow>();

        /// <summary>
        /// 根据EWindowType 来获取显示的堆栈情况。
        /// </summary>
        private Dictionary<EWindowType, Stack<BaseViewWindow>> _showStack = new Dictionary<EWindowType, Stack<BaseViewWindow>>();
        /// <summary>
        /// 加载中的界面
        /// </summary>
        private HashSet<EWindow> _loadingMap = new HashSet<EWindow>();
        /// <summary>
        /// 等待加载的界面
        /// </summary>
        private Queue<EWindow> _waitLoadQueue = new Queue<EWindow>();
        /// <summary>
        /// 展示面板排队使用的协程
        /// </summary>
        private Coroutine _showCoroutine;

        //显示层级
        private int _orderLayer = 1000;
        private int _orderStep = 20;//每开一个界面增加层级

        //唯一id
        private Queue<int> _releaseIDQueue = new Queue<int>();
        private int _newID = 0;
        
        public void OnInitlization(Camera uiCamera)
        {
            _showStack.Add(EWindowType.Fixed, new Stack<BaseViewWindow>());
            _showStack.Add(EWindowType.Normal, new Stack<BaseViewWindow>());
            _showStack.Add(EWindowType.Popup, new Stack<BaseViewWindow>());
            _uiCamera = uiCamera;
            _uiCamera.transform.position = new Vector3(9999, 9999, 9999);
            _uiCamera.clearFlags = CameraClearFlags.Depth;
            _uiCamera.cullingMask = LayerMask.NameToLayer("UI");
            _uiCamera.orthographicSize = 0;
            _uiCamera.allowHDR = false;
            _uiCamera.allowMSAA = false;
            _orderStep = 20;
        }

        public void OnShow(EWindow window, BaseWindowParams param = null)
        {
            if (!_loadingMap.Add(window))
            {
                throw new Exception($"被加载的资源正在加载中");
            }
            _loadingMap.Add(window);
            _waitLoadQueue.Enqueue(window);
            if (_showCoroutine == null)
            {
                _showCoroutine = StartCoroutine(CoShowWindow(window, false, param));
            }
        }

        /// <summary>
        /// 关闭界面
        /// </summary>
        /// <param name="window"></param>
        public void OnClose(BaseViewWindow window)
        {
            StartCoroutine(CoCloseWindow(window.viewConfig.windowType, window.viewID));
        }

        /// <summary>
        /// 返回上层界面
        /// </summary>
        public void OnBack()
        {

        }

        /// <summary>
        /// 获取唯一id
        /// </summary>
        private int NewViewID()
        {
            if(_releaseIDQueue.Count > 0)
            {
                return _releaseIDQueue.Dequeue();
            }
            return _newID++;
        }

        private void ReleaseViewID(int id)
        {
            _releaseIDQueue.Enqueue(id);
        }

        /// <summary>
        /// 缓存窗口
        /// </summary>
        /// <param name="window"></param>
        private void CacheWindow(BaseViewWindow window)
        {
            if (window.viewConfig.cache)
            {
                if (_cacheMap.Count >= cacheMax)
                {
                    var key = _cacheMap.Keys.GetEnumerator().Current;
                    var removeWindow = _cacheMap[key];
                    GameObject.DestroyImmediate(removeWindow.gameObject);
                    _cacheMap.Remove(key);
                }
                window.gameObject.SetActive(false);
                _cacheMap.Add(window.viewConfig.window, window);
            }
            else
            {
                GameObject.DestroyImmediate(window.gameObject);
            }
            
        }
        
        /// <summary>
        /// 排序展示界面窗口
        /// </summary>
        /// <param name="window"></param>
        /// <param name="isParent">父窗口可以优先展示</param>
        /// <param name="param"></param>
        /// <returns></returns>
        private IEnumerator CoShowWindow(EWindow window, bool isParent, BaseWindowParams param = null)
        {
            BaseViewWindow view = null;
            do
            {
                if (!isParent)
                {
                    window = _waitLoadQueue.Dequeue();
                }
                if (_cacheMap.ContainsKey(window))
                {
                    view = _cacheMap[window];
                    StartCoroutine(CoRealShowWindow(view, param));
                }
                else
                {
                    yield return CoLoadWindow(window, view);
                    if (view != null)
                    {
                        yield return CoRealShowWindow(view, param);
                    }
                }
            } while (!isParent && _waitLoadQueue.Count > 0);

            if (!isParent)
            {
                _showCoroutine = null;

            }

            yield return null;
        }

        /// <summary>
        /// 展示窗口
        /// </summary>
        /// <param name="view"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private IEnumerator CoRealShowWindow(BaseViewWindow view, BaseWindowParams param)
        {
            var viewConf = view.viewConfig;
            _loadingMap.Remove(viewConf.window);
            view.setViewID = NewViewID();
            if (viewConf.windowType == EWindowType.Normal)
            {
                //Normal 的窗口需要关闭当前所有的窗口
                yield return CoCloseWindow(EWindowType.Normal, -1, viewConf.parentWindow);
            }
            //等待父窗口开启之后在开启当前窗口。
            if (viewConf.parentWindow !=  EWindow.None )
            {
                if(_showStack[viewConf.windowType].Count == 0 || _showStack[viewConf.windowType].Peek().viewConfig.window != viewConf.parentWindow)
                {
                    yield return CoShowWindow(viewConf.parentWindow, true);
                }
            }

            view.mediator = Activator.CreateInstance(Type.GetType(viewConf.viewMediatorName)) as BaseViewMediator;
            view.gameObject.SetActive(true);
            if(viewConf.windowType == EWindowType.Fixed)
            {
                view.GetComponent<Canvas>().sortingOrder = viewConf.fixedOrderLayer;
            }
            else
            {
                view.GetComponent<Canvas>().sortingOrder = _orderLayer + (_showStack[EWindowType.Normal].Count + _showStack[EWindowType.Popup].Count) * _orderStep;
            }
            //在动画播放完毕之前不允许点击事件
            GraphicRaycaster _renderer = view.GetComponent<GraphicRaycaster>();
            _renderer.enabled = false;
            view.mediator.OnCreate(view, param);
            _showStack[viewConf.windowType].Push(view);
            StartCoroutine(CoPlayAnimatior(view, true, () =>
            {
                if (_renderer)
                {
                    _renderer.enabled = true;
                }

            }));
            
        }

        /// <summary>
        /// 加载预制件资源
        /// </summary>
        /// <param name="window"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private IEnumerator CoLoadWindow(EWindow window, BaseViewWindow view)
        {
            //加载界面资源
            string path = "Assets/" + MVCUtil.PREFAB_PATH + "/" + window.ToString() + ".prefab";
#if UNITY_EDITOR
           GameObject  go = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
#endif
            if(go == null)
            {
                throw new Exception($"资源加载错误:{path}");
            }
            //加载完成
            go = GameObject.Instantiate<GameObject>(go);
            view = go.GetComponent<BaseViewWindow>();
            go.gameObject.SetActive(false);
            Canvas canvas = go.GetComponent<Canvas>();
            canvas.worldCamera = _uiCamera;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            go.layer = LayerMask.NameToLayer("UI");
            GameObject.DontDestroyOnLoad(go);
            yield return null;
        }

        /// <summary>
        /// 关闭窗口，
        /// </summary>
        /// <param name="windowType">关闭窗口类型</param>
        /// <param name="remove">指定关闭某一个窗口,其他的窗口会被保留</param>
        /// <param name="retain">保留的窗口，按栈顺序关闭，一直到保留的窗口的位置</param>
        /// <returns></returns>
        private IEnumerator CoCloseWindow(EWindowType windowType,int removeID = -1,EWindow retain = EWindow.None)
        {
            var showWindow = _showStack[windowType];
            Queue<BaseViewWindow> allClose = new Queue<BaseViewWindow>();
            if(removeID != -1)
            {
                Stack<BaseViewWindow> temp = new Stack<BaseViewWindow>();
                while(showWindow.Count > 0)
                {
                    var view = showWindow.Pop();
                    if(view.viewID == removeID)
                    {
                        allClose.Enqueue(view);
                        break;
                    }
                    temp.Push(view);
                }
                while(temp.Count > 0)
                {
                    showWindow.Push(temp.Pop());
                }
            }
            else if(retain != EWindow.None)
            {
                while(showWindow.Count > 0)
                {
                    var view = showWindow.Peek();
                    if(view.viewConfig.window == retain)
                    {
                        break;
                    }
                    allClose.Enqueue(showWindow.Pop());
                }
            }
            else
            {
                while(showWindow.Count > 0)
                {
                    allClose.Enqueue(showWindow.Pop());
                }
            }
            while (allClose.Count > 0)
            {
                var view = allClose.Dequeue();
                view.mediator.OnDestory();
                view.mediator = null;
                ReleaseViewID(view.viewID);
                yield return CoPlayAnimatior(view, false);
                CacheWindow(view);
            }
            yield return null;
        }

       /// <summary>
       /// 播放结束，开启，持续的动画
       /// </summary>
       /// <param name="view"></param>
       /// <param name="isopen"></param>
       /// <param name="callback"></param>
       /// <returns></returns>
        private IEnumerator CoPlayAnimatior(BaseViewWindow view, bool isopen,Action callback =null)
        {
            AnimatorWindowAnimationBehaviour ani = view.GetComponent<AnimatorWindowAnimationBehaviour>();
            if (ani == null)
            {
                ani = view.gameObject.AddComponent<AnimatorWindowAnimationBehaviour>();
            }
            if (isopen)
            {
                ani._openClip = view.viewConfig.openAnimation;
                ani._closeClip = view.viewConfig.closeAnimation;
                ani._loopClip = view.viewConfig.loopAnimation;
            }
            bool show = true;
            while (show)
            {
                if (isopen)
                {
                    ani.OnOpen(() =>
                    {
                        show = false;
                        callback?.Invoke();
                    });
                }
                else
                {
                    ani.OnClose(() =>
                    {
                        show = false;
                        ani.StopAllCoroutines();
                        callback?.Invoke();
                    });
                }
                yield return null;
            }

        }
    }
}
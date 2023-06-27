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
        Normal = 0,
        //弹窗
        Popup = 1,
        //固定界面弹窗
        Fixed = 2,
    }

    /// <summary>
    /// 窗口状态
    /// </summary>
    public enum EWindowState
    {
        /// <summary>
        /// 加载中
        /// </summary>
        Loading,
        /// <summary>
        /// 展示
        /// </summary>
        Show,
        /// <summary>
        /// 关闭
        /// </summary>
        Close,
        /// <summary>
        /// 缓存
        /// </summary>
        Cache,
        /// <summary>
        /// 移除
        /// </summary>
        Destroy,
    }

    public class WindowItem : IComparer<WindowItem>,IEquatable<WindowItem>
    {
        public EWindowState eState;
        public EWindowType eType;
        public EWindow eWindow;
        public BaseViewWindow window;
        public BaseWindowParams param;
        public int viewID;

        public int Compare(WindowItem x, WindowItem y)
        {
            return x.eWindow.CompareTo(y);
        }

        public bool Equals(WindowItem other)
        {
            return eWindow == other.eWindow;
        }


    }

    public partial class ViewManager :MonoBehaviour
    {
        public static ViewManager instance;
        //ui Camera
        private Camera _uiCamera;
        //缓存
        private readonly int cacheMax = 5;
        private Dictionary<EWindow, WindowItem> _cacheMap = new Dictionary<EWindow, WindowItem>();

        /// <summary>
        /// 窗口Item字典
        /// </summary>
        private Dictionary<int, WindowItem> _window = new Dictionary<int, WindowItem>();
        /// <summary>
        /// 显示栈。
        /// </summary>
        private Stack<BaseViewWindow> _showStack = new Stack<BaseViewWindow>();
        /// <summary>
        /// 返回上一层使用的栈
        /// </summary>
        private Stack<WindowItem> _goBackStack = new Stack<WindowItem>();
        //显示层级
        private int _orderLayer = 1000;
        private int _orderStep = 20;//每开一个界面增加层级

        //唯一id
        private Queue<int> _releaseIDQueue = new Queue<int>();
        private int _newID = 0;
        
        public void OnInitlization(Camera uiCamera)
        {
            _uiCamera = uiCamera;
            _uiCamera.transform.position = new Vector3(9999, 9999, 9999);
            _uiCamera.clearFlags = CameraClearFlags.Depth;
            _uiCamera.cullingMask = 1 << LayerMask.NameToLayer("UI");
            _uiCamera.allowHDR = false;
            _uiCamera.allowMSAA = false;
            _orderStep = 20;
            _uiCamera.depth = -1;
        }

        public Coroutine OnShow(EWindow window, BaseWindowParams param = null)
        {
            WindowItem item;
            if (_cacheMap.TryGetValue(window, out item))
            {
                item.param = param;
                item.eState = EWindowState.Show;
            }
            else
            {
                item = new WindowItem()
                {
                    eWindow = window,
                    param = param,
                    eState = EWindowState.Loading
                };
            }
            item.viewID = NewViewID();
            _window.Add(item.viewID, item);
            return StartCoroutine(CoRealShowWindow(item));
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
        public void OnBack(EWindow normalWindow,BaseWindowParams parmas = null)
        {
            if(_goBackStack.Count > 0)
            {
                var lastItem = _goBackStack.Pop();
                normalWindow = lastItem.eWindow;
                parmas = lastItem.param;
            }
            if(parmas == null)
            {
                parmas = new BaseWindowParams() { _isback = true };
            }
            OnShow(normalWindow, parmas);
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
        /// <param name="windowItem"></param>
        private void CacheWindow(WindowItem windowItem)
        {
            if (windowItem.window.viewConfig.cache)
            {
                if (_cacheMap.Count >= cacheMax)
                {
                    var key = _cacheMap.Keys.GetEnumerator().Current;
                    var removeWindow = _cacheMap[key];
                    GameObject.DestroyImmediate(removeWindow.window.gameObject);
                    ClearObjCache(removeWindow.eWindow);
                    removeWindow.eState = EWindowState.Destroy;
                    _cacheMap.Remove(key);
                }
                windowItem.window.gameObject.SetActive(false);
                _cacheMap.Add(windowItem.window.viewConfig.window, windowItem);
                windowItem.eState = EWindowState.Cache;
            }
            else
            {
                GameObject.DestroyImmediate(windowItem.window.gameObject);
                windowItem.eState = EWindowState.Destroy;
                ClearObjCache(windowItem.eWindow);
            }

        }

        /// <summary>
        /// 展示窗口
        /// </summary>
        /// <param name="WindowItem">item</param>
        /// <returns></returns>
        private IEnumerator CoRealShowWindow(WindowItem item)
        {
            if(item.eState == EWindowState.Loading)
            {
                yield return CoLoadWindow(item);
            }
            BaseViewWindow view = item.window;
            var viewConf = item.window.viewConfig;
            item.window.viewID = item.viewID;
            if (viewConf.windowType == EWindowType.Normal)
            {
                //Normal 的窗口需要关闭当前所有的窗口
                yield return CoCloseWindow(EWindowType.Normal, -1, viewConf.parentWindow);
            }
            //开启父面板
            if (viewConf.parentWindow != EWindow.None && _showStack.Peek() && _showStack.Peek().viewConfig.window != viewConf.parentWindow)
            {
                Coroutine baseLoad = OnShow(viewConf.parentWindow);
                
                yield return baseLoad;
            }
           

            view.mediator = Activator.CreateInstance(Type.GetType(viewConf.viewMediatorName)) as BaseViewMediator;
            view.gameObject.SetActive(true);
            if(viewConf.windowType == EWindowType.Fixed)
            {
                view.GetComponent<Canvas>().sortingOrder = viewConf.fixedOrderLayer;
            }
            else
            {
                view.GetComponent<Canvas>().sortingOrder = _orderLayer + _showStack.Count * _orderStep;
            }
            //在动画播放完毕之前不允许点击事件
            GraphicRaycaster _renderer = view.GetComponent<GraphicRaycaster>();
            _renderer.enabled = false;
            view.mediator.OnCreate(view, item.param);
            _showStack.Push(view);
            
            StartCoroutine(CoPlayAnimatior(view, true, () =>
            {
                if (_renderer)
                {
                    _renderer.enabled = true;
                }

            }));
            
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
            Queue<BaseViewWindow> allClose = new Queue<BaseViewWindow>();
            Stack<BaseViewWindow> retainWindow = new Stack<BaseViewWindow>();
            if(removeID == -1)
            {
                while(_showStack.Count > 0)
                {
                    var window = _showStack.Pop();
                    if(window.viewConfig.window == retain)
                    {
                        retainWindow.Push(window);
                    }
                    else
                    {
                        allClose.Enqueue(window);
                    }
                }
            }
            else
            {
                while(_showStack.Count > 0)
                {
                    var window = _showStack.Pop();

                    if (removeID == window.viewID)
                    {
                        allClose.Enqueue(window);
                    }
                    else
                    {
                        retainWindow.Push(window);
                    }
                }
            }
            while(retainWindow.Count > 0)
            {
                _showStack.Push(retainWindow.Pop());
            }
            while (allClose.Count > 0)
            {
                var view = allClose.Dequeue();
                view.mediator.OnDestory();
                view.mediator = null;
                ReleaseViewID(view.viewID);
                WindowItem item = _window[view.viewID];
                item.eState = EWindowState.Close;
                _window.Remove(view.viewID);
                yield return CoPlayAnimatior(view, false);
                CacheWindow(item);
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
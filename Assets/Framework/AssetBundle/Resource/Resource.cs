using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Sword.Resource
{

    public enum ResourceType
    {
        Error = 0 ,
        Use ,
        WaitUnLoad,
        Loading,
    }

    /// <summary>
    /// 资源使用计数器
    /// </summary>
    public abstract class ResourceUserCounterBase
    {
        public int counter
        {
            get; protected set;
        }

        public bool isRecove
        {
            get; protected set;
        }

        public virtual void Retain()
        {
            counter++;
            if (isRecove)
            {
                UnDispose();
            }
        }


        public virtual void Release()
        {
            counter--;
            if (counter == 0)
            {
                Dispose();
            }
        }

        public virtual void Dispose()
        {

        }

        public virtual void UnDispose()
        {
            isRecove = false;
        }

    }

    public class Resource : ResourceUserCounterBase
    {
        private Dictionary<string, UnityEngine.Object> _resourceMap = new Dictionary<string, UnityEngine.Object>();
        private HashSet<string> _allNameMap = new HashSet<string>();

        //依赖列表
        private HashSet<Resource> _manifest = new HashSet<Resource>();
        public List<Resource> manifest { get { return _manifest.ToList(); } }

        //被依赖列表
        private HashSet<string> _beDependentMap = new HashSet<string>();
        public List<string> beDependentMap { get { return _beDependentMap.ToList(); } }


        public ResourceType resourceState { get; private set; }

        ///是否是依赖项,如果是被依赖的 不进行移除
        public bool isDependency = false;

        private AssetBundle _bundle;
        public AssetBundle bundle
        {
            get { return _bundle; }
            internal set
            {
                _bundle = value;
                if (!isDependency && _bundle != null)
                {
                    ResourceManager.getInstance.AddUnLoadBundle(this);
                }
                if(_bundle!= null)
                {
                    bundleIsLoad = true;
                }
                else
                {
                    bundleIsLoad = false;
                }
            }
        }

        public bool bundleIsLoad
        {
            get;private set;
        }

        /// <summary>
        /// 是否当前AB包中的全部资源全部加载完毕了
        /// </summary>
        public bool isLoadFinsh
        {
            get
            {
                return _allNameMap.Count == _resourceMap.Count;
            }
        }

        private string _abName;
        public string abName { get { return _abName; } }

        /// <summary>
        /// 回收计时
        /// </summary>
        protected float _unLoadTimer = 5f;
        private float _unLoadTime = 0f;
    
        /// <summary>
        /// 卸载Bundle倒计时
        /// </summary>
        protected float _unLoadBundleTimer = 2f;
        private float _unLoadBundleTime = 0;
       
        /// <summary>
        /// 加载当前res中的物体的回调列表  
        /// </summary>
        private Dictionary<string,List<Action<UnityEngine.Object>>> _resTaskMap = new Dictionary<string, List<Action<UnityEngine.Object>>>();


#if UNITY_EDITOR
        private List<double> _loadingTime = new List<double>();
        public List<double> loadingTime { get { return _loadingTime; } }

        public void AddLoadingTime(double time)
        {
            _loadingTime.Add(time);
        }
#endif
        internal Resource(string _abname, AssetBundle _bundle, List<Resource> mainifest, bool _isdependency = false,bool _iserror =false, params UnityEngine.Object[] objs)
        {
            _abName = _abname;
            if (_iserror)
            {
                resourceState = ResourceType.Error;
                bundleIsLoad = false;
                isRecove = true;
                Dispose();
                return;
            }
            resourceState = ResourceType.Use;
            isDependency = _isdependency;
            bundle = _bundle;
            string[] names = _bundle.GetAllAssetNames();
            for (int i = 0; i < names.Length; i++)
            {
                if (!_allNameMap.Add(names[i]))
                {
                    Debug.LogError(abName + "==中有重复名字的内容");
                }
            }
            for (int i = 0; i < objs.Length; i++)
            {
                _resourceMap.Add(objs[i].name, objs[i]);
            }
            //依赖的引用计数自动增长
            AddDependency(mainifest);
        }

        internal void AddDependency(List<Resource> resources)
        {
            if (resources == null) return;
            Debug.Log(abName + " 依赖信息:");
            for (int i = 0; i < resources.Count; i++)
            {
                if (_manifest.Add(resources[i]))
                {
                    resources[i].Retain(abName);
                }
            }
        }


        internal void AddObj(params UnityEngine.Object[] objs)
        {
            foreach (var obj in objs)
            {
                if (!_resourceMap.ContainsKey(obj.name))
                {
                    _resourceMap.Add(obj.name, obj);
                }
            }
        }

        public void LateUpdate()
        {
            if (bundleIsLoad)
            {
                _unLoadBundleTime += Time.deltaTime;
                if (_unLoadBundleTime >= _unLoadBundleTimer)
                {
                    ClearBundle();
                    _unLoadBundleTime = 0;
                }
            }
            if (isRecove)
            {
                _unLoadTime += Time.deltaTime;
                if (_unLoadTime >= _unLoadTimer)
                {
                    Clear();
                    _unLoadTime = 0;
                }
            }
        }


        /// <summary>
        /// 添加资源加载回调方案
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public void AddResLoadCallback(string name,Action<UnityEngine.Object> action) 
        {
            if (resourceState == ResourceType.Error)
            {
                Debug.LogErrorFormat("加载失败,abname:{0},name{1}", abName, name);
                return ;
            }
            _resTaskMap[name].Add(action);
        }

        /// <summary>
        /// 创建物体
        /// </summary>
        /// <typeparam name="T">UnityEngine.Object</typeparam>
        /// <param name="name">物体名称</param>
        /// <param name="parentobj">父物体（在当前创建物体为GameObject的时候不用）</param>
        /// <returns>false  请使用AddResLoadCallback 添加加载成功回调</returns>
        public bool Instance<T>(string name, out T obj, GameObject parentobj = null) where T : UnityEngine.Object
        {
            obj = null;
            if (resourceState == ResourceType.Error)
            {
                Debug.LogErrorFormat("加载失败,abname:{0},name{1}", abName, name);
                return false;
            }
            if (parentobj == null && typeof(T) != typeof(GameObject))
            {
                Debug.LogErrorFormat("abName:{0} Create Obj Name:{1} Log", abName, name);
                return new UnityEngine.Object() as T;
            }
            if (_resourceMap.ContainsKey(name))
            {
                obj = _resourceMap[name] as T;
            }
            if (obj == null)
            {
                resourceState = ResourceType.Loading;
                if (_resTaskMap.ContainsKey(name))
                {
                    return false;
                }
                AssetBundleTaskInfo info = new AssetBundleTaskInfo(abName, ELoadingType.Async, ELoadingLevel.Nomal, (res) =>
                {
                    for (int i = 0; i < _resTaskMap[name].Count; i++)
                    {
                        UnityEngine.Object item = GameObject.Instantiate(_resourceMap[name] as T);
                        item.name = name;
                        _resTaskMap[name][i].Invoke(item);
                    }
                    _resTaskMap.Remove(name);

                }, false, name);
                ResourceManager.getInstance.AddLoadingTask(info);
                _resTaskMap.Add(name, new List<Action<UnityEngine.Object>>());
                return false;
            }
            obj = GameObject.Instantiate(obj);
            obj.name = name;
          
            return true;
        }

        public  bool Instance(string name, out UnityEngine.Object obj,GameObject parentobj =null)
        {
            return Instance<UnityEngine.Object>(name,out obj,parentobj);
        }

        
        /// <summary>
        /// 全自动引用计数（外部不用调用）
        /// </summary>
        public override void Retain()
        {
            base.Retain();
            Debug.Log(abName + " Retain:" + counter);
        }

        /// <summary>
        /// 添加引用为依赖引用
        /// </summary>
        /// <param name="name"></param>
        public void Retain(string name)
        {
            Retain();
            if (_beDependentMap.Add(name) && !isDependency)
            {
                isDependency = true;
                if (!bundleIsLoad)
                {
                    ResourceManager.getInstance.AddLoadingTask(new AssetBundleTaskInfo(_abName, ELoadingType.Async, ELoadingLevel.Nomal, null));
                }
            }
        }


        public override void Release()
        {
            base.Release();
           // Debug.LogFormat("Abname:{1) Release Counter:{2}", abName, counter);
        }

        /// <summary>
        /// 释放依赖的时候使用此方法释放
        /// </summary>
        /// <param name="abname"></param>
        public void Release(string abname)
        {
            Release();
            _beDependentMap.Remove(abname);
            if (_beDependentMap.Count == 0)
            {
                isDependency = false;
                if (bundleIsLoad)
                {
                    ResourceManager.getInstance.AddUnLoadBundle(this);
                }
            }
        }
       

        public override void Dispose()
        {
            ResourceManager.getInstance.UnLoadResource(this);
            if(resourceState != ResourceType.Error)
            {
                resourceState = ResourceType.WaitUnLoad;
            }
        }

        public override void UnDispose()
        {
            _unLoadTime = 0;
        }


        public void ClearBundle()
        {
            bundle.Unload(false);
            bundleIsLoad = false;
            Debug.LogFormat("ClearBundle AbName={0}", abName);
        }

        public void Clear()
        {
            if (bundleIsLoad)
            {
                bundle.Unload(true);
            }

            foreach (var res in _manifest)
            {
                res.Release(abName);
            }
#if UNITY_EDITOR
            _loadingTime.Clear();
#endif
            bundle = null;
            bundleIsLoad = false;
        }
    }
}
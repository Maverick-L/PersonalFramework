using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
/// <summary>
/// 资源加载：1.资源的依赖计数
///                 资源依赖计数，在时间内自动检查引用计数
///                  资源依赖计数，在物体被Destory的时候返回抹去消息
///                  
///          2.AB包的加载方案：    1.同步
///                               2.异步
///                               
///          3.AB包内容获取：      1.All
///                               2.One
///                             
/// </summary>
namespace Sword.Resource
{
    /// <summary>
    /// 加载状态
    /// </summary>
    public enum ELoadingState
    {
        None,
        Wait,
        LoadingHead,
        LoadingManifest,
        Loading,
        Finsh,
        Error,
    }

    public enum ELoadingType
    {
        Sync,
        Async
    }

    public enum ELoadingLevel
    {
        /// <summary>
        /// 预加载紧急程度低
        /// </summary>
        Preload =1,
        /// <summary>
        /// 普通状态
        /// </summary>
        Nomal =0,
        /// <summary>
        /// 紧急
        /// </summary>
        Urgent =2,
        /// <summary>
        /// 依赖加载
        /// </summary>
        Dependency = 3,
    }


    public class Manifest
    {
        private Dictionary<string, AssetBundleDependenciesInfo> _manifestMap = new Dictionary<string, AssetBundleDependenciesInfo>();

        public Manifest()
        {
            string path = ResourceUtil.GetDependencyPath();
            if (!File.Exists(path))
            {
                Debug.LogError("当前此地址无效:" + path);
            }
            else
            {
                List<AssetBundleDependenciesInfo> _manifestInfoes = new List<AssetBundleDependenciesInfo>();
                string json = File.ReadAllText(path);
                Debug.Log("json:" + json);
                _manifestInfoes = JsonMapper.ToObject<List<AssetBundleDependenciesInfo>>(json);
                foreach (var man in _manifestInfoes)
                {
                    Debug.Log("abname:" + man.abname + "===count:" + man.manifest.Count);
                    _manifestMap.Add(man.abname, man);
                }
            }
        }

        public AssetBundleDependenciesInfo GetManifestInfo(string abname)
        {
            return _manifestMap[abname];
        }

        public List<string> GetAllDependency(string abname)
        {
            return _manifestMap[abname].manifest;
        }
    }


    public class ResourceManager : MonoBehaviour
    {
        private static ResourceManager _instance;
        public static ResourceManager getInstance => _instance;

        private int _onceLoadMaxNum = 10;//一次最大加载数量
        public int loadNum { get; private set; }//当前的加载中的内容
        public int OnceLoadMaxNum { get { return _onceLoadMaxNum; } }

        private int _autoID = 1000;  // 自动回收管理ID；
        private bool _isLoading = true;

        private Manifest _manifest;

        /// <summary>
        /// 等待加载队列，按照紧急等级分配
        /// </summary>
        private Dictionary<ELoadingLevel, Queue<AssetBundleTaskInfo>> _waitLoadingQueue = new Dictionary<ELoadingLevel, Queue<AssetBundleTaskInfo>>();

    
        /// <summary>
        /// 加载等待队列中的依赖列表（优先加载）
        /// </summary>
        private Dictionary<string, List<AssetBundleTaskInfo>> _waitLoadingDependencyQueue = new Dictionary<string, List<AssetBundleTaskInfo>>();
       
        /// <summary>
        /// 加载合集( 等待加载中  正在加载中)
        /// </summary>
        private HashSet<string> _LoadingMap = new HashSet<string>();

        /// <summary>
        /// 等待加载列表数量
        /// </summary>
        private int _waitLoadingCounter = 0;

        /// <summary>
        /// 当前加载的任务合集
        /// </summary>
        private Dictionary<string, ResourceBundleTask> _bundleTaskMap = new Dictionary<string, ResourceBundleTask>();
        /// <summary>
        /// 任务完成释放之后的合集
        /// </summary>
        private Queue<ResourceBundleTask> _recoveBundleTask = new Queue<ResourceBundleTask>();


        /// <summary>
        /// 当前已经加载完毕的所有的Resource的名字合集,这部分可以使用HashCode值进行存储
        /// </summary>
        private HashSet<string> _resourceMap = new HashSet<string>();
        /// <summary>
        /// 正在使用中的合集
        /// </summary>
        private Dictionary<string, Resource> _userBundleMap = new Dictionary<string, Resource>();
        /// <summary>
        /// 加载失败合集，定时清理
        /// </summary>
        private Dictionary<string, Resource> _loaingFileBundleMap = new Dictionary<string, Resource>();
        /// <summary>
        /// 等待回收内存的合集
        /// </summary>
        private Dictionary<string, Resource> _waitRecoveMap = new Dictionary<string, Resource>();
        /// <summary>
        /// 刷新列表
        /// </summary>
        HashSet<Resource> _lateUpdateMap = new HashSet<Resource>();



        public void Awake()
        {
            _waitLoadingQueue.Add(ELoadingLevel.Nomal, new Queue<AssetBundleTaskInfo>());
            _waitLoadingQueue.Add(ELoadingLevel.Preload, new Queue<AssetBundleTaskInfo>());
            _waitLoadingQueue.Add(ELoadingLevel.Urgent, new Queue<AssetBundleTaskInfo>());
            _waitLoadingQueue.Add(ELoadingLevel.Dependency, new Queue<AssetBundleTaskInfo>());
            _manifest = new Manifest();
            _instance = this;

        }

        private void LateUpdate()
        {
            //加载 加载规则
            //1一次性加载10个，创建一个协程，所有的加载必须顺序加载

            if (_isLoading && _waitLoadingCounter > 0)
            {
                StartCoroutine(Load());
            }
           
            if(_waitLoadingCounter > 0 && _waitLoadingQueue[ELoadingLevel.Dependency].Count > 0)
            {
                StartCoroutine(Load(true));
            }
            if(_isLoading && _waitLoadingCounter == 0)
            {
                StopAllCoroutines();
            }
            ResourceLateUpdate();
        }

        private void ResourceLateUpdate()
        {
            var reses = _lateUpdateMap.ToList();
            foreach (var _res in reses)
            {
                _res.LateUpdate();
            }
        }

        IEnumerator Load(bool isDependency =false)
        {
            AssetBundleTaskInfo info = GetLoadingInfo(isDependency);
            ResourceBundleTask task = LoadingTask(info);
            _waitLoadingCounter--;
            if (isDependency)
            {
                yield return StartCoroutine(task.LoadingBundle());
            }
            else
            {
                _isLoading = false;
                yield return StartCoroutine(task.LoadingBundle());
                _isLoading = true;
            }
            yield return null;
        }

        private AssetBundleTaskInfo GetLoadingInfo(bool isDependency)
        {
            if (isDependency)
            {
                return _waitLoadingQueue[ELoadingLevel.Dependency].Dequeue();
            }
            if (_waitLoadingQueue[ELoadingLevel.Urgent].Count > 0)
            {
                return _waitLoadingQueue[ELoadingLevel.Urgent].Dequeue();
            }
            else if (_waitLoadingQueue[ELoadingLevel.Nomal].Count > 0)
            {
                return _waitLoadingQueue[ELoadingLevel.Nomal].Dequeue();
            }
            else
            {
                return _waitLoadingQueue[ELoadingLevel.Preload].Dequeue();
            }
        }

        /// <summary>
        /// 添加任务到正在加载队列
        /// </summary>
        /// <param name="info"></param>
        /// <param name="_task"></param>
        /// <returns></returns>
        private ResourceBundleTask LoadingTask(AssetBundleTaskInfo info)
        {
            Debug.Log("Load ABName:" + info.abname);
            ResourceBundleTask task = CreateTask();
            task.Init(info);
            _bundleTaskMap.Add(info.abname, task);            
            return task;

        }

        public void ReadyLoadingTask(string abname,Action<Resource> _callback)
        {

        }

        /// <summary>
        /// 添加一个新的任务
        /// </summary>
        /// <param name="task"></param>
        /// <param name="callback"></param>
        public bool AddLoadingTask(AssetBundleTaskInfo task)
        {
            if (!LoadVerification(task)) return false;

            if (_LoadingMap.Add(task.abname))
            {
                _waitLoadingQueue[task.loadingLevel].Enqueue(task);
                _waitLoadingCounter++;
            }
            else
            {
                if (_bundleTaskMap.ContainsKey(task.abname))
                {
                    _bundleTaskMap[task.abname].AddCallback(task.callbackMap.ToArray());
                }
                else
                {
                    return true;
                }
            }
            
            return true;

        }

        /// <summary>
        /// 加载完成的回调
        /// </summary>
        /// <param name="res"></param>
        internal void LoadingFinsh(Resource res,ResourceBundleTask task)
        {
           // _loadingMap.Remove(res.abName);
           if(task.info.loadingState != ELoadingState.Error)
            {
                if (_resourceMap.Add(res.abName))
                {
                    _userBundleMap.Add(res.abName, res);
                }
            }
            else
            {
                if (_resourceMap.Add(res.abName))
                {
                    _loaingFileBundleMap.Add(res.abName, res);
                }
            }
            StopCoroutine(task.LoadingBundle());
            loadNum--;
            if (!task.info.isDependency)
            {
                _LoadingMap.Remove(res.abName);
            }
            _bundleTaskMap.Remove(task.info.abname);
            task.Release();
            _recoveBundleTask.Enqueue(task);  
        }

        
        /// <summary>
        ///         /// 获取资源的三种情况
        ///         1.当前usermap中有 直接返回
        ///         2.当前usermap中没有，waitunload中有，将其移出wait到user，返回
        /// </summary>
        /// <param name="abname"></param>
        /// <param name="res"></param>
        /// <returns>返回true为当前资源已经存在</returns>
        public bool GetResource(string abname,out Resource res)
        {
            res = null;
            if (_resourceMap.Contains(abname))
            {
                if (_userBundleMap.ContainsKey(abname))
                {
                    res = _userBundleMap[abname];
                }
                else if (_waitRecoveMap.ContainsKey(abname))
                {
                    res = _waitRecoveMap[abname];
                    _waitRecoveMap.Remove(abname);
                    _userBundleMap.Add(abname, res);
                    res.UnDispose();
                    if (res.bundle || !res.isDependency)
                    {
                        _lateUpdateMap.Remove(res);
                    }
                }
                else if(_loaingFileBundleMap.ContainsKey(abname))
                {
                    Debug.LogError("重新加载上一次加载失败的AB包:" + abname);
                    _resourceMap.Remove(abname);
                    _loaingFileBundleMap.Remove(abname);
                    return false;
                }
                else
                {
                    return false;
                }
                return true;
            }
            return false;
        }


        public int GetAutoID()
        {
            return _autoID++;
        }


        /// <summary>
        /// 等待卸载头部列表添加
        /// </summary>
        /// <param name="res"></param>
        public void AddUnLoadBundle(Resource res)
        {
            _lateUpdateMap.Add(res);
        }

        private ResourceBundleTask CreateTask()
        {
            if(_recoveBundleTask.Count != 0)
            {
                return _recoveBundleTask.Dequeue();
            }
            return new ResourceBundleTask(this);
        }

        public void UnLoadResource(Resource res)
        {
            if (!_waitRecoveMap.ContainsKey(res.abName))
            {
                _waitRecoveMap.Add(res.abName,res);
            }
            _lateUpdateMap.Add(res);
        }

        public void UnLoadFinsh(Resource res)
        {
            _waitRecoveMap.Remove(res.abName);
            _resourceMap.Remove(res.abName);

        }



        /// <summary>
        /// 是否允许加载验证
        /// </summary>
        /// <returns></returns>
        private bool LoadVerification(AssetBundleTaskInfo task)
        {
            //名字验证
            if (!System.IO.File.Exists(task.path))
            {
                Debug.LogError("path Is None In Disk===path:" + task.path);
                return false;
            }
            return true;
        }


        private void OnDestroy()
        {
            foreach(var res in _userBundleMap.Values)
            {
                Debug.LogFormat("Recove Cache: AbName:{0} ===Bundle:{1}" , res.abName,res.bundle);
                if (res.bundleIsLoad)
                {
                    res.bundle.Unload(true);
                }
            }

            foreach(var res in _waitRecoveMap.Values)
            {
                if (res.bundleIsLoad)
                {
                    res.bundle.Unload(true);
                }
            }
            
        }
    }
}
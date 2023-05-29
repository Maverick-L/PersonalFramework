using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Sword.Resource
{
    public class AssetBundleTaskInfo
    {
        public string abname;
        public string path;
        public ELoadingState loadingState;
        public ELoadingType loadingType;
        public ELoadingLevel loadingLevel;
        public bool isLoadingAll;
        public string[] loadingObjNames;
        public bool isAsync;
        public List<Action<Resource>> callbackMap = new List<Action<Resource>>();
        public bool isDependency = false;
        public List<Resource> mainDependence = new List<Resource>();

        public AssetBundleTaskInfo(string _abname,ELoadingType _type,ELoadingLevel _level,Action<Resource> callback,bool isloadingAll =false,params string[] loadingnames)
        {
            abname = _abname;
            path = ResourceUtil.GetFilePath(_abname, true);
            loadingType = _type;
            loadingLevel = _level;
            loadingState = ELoadingState.None;
            isLoadingAll = isloadingAll;
            loadingObjNames = loadingnames;
            callbackMap.Add(callback);
        }

        public void AddCallback(params Action<Resource>[] callback)
        {
            callbackMap.AddRange(callback);
        }
        
    }

#if UNITY_EDITOR
    //资源加载任务只会发生在程序集内部
    public class ResourceBundleTask

#else
     internal class ResourceBundleTask

#endif
    {
        private ResourceManager _manager;
        private AssetBundle _package;
        private AssetBundleTaskInfo _info;
        private AssetBundleCreateRequest _loadingBundleRequest;
        private AssetBundleRequest _loadingResourceRequest;
        private bool _isInit = false;
        private Resource _resource;
        public AssetBundle package { get { return _package; } }
        public ELoadingType loadingType { get { return _info.loadingType; } }
        public AssetBundleCreateRequest loadingBundleRequest { get { return _loadingBundleRequest; } }
        public AssetBundleRequest loadingResourceRequest { get { return _loadingResourceRequest; } }

        public AssetBundleTaskInfo info { get { return _info; } }

        #region class Init And Release
        internal ResourceBundleTask(ResourceManager manager)
        {
            _manager = manager;
        }

        internal void Init(AssetBundleTaskInfo info)
        {
            _info = info;
            _isInit = true;
            
        }

        internal void Release()
        {
            _isInit = false;
            _info = null;
            _package = null;
            _loadingBundleRequest = null;
            _loadingResourceRequest = null;
            _resource = null;
        }
        #endregion

        public void AddCallback(params Action<Resource>[] _callback)
        {
            _info.AddCallback(_callback);
        }

#if UNITY_EDITOR
        DateTime startTime;
        DateTime endTime;
#endif
        internal IEnumerator LoadingBundle()
        {
            while (!_isInit)
            {
                yield return null;
                Debug.LogError("ResourceBundleTask Loading Erroy: No Init");
            }
            Debug.Log("LoadingBundle Is " + _info.abname);
#if UNITY_EDITOR
            startTime = System.DateTime.Now;
#endif
            bool ishaveres = _manager.GetResource(_info.abname, out _resource);

            
            if (!ishaveres || !_resource.bundleIsLoad)
            {
                _info.loadingState = ELoadingState.LoadingHead;
                {
                    switch (_info.loadingType)
                    {
                        case ELoadingType.Async:
                            try
                            {
                                _loadingBundleRequest = AssetBundle.LoadFromFileAsync(_info.path);
                            }
                            catch (UnityException ex)
                            {
                                LoadingError(ex);
                            }

                            yield return _loadingBundleRequest;
                            if (_loadingBundleRequest.isDone)
                            {
                                _package = _loadingBundleRequest.assetBundle;
                            }
                            break;
                        case ELoadingType.Sync:
                            try
                            {
                                _package = AssetBundle.LoadFromFile(_info.path);
                            }
                            catch (UnityException ex)
                            {
                                LoadingError(ex);
                            }
                            break;
                    }

                }
            }
            else
            {
                _package = _resource.bundle;
            }

            List<Resource> mainDependencyRes = new List<Resource>();
            if (!ishaveres)
            {
                _info.loadingState = ELoadingState.LoadingManifest;
                {
                    List<string> dependences = BundleDependency.getInstance().GetMainDependency(_info.abname);                  
                    foreach (var dependencyName in dependences)
                    {
                        AssetBundleTaskInfo info = new AssetBundleTaskInfo(
                             dependencyName, ELoadingType.Async, ELoadingLevel.Dependency,
                             (res) =>
                              {
                                  if (res.resourceState == ResourceType.Error)
                                  {
                                      LoadingError(new Exception("Dependency:" + dependencyName + " Loading Log"));
                                  }
                                  mainDependencyRes.Add(res);
                              }
                            );
                        info.isDependency = true;
                        if (!_manager.AddLoadingTask(info))
                        {
                            LoadingError(new Exception("Loading Dependency Log " + info.abname));
                        }
                    }
                    while (true)
                    {
                        if (mainDependencyRes.Count == dependences.Count)
                        {
                            break;
                        }
                        yield return null;
                    }

                }
            }


            UnityEngine.Object[] objects = null;
            if (_package && !_package.isStreamedSceneAssetBundle)
            {
                _info.loadingState = ELoadingState.Loading;
                if (_info.isAsync)
                {
                    if (_info.isLoadingAll)
                    {
                        try
                        {
                            _loadingResourceRequest = _package.LoadAllAssetsAsync();
                        }
                        catch (UnityException ex)
                        {
                            LoadingError(ex);
                        }
                        yield return _loadingBundleRequest;
                        if (_loadingResourceRequest.isDone)
                        {
                            objects = _loadingResourceRequest.allAssets;
                        }
                    }
                    else
                    {
                        objects = new UnityEngine.Object[_info.loadingObjNames.Length];
                        for (int i = 0; i < _info.loadingObjNames.Length; i++)
                        {
                            try
                            {
                                _loadingResourceRequest = _package.LoadAssetAsync(_info.loadingObjNames[i]);
                            }
                            catch (UnityException ex)
                            {
                                LoadingError(ex);
                            }
                            yield return _loadingResourceRequest;
                            if (_loadingBundleRequest.isDone)
                            {
                                objects[i] = _loadingResourceRequest.asset;
                            }
                        }
                    }
                }
                else
                {
                    if (_info.isLoadingAll)
                    {
                        try
                        {
                            objects = _package.LoadAllAssets();
                        }
                        catch (UnityException ex)
                        {
                            LoadingError(ex);
                        }
                    }
                    else
                    {
                        objects = new UnityEngine.Object[_info.loadingObjNames.Length];
                        for (int i = 0; i < _info.loadingObjNames.Length; i++)
                        {
                            try
                            {
                                objects[i] = _package.LoadAsset(_info.loadingObjNames[i]);
                            }
                            catch (UnityException ex)
                            {
                                LoadingError(ex);
                            }
                        }
                    }
                }
            }
            else
            {
                objects = new UnityEngine.Object[0];
            }
            if (!ishaveres)
            {
                _resource = new Resource(_info.abname, _package, mainDependencyRes, _info.isDependency,false, objects);
            }
            else
            {
                _resource.bundle = _package;
                _resource.AddObj(objects);
            }
            LoadingFinsh(_resource);
        }

        

        private void LoadingFinsh(Resource _resource)
        {
#if UNITY_EDITOR
            endTime = DateTime.Now;
            var time =endTime - startTime;
            _resource.AddLoadingTime(time.Milliseconds);
#endif
            _info.loadingState = ELoadingState.Finsh;
            _info.callbackMap.ForEach(action =>
            {
                if (action != null)
                {
                    action.Invoke(_resource);
                }
            });
            _manager.LoadingFinsh(_resource,this);
        }

        private void LoadingError(Exception ex)
        {
            Debug.LogErrorFormat("[ABName:{0}] Loading Log [Log Message:{1}] [Loading State:{2}]", _info.abname, ex.Message, _info.loadingState);
            if (_info.loadingState == ELoadingState.LoadingHead)
            {
                _resource = new Resource(_info.abname, null, null, false);
            }
            _info.loadingState = ELoadingState.Error;
            _manager.LoadingFinsh(_resource, this);
        }


    }
}
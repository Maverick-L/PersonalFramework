using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using LitJson;
using System.Linq;
using System.Text;

namespace Sword.Resource
{
    [System.Serializable]
    public struct AssetBundleDependenciesInfo
    {
        public string abname;
        public List<string> manifest;
        public bool isGroup;
        public uint crc;
    }
    public class BundleDependency
    {
        public static BundleDependency _instance;
        public static BundleDependency getInstance()
        {
            if(_instance == null)
            {
                _instance = new BundleDependency();
            }
            return _instance;
        }
        private Dictionary<string, string> _dependencyPathMap = new Dictionary<string, string>();
        private Dictionary<string, List<string>> _dependencyNameMap = new Dictionary<string, List<string>>();
        private Dictionary<string, AssetBundleDependenciesInfo> _manifestInfoMap = new Dictionary<string, AssetBundleDependenciesInfo>();
        private HashSet<string> _dependencyReadMap = new HashSet<string>();
        BundleDependency()
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
                    _manifestInfoMap.Add(man.abname, man);
                }
            }

        }
        #region Get Dependency
        /// <summary>
        /// 获取依赖信息的所有的地址列表(所有依赖)
        /// </summary>
        /// <param name="abname"></param>
        /// <returns>返回一个包含地址的依赖列表</returns>
        public List<string> GetDependencyPath(string abname)
        {
            CheckDependencyTree(abname);
            List<string> path = new List<string>();
            foreach(var name in _dependencyNameMap[abname])
            {
                path.Add(_dependencyPathMap[name]); 
            }
            return path;
        }

        /// <summary>
        /// 获取AB包的地址
        /// </summary>
        /// <param name="abname"></param>
        /// <returns></returns>
        public string GetDependencyPathOnce(string abname)
        {
            CheckDependencyTree(abname);
            return _dependencyPathMap[abname];
        }

        /// <summary>
        /// 获取AB包依赖信息的名字列表（所有依赖项）
        /// </summary>
        /// <param name="abname"></param>
        /// <returns></returns>
        public List<string> GetDependencyName(string abname)
        {
            CheckDependencyTree(abname);
            Debug.LogFormat("ABname:{0} Request Dependency=======================", abname);
            int index = 1;
            foreach(var value in _dependencyNameMap[abname])
            {
                Debug.LogFormat("Request{0}:{1}", index++, value);
            }
            Debug.Log("==================================================");
            return _dependencyNameMap[abname];
        }

        /// <summary>
        /// 获取AB包依赖信息的列表（所有的依赖项）
        /// </summary>
        /// <param name="abname"></param>
        /// <returns>0：AB包名字   1：地址</returns>
        public List<List<string>> GetDependency(string abname)
        {
            CheckDependencyTree(abname);
            List<List<string>> list = new List<List<string>>();
           foreach(var name in _dependencyNameMap[abname])
            {
                List<string> _list = new List<string>();
                _list.Add(name);
                _list.Add(_dependencyPathMap[name]);
                list.Add(_list);
            }
            return list;
        }

        /// <summary>
        /// 获取AB包内引用的AB包
        /// </summary>
        /// <param name="abname"></param>
        /// <returns></returns>
        public List<string>GetMainDependency(string abname)
        {
            return _manifestInfoMap[abname].manifest;
        }

        #endregion
        private void CheckDependencyTree(string abname)
        {
            if (_dependencyReadMap.Add(abname))
            {
                CreateDependencyTree(abname);
            }
        }

        private void CreateDependencyTree(string abname)
        {
            AssetBundleDependenciesInfo? manifests =null;
            if (_manifestInfoMap.ContainsKey(abname))
            {
                manifests = _manifestInfoMap[abname];
            }
#if UNITY_EDITOR
            if(manifests == null)
            {
                Debug.LogError(abname + " not is true name");
            }
#endif
            if (!_dependencyPathMap.ContainsKey(abname))
            {
                string path = ResourceUtil.GetPathWithLocal(abname,true);
                _dependencyPathMap.Add(abname, path);
            }
            _dependencyNameMap.Add(abname, new List<string>());
            foreach (var manifest in manifests.Value.manifest)
            {
                if (!_dependencyNameMap[abname].Contains(manifest))
                {
                    _dependencyNameMap[abname].Add(manifest);
                }
                List<string> manifestes = GetDependencyName(manifest);
                for(int i = 0; i < manifestes.Count; i++)
                {
                    if (!_dependencyNameMap[abname].Contains(manifestes[i]))
                    {
                        _dependencyNameMap[abname].Add(manifestes[i]);
                    }
                }
            }
        }
    }
}

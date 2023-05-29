using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Linq;
using System.Text;
using LitJson;

namespace Sword.Resource
{
    public class ResoureceBuildConfig
    {
       public ResourceConfig[] datas;
    }

    public struct ResourceConfig
    {
        public string root;
        public bool isGroup;
        public bool canPatch;
        public string fifter;
        public bool isRecursion;
    }

    public class AssetBundleEditor : MonoBehaviour
    {
        [MenuItem("Sword/AssetBundle/Resource Build")]
        public static void LZMAZIPBuildAB(){
            InnerBundle(false,null);  
        }

        [MenuItem("Sword/AssetBundle/Resourece Patch")]
        public static void ResourcePatch()
        {
        }

        [MenuItem("Sword/Test")]
        public static void Test()
        {
            //Debug.LogError(Application.dataPath);
            //Debug.LogError(File.ReadAllText(Application.dataPath + "/AssetBundle/data/config_build.txt", Encoding.UTF8));
            var _allData = Directory.GetDirectories("Assets/AssetBundle/data/", "*.*", SearchOption.TopDirectoryOnly);
            var _allData2 = Directory.GetFiles(Application.dataPath + "/AssetBundle/data/","*.mat*", SearchOption.AllDirectories);
            //var _allData3 = Directory.GetDirectories("Assets/AssetBundle/data/","*.*")
            foreach (var path in _allData)
            {
                Debug.LogError(path);
            }
            Debug.LogError(".......");
            foreach(var path in _allData2)
            {
                Debug.LogError(path);
            }
            Debug.LogError(".......");
            foreach(var path in _allData)
            {
                Debug.LogError("GetFileName:"+Path.GetFileName(path));
                Debug.LogError("GetFileNameWithoutExtension:" + Path.GetFileNameWithoutExtension(path));
            }
        }

        public static void InnerBundle(bool isPatch,HashSet<string> patchList)
        {
            string _ABPath = ResourceUtil.ABStreamingPath;

            string conf = File.ReadAllText(Application.dataPath+"/AssetBundle/data/config_build.txt", Encoding.UTF8);
            Debug.LogError(conf);
            ResoureceBuildConfig _rootData = JsonMapper.ToObject<ResoureceBuildConfig>(conf);
            List<AssetBundleBuild> _bundlebuilds = new List<AssetBundleBuild>();
            Dictionary<string, ResourceConfig> _configMap = new Dictionary<string, ResourceConfig>();
            //清除当前已经保存的所有的AB包名
            {
                var allasset = AssetDatabase.GetAllAssetBundleNames();
                foreach (var a in allasset)
                {
                    AssetDatabase.RemoveAssetBundleName(a,true);
                }
            }

            for (int i = 0; i < _rootData.datas.Length; i++)
            {
                var _root = _rootData.datas[i];
                if (!_root.canPatch && isPatch) continue;
                var _rootPath =  "Assets/AssetBundle/data/" + _root.root+"/";
                string[] _allPath;
                if (_root.isGroup)
                {
                    _allPath = Directory.GetDirectories(_rootPath, _root.fifter, _root.isRecursion ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                }
                else
                {
                    _allPath = Directory.GetFiles(_rootPath, _root.fifter,_root.isRecursion ?SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                }

                foreach(var path in _allPath)
                {
                    if (path.EndsWith(".meta"))
                    {
                        continue;
                    }
                    var name = Path.GetFileNameWithoutExtension(path).ToLower();
                    if (_root.isGroup)
                    {
                        var subPaths = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                        var subItems = new List<string>();
                        foreach(var subPath in subPaths)
                        {
                            if (subPath.EndsWith("*.meta"))
                            {
                                continue;
                            }
                            subItems.Add(subPath);
                        }
                        _bundlebuilds.Add(new AssetBundleBuild() {
                            assetBundleName = name,
                            assetNames = subItems.ToArray(),
                            addressableNames = GetAddressableName(subItems.ToArray(), _rootPath),
                        });
                    }
                    else
                    {
                        _bundlebuilds.Add(new AssetBundleBuild()
                        {
                            assetBundleName = name,
                            assetNames = new string[] { path },
                            addressableNames = new string[] { "_" },
                        });
                    }
                    _configMap.Add(name, _root);

                }
            }

            AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(_ABPath, _bundlebuilds.ToArray(), BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows);
            string[] _allAb = manifest.GetAllAssetBundles();
            List<AssetBundleDependenciesInfo> _infos = new List<AssetBundleDependenciesInfo>(_allAb.Length);
            foreach(var ab in _allAb)
            {
                string[] _manifestPath = manifest.GetAllDependencies(ab);
                uint crc;
                bool iscrc = BuildPipeline.GetCRCForAssetBundle(ab, out crc);
                _infos.Add(new AssetBundleDependenciesInfo() {
                    abname = ab,
                    manifest = _manifestPath.ToList(),
                    crc = iscrc ? crc : 0,
                    isGroup = _configMap[ab].isGroup,
                });
            }
          
            string json = JsonMapper.ToJson(_infos);
            string jsonpath = ResourceUtil.GetDependencyPath();
            File.WriteAllText(jsonpath, json);
            Debug.Log("json:" + json);
        }

        internal static string[] GetAddressableName(string[] items,string root)
        {
            string[] outputs = new string[items.Length];
            for(int i = 0; i < items.Length; i++)
            {
                outputs[i] = items[i].Replace("\\", "/").Replace(root + "/", "");
            }
            return outputs;
        }

        internal static Dictionary<string, List<string>> GetAssetPath()
        {
            string path = Application.dataPath;
            path += "/AssetBundle/data";
            string _infoDirectionPath ="Assets/AssetBundle/data/";
            Dictionary<string, List<string>> assetDic = new Dictionary<string, List<string>>();
            if (Directory.Exists(path))
            {
                DirectoryInfo direction = new DirectoryInfo(path);
                DirectoryInfo[] directions = direction.GetDirectories();
                foreach(var dir in directions)
                {
                    List<string> names = new List<string>();
                    FileInfo[] infoes = dir.GetFiles();
                   // Debug.Log("DirectionName:" + dir.Name);
                    foreach(var info in infoes)
                    {
                        if (info.FullName.EndsWith(".meta"))
                        {
                            continue;
                        }
                        //路径拼接为Asset/AssetBundle/data/dirname/info.name
                        string infopath = _infoDirectionPath + dir.Name + '/' + info.Name;
                        names.Add(infopath);
                    //    Debug.Log("InfoName:" + infopath);
                    }
                    assetDic.Add(dir.Name, names);
                }

            }
            else
            {
                Debug.LogError("path is erroy(not in disk) =====path:" + path);
            }
            return assetDic;
        }

    }




}
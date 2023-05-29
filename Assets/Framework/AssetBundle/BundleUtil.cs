using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Sword.Resource
{
    public class ResourceUtil
    {

        public static readonly string ABStreamingPath = Application.streamingAssetsPath + "/assetsbundle";
        public static readonly string ABpersistentDataPath = Application.persistentDataPath + "/assetbundle";

        public static readonly string ABResourcePath = Application.dataPath + "/AssetBundle/data";

        public static string GetBuildAssetBundlePath(string platform)
        {
            string path = ABStreamingPath + platform;
            Debug.Log("BundlePath:" + path);
            CreateDirectory(path);
            return path;
        }

        public static string GetDependencyPath()
        {
            return GetPathWithLocal("main.s",false);
        }


        public static void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public static string GetPathWithLocal(string filename,bool isAB =true)
        {
            
            return (isAB ? ABStreamingPath : Application.streamingAssetsPath) + CheckFileName(filename);
        }

        public static string GetPathWithServer(string filename,bool isAB = true)
        {
            return (isAB ? ABpersistentDataPath : Application.persistentDataPath) + CheckFileName(filename);
            
        }

        public static string CheckFileName(string filename)
        {
            if (!filename.StartsWith("/"))
            {
                filename = "/" + filename;
            }
            return filename;
        }

        public static string GetAssetBundleName(string path)
        {
            string[] strs = path.Split('/');
            Debug.Log("BundleName:" + strs[strs.Length - 1]);
            return strs[strs.Length - 1];
        }

        public static T GetNotABJsonFile<T>(string filename) where T : struct 
        {
            T t = new T();
            var path = GetFilePath(filename, false);
            var jsonStr = File.ReadAllText(path);
            t = JsonUtility.FromJson<T>(jsonStr);
            return t;
        }
         
        public static string GetFilePath(string filename,bool isAb)
        {
            string path = GetPathWithServer(filename, false);
            if (!File.Exists(path))
            {
                path = GetPathWithLocal(filename, isAb);
            }
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("文件不存在:" + filename);
            }
            return path;
        }


        public static string MD5(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            using (var md5 = new MD5CryptoServiceProvider())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(str);
                var md5bytes = md5.ComputeHash(bytes);
                StringBuilder sb = new StringBuilder();
                for(int i = 0; i < md5bytes.Length; i++)
                {
                    sb.Append(md5bytes[i].ToString("x2"));
                }
                return sb.ToString();
            }

        }



    }
}
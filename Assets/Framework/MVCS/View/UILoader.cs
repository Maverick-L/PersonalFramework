using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
#if UNITY_EDITOR
using UnityEditor;
#endif

using System;
namespace Framework.MVC {


    /// <summary>
    /// 资源加载器
    /// </summary>
    public  class AssetLoader<T> where T:UnityEngine.Object , IDisposable
    {
        protected T _current;
        protected float _progress;
        protected string _path;
        protected bool _isDispose;
        public bool finsh
        {
            get
            {
                return progress == 100f;
            }
        }
        public float progress => _progress;
        public T Current
        {
            get
            {
                return _current;
            }
        }
        public AssetLoader(string path, bool isAsync)
        {
            _path = path;
            _progress = 0;
            if (isAsync)
            {
                OnLoaderAsync();
            }
            else
            {
                OnLoaderSync();
            }
        }
        ~AssetLoader()
        {
            Dispose(false);
        }
        /// <summary>
        /// 异步加载
        /// </summary>
        /// <param name="window"></param>
        protected virtual void OnLoaderAsync()
        {
#if UNITY_EDITOR
            OnLoaderSync();
#endif
        }

        /// <summary>
        /// 同步加载
        /// </summary>
        protected virtual void OnLoaderSync()
        {
            _progress = 100f;
#if UNITY_EDITOR
            AssetDatabase.LoadAssetAtPath<T>(_path);
#endif
        }

        public void Dispose()
        {
            Dispose(true);
        }
        protected virtual void Dispose(bool dispose =false)
        {
            if (_isDispose)
            {
                return;
            }
            _current = null;
            _isDispose = true;
        }
    }
}
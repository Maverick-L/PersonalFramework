using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sword.Resource
{
    public class ResourceAutoRecover : MonoBehaviour
    {
        private Dictionary<string,Resource> _res;
        private bool _isRetain = false;
        public int autoID = -1;

        public void AddRes(Resource res)
        {
            res.Retain();
            if (!_res.ContainsKey(res.abName))
            {
                _res.Add(res.abName, res);
            }
        }

        public void Release(string resName)
        {
            _res[resName].Release();

            _res.Remove(resName);
        }

        private void OnDestroy()
        {
            foreach(var res in _res.Values)
            {
                res.Release();
            }
            _isRetain = false;
            Debug.Log(gameObject.name + " Destory");
        }

    }
}
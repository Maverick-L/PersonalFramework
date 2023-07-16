using System.Collections;
using System.Collections.Generic;

namespace Framework.MVC
{
    public class MVCLaunch
    {
        
        public void OnInit(UnityEngine.Camera camera)
        {
            ModelManager.instance.OnInitModel();
            ViewManager.instance = camera.gameObject.GetComponent<ViewManager>();
            ViewManager.instance.OnInitlization(camera);
            BaseContraller.OnInitContraller();
        }
        public void OnDestroy()
        {
            ModelManager.instance.OnDestoryModel();
            BaseContraller.OnDestroy();
        }
        
    }

    public class MVCLaunchMono : UnityEngine.MonoBehaviour
    {
        public UnityEngine.Camera camera;
        public MVCLaunch launch;
        private void Awake()
        {
            launch = new MVCLaunch();
            launch.OnInit(camera);
            DontDestroyOnLoad(this);
        }
        
        private void OnDestroy()
        {
            launch.OnDestroy();
        }
    }
}
using System.Collections;
using System.Collections.Generic;

namespace Framework.MVC
{
    public class MVCLaunch
    {
        public static MVCLaunch instance = new MVCLaunch();

        private ModelManager _model;
        internal ModelManager model => _model;

        
        public void OnInit()
        {
            _model = new ModelManager();
            _model.OnInitModel();
        }

        
    }

    public class MVCLaunchMono : UnityEngine.MonoBehaviour
    {
        private void Awake()
        {
            MVCLaunch.instance = new MVCLaunch();
            MVCLaunch.instance.OnInit();
        }
    }
}
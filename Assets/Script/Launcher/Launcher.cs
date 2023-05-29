using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.FSM;
using UnityEngine.SceneManagement;
using Framework.FindRoad;
using Framework.Net.Server;
using Framework.Net.Client;
using UnityEditor;
using Framework.MVC;
using UnityEngine.Profiling;


public class Launcher : MonoBehaviour
{
    private int _seed;

    public static Launcher instatnce;
    public bool addWindowGUI = true;
    public bool initFFPFindRoad = false;
    public bool server;
    public bool client;

    public Camera uiCamera;
    public Canvas uiCanvas;

    private void Awake()
    {
        instatnce = this;
        _seed = 100000000;
        UnityEngine.Random.InitState(_seed);
        if (addWindowGUI)
        {
            //gameObject.AddComponent<WindowGUI>();
        }
        if (initFFPFindRoad)
        {
            Global.getFindRoadManagerInstance.InitController<FFPController>();
            GameObject ui = GameObject.Instantiate(Resources.Load<GameObject>("Floor/ViewFFP"));

            ui.transform.parent = uiCanvas.transform;
            (ui.transform as RectTransform).anchoredPosition3D = Vector3.zero;
            (ui.transform as RectTransform).sizeDelta = Vector2.zero;
            ui.transform.localScale = Vector3.one;
        }

        //测试TCP
        //GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/data/prefab/TCPExample.prefab");
        //Instantiate(go);

        //测试MVC
        //MVCLaunch.instance.OnInitlization();
        //MVCLaunch.instance.getContral.GetContraller(EContraller.MVCContrallerExample);
       // gameObject.AddComponent<MVCLaunchMono>();
       // MVCModelExample_Model2 model2 = MVCLaunch.instance.model.Get<MVCModelExample_Model2>();
       // MVCMoelExample_Model model = MVCLaunch.instance.model.Get<MVCMoelExample_Model>();

       // model2.cccc = 1111;
       // model2.dddd = 'c';
       // model2.eeee = 2222;

       // model.a = 33;
       // model.b = 'b';
       // model.c = 'c';
       // MVCLaunch.instance.model.Set<MVCModelExample_Model2>(model2);
       // MVCLaunch.instance.model.Set<MVCMoelExample_Model>(model);

       // model2.cccc = 55555;

       //Debug.LogError(MVCLaunch.instance.model.Get<MVCModelExample_Model2>().ToString());
       //Debug.LogError(MVCLaunch.instance.model.Get<MVCMoelExample_Model>().ToString());

        DontDestroyOnLoad(uiCamera);
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        if (server)
        {
            new ServerManager().Initlization();
        }
        //if (client)
        //{
        //    new NetManager().OnInitLization();
        //}


    }

    public void OnConvertToEntity()
    {
        var allManager = Util.GetAllChildClass(typeof(ManagerBase));
        for (int i = 0; i < allManager.Count; i++)
        {
            //这里等着需要用到消息系统下发所有的消息
        }

    }

    public void OnLoadScene(string scenePath)
    {
        StopCoroutine("LoadScene");
        StartCoroutine(LoadScene(scenePath));
    }

    private IEnumerator LoadScene(string scenePath)
    {
        var operation = SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);
        while (!operation.isDone)
        {
            var progress = operation.progress;
            //等待消息系统发送进度出去
        }
        yield return null;
    }

}

public class Global
{
    public static readonly string LOGIC_WORLD_NAME = "LogicWorld";
    public static readonly string RENDER_WORLD_NAME = "RenderWorld";

    private static FindRoadManager _findRoadManagerInstance;
    public static FindRoadManager getFindRoadManagerInstance
    {
        get
        {
            if (_findRoadManagerInstance == null)
            {
                _findRoadManagerInstance = Launcher.instatnce.gameObject.AddComponent<FindRoadManager>();
                _findRoadManagerInstance.Initialization();
            }
            return _findRoadManagerInstance;
        }

    }

    public static InputManager _inputManagerInstance;
    public static InputManager getInputManagerInstance
    {
        get
        {
            if (_inputManagerInstance == null)
            {
                _inputManagerInstance = Launcher.instatnce.gameObject.AddComponent<InputManager>();
                _inputManagerInstance.Initialization();
            }
            return _inputManagerInstance;
        }
    }

    public static FSMManager _fsmManagerInstance;
    public static FSMManager getFsmManagerInstance
    {
        get
        {
            if (_fsmManagerInstance == null)
            {
                _fsmManagerInstance = Launcher.instatnce.gameObject.AddComponent<FSMManager>();
                _fsmManagerInstance.Initialization();
            }
            return _fsmManagerInstance;
        }
    }
}

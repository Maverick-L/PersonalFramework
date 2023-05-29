using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Rendering;
using System.Linq;
using System;
using Framework.FSM;
using Framework.FindRoad;
public class WindowGUI : MonoBehaviour
{
    private void OnGUI()
    {
        if (GUILayout.Button("加入FloorCreateDataToEntity"))
        {
        }

        if (GUILayout.Button("加入FloorCreateSystem"))
        {

        }

        if(GUILayout.Button("ECS Renderer"))
        {
            var allSystem = DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.EntitySceneOptimizations).ToArray();
            DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(CustomBootStrap.GetWorld(Global.LOGIC_WORLD_NAME),allSystem);

            DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(CustomBootStrap.GetWorld(Global.LOGIC_WORLD_NAME),
                typeof(RenderMeshSystemV2));

        }

        if (GUILayout.Button("初始化FFPControl"))
        {
            Global.getFindRoadManagerInstance.InitController<FFPController>();
        }

        if (GUILayout.Button("创建big地形物体")){
           // FloorControllerBase control = Global.getFindRoadManagerInstance.GetControl<FloorControllerBase>();
           // control.CreateBigGameObject();
        }

        if(GUILayout.Button("Add Player Data"))
        {
            Global.getFindRoadManagerInstance.CreatePlayerData(10);
        }

        if(GUILayout.Button("Create Player "))
        {
            //StartCoroutine(Global.getFindRoadManagerInstance.CreatePlayerObject());
        }
      
        if(GUILayout.Button("Init InputFrame"))
        {
           var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Cube";
            Global.getInputManagerInstance.Initialization();
        }

        if(GUILayout.Button("Get Frame"))
        {
            MyInput input = Global.getInputManagerInstance.GetInput();
            Debug.LogError(input.click + "\t" + input.pointPos+"\t"+Global.getInputManagerInstance.inputFrameCount+"\t"+Global.getInputManagerInstance.saveFrameCount);
            Debug.LogError(Camera.main.ScreenToWorldPoint(new Vector3(input.pointPos.x,input.pointPos.y,0.5f)));
            Debug.LogError(Camera.main.WorldToScreenPoint(Camera.main.ScreenToWorldPoint(input.pointPos)));
            Ray ray = Camera.main.ScreenPointToRay(input.pointPos);
            Debug.DrawRay(ray.origin,ray.direction,Color.yellow,100000);
            GameObject.Find("Cube").transform.position = Camera.main.ScreenToWorldPoint(input.pointPos);
        }

        if(GUILayout.Button("GetFreame Console"))
        {
            Debug.LogError(Global.getInputManagerInstance.GetInput().ToString());
        }
    }

    private static void CheckToEnum<T>(int state, Type type, out T targetState) where T : System.Enum
    {
        targetState = (T)Enum.ToObject(type, state);
    }


}



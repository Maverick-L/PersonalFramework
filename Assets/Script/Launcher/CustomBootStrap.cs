using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using System.Linq;
using UnityEngine.LowLevel;
using System;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public class LogicUpdateGroup : ComponentSystemGroup { }

public class RenderUpdateGroup : ComponentSystemGroup { }



public class CustomBootStrap : ICustomBootstrap
{
    

    public bool Initialize(string defaultWorldName)
    {
        World logicWorld = new World(Global.LOGIC_WORLD_NAME);
        World renderWorld = new World(Global.RENDER_WORLD_NAME);

        //方法1：
        //var simulationSystem = logicWorld.GetOrCreateSystem<SimulationSystemGroup>();
        //LogicUpdateGroup logicGroup = logicWorld.CreateSystem<LogicUpdateGroup>();
        //System2WithLogicGroupType s1 = logicWorld.CreateSystem<System2WithLogicGroupType>();
        //System3WithLogicGroupType s2 = logicWorld.CreateSystem<System3WithLogicGroupType>();
        //var system = logicWorld.CreateSystem<FloorCreateSystem>();
        //simulationSystem.AddSystemToUpdateList(logicGroup);
        //logicGroup.AddSystemToUpdateList(system);
        //logicGroup.AddSystemToUpdateList(s2);
        //logicGroup.AddSystemToUpdateList(s1);
        //logicGroup.SortSystems();
        //simulationSystem.SortSystems();
        DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(logicWorld, GetAllSystemType(typeof(LogicUpdateGroup)));
       // ScriptBehaviourUpdateOrder.AppendSystem(logicWorld,PlayerLoop.GetCurrentPlayerLoop());

        //方法2：
        DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(renderWorld, GetAllSystemType(typeof(RenderUpdateGroup)));
      //  ScriptBehaviourUpdateOrder.UpdatePlayerLoop(renderWorld, PlayerLoop.GetCurrentPlayerLoop());

        return true;
    }

    public static World GetWorld(string worldName)
    {
        foreach(var world in World.All)
        {
            if (world.Name == worldName)
            {
                return world;
            }
        }
        return null;
    }

    public static Type[] GetAllSystemType(Type baseType)
    {
        System.Reflection.Assembly asm = System.Reflection.Assembly.GetAssembly(baseType);
        System.Reflection.PropertyInfo info = typeof(UpdateInGroupAttribute).GetProperty("GroupType");
        List<Type> allType = new List<Type>();
        foreach (var a in asm.GetExportedTypes())
        {
            var attribute = a.GetCustomAttributes(true);
            for(int i = 0; i < attribute.Length; i++)
            {
                if (attribute[i].GetType() == typeof(UpdateInGroupAttribute))
                {
                    if (((UpdateInGroupAttribute)attribute[i]).GroupType == baseType)
                    {
                        allType.Add(a);
                    }
                }
            }
            if (a == baseType)
            {
                allType.Add(a);
            }
        }
        return allType.ToArray() ;
    }
}

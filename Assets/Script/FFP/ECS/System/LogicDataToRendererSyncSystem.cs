using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Rendering;
using Unity.Entities;

[UpdateInGroup(typeof(LogicUpdateGroup), OrderLast = true)]
[UpdateAfter(typeof(EndLogicSystemEntityCommandBufferSystem))]
public class LogicDataToRendererSyncSystem : SystemBase
{
    BeginRenderSystemEntityCommandBufferSystem _renderSyscPoint;
    protected override void OnCreate()
    {
        _renderSyscPoint = CustomBootStrap.GetWorld(Global.RENDER_WORLD_NAME).GetOrCreateSystem<BeginRenderSystemEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {

        //var renderCommandBuffer = _renderSyscPoint.CreateCommandBuffer();
        ////将Floor信息更新到渲染世界中去
        //Entities.WithChangeFilter<CellData>().ForEach((in CellData floorInfo) => {


        //});
    }
}

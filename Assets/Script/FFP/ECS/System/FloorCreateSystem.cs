using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(LogicUpdateGroup),OrderFirst = true)]
public class BeginLogicSystemEntityCommandBufferSystem : EntityCommandBufferSystem
{

}

[UpdateInGroup(typeof(LogicUpdateGroup), OrderLast  = true)]
public class EndLogicSystemEntityCommandBufferSystem : EntityCommandBufferSystem
{

}

[UpdateInGroup(typeof(RenderUpdateGroup), OrderFirst = true)]
public class BeginRenderSystemEntityCommandBufferSystem : EntityCommandBufferSystem
{
}

[UpdateInGroup(typeof(RenderUpdateGroup), OrderLast = true)]
public class EndRenderSystemEntityCommandBufferSystem : EntityCommandBufferSystem
{

}






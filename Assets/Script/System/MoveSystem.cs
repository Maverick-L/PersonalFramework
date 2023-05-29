using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.FSM;
using Framework.FindRoad;
using Unity.Mathematics;
public static class MoveSystem 
{

    public static void Move(ControllerType type,FloorPlayer player,Floor nowfloor,Floor nextfloor)
    {
        PlayerData data = player.getPlayerData;
        Vector3 dir = (nextfloor.getFloorData.aabb.Center - data.pos);
        Floor targetFloor = Global.getFindRoadManagerInstance.GetFloor(type, data.bigFloorTarget, data.floorTarget);
        float dis = Vector3.Distance(player.getPlayerData.pos, nextfloor.getFloorData.aabb.Center);
        float speed = MoveSpeed(player.getType, nowfloor, data.normalSpeed);
        var newPos = data.pos + new float3(dir.normalized * speed * Time.deltaTime);
        if(nextfloor == nowfloor && MoveEnd(player,nextfloor))
        {
            player.UpdatePos(nextfloor.getFloorData.aabb.Center);
            Debug.LogError(nextfloor.getFloorData.index + "\t" + targetFloor.getFloorData.index);
            if(nextfloor == targetFloor)
            {
                player.SetNextState(EFloorPlayerState.Idel);
                Debug.LogError("Move To End:" + player.GetControlName());
            }
            return;
        }
        if (nextfloor.CheckPoint(newPos))
        {
            player.RefreshPlayerInGrid(nextfloor.getFloorData.bigCellIndex, nextfloor.getFloorData.cellIndex);
        }
        player.UpdatePos(newPos);
    }

    public static bool MoveEnd(FloorPlayer player,Floor targetFloor)
    {
        float dis = Vector3.Distance(player.getPlayerData.pos, targetFloor.getFloorData.aabb.Center);
        return dis < 0.05f;
    }

    public static float MoveSpeed(ControllerType type,Floor floor,float normalSpeed)
    {
        return normalSpeed - (((int)floor.getFloorData.cellType >> 1) * 10 / (int)ECellMatType.Max);

    }

}

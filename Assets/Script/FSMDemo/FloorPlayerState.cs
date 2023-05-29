using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.FSM;
using Framework.FindRoad;
using Unity.Mathematics;

public enum EFloorPlayerState
{
    Idel =0,
    Move,
    FindRoadMove,
    Wait,
    
}

[StateFuncAttrubite(typeof(EFloorPlayerState), (int)EFloorPlayerState.Idel)]
public class FloorPlayerStateIdel : StateBase<EFloorPlayerState>
{

    public FloorPlayerStateIdel(StateController<EFloorPlayerState> control) : base(control) { }

    public override void OnEnter()
    {
        FloorPlayer player = _stateControl.getMainBody as FloorPlayer;
        player.SettingPlayerNextGrid(-1, -1);
    }

    public override void OnExit()
    {
    }

    public override void OnUpdate()
    {
    }     
}


[StateFuncAttrubite(typeof(EFloorPlayerState), (int)EFloorPlayerState.Wait)]
public class FloorPlayerStateWait : StateBase<EFloorPlayerState>
{

    public FloorPlayerStateWait(StateController<EFloorPlayerState> control) : base(control) { }

    public override void OnEnter()
    {
        FloorPlayer player = _stateControl.getMainBody as FloorPlayer;
        player.SettingPlayerNextGrid(-1, -1);
    }

    public override void OnExit()
    {
    }

    public override void OnUpdate()
    {
    }
}



[StateFuncAttrubite(typeof(EFloorPlayerState), (int)EFloorPlayerState.Move)]
public class FloorPlayerStateMove : StateBase<EFloorPlayerState>
{

    private Floor _nextTarget;
    public FloorPlayerStateMove(StateController<EFloorPlayerState> control) : base(control) { }

    public override void OnEnter()
    {


    }

    public override void OnExit()
    {
        _nextTarget = null;
    }

    public override void OnUpdate()
    {
        FloorPlayer player = _stateControl.getMainBody as FloorPlayer;
        PlayerData data = player.getPlayerData;
        Floor nowFloor = Global.getFindRoadManagerInstance.GetFloor(player.getType, data.bigFloorIndex, data.floorIndex);
        Floor targetFloor = Global.getFindRoadManagerInstance.GetFloor(player.getType, data.bigFloorTarget, data.floorTarget);
        if (targetFloor != _nextTarget && !Global.getFindRoadManagerInstance.CheckFloorCanCross(player.getType, data.bigFloorTarget, data.floorTarget, true))
        {
            player.SetNextState(EFloorPlayerState.Wait);
            return;
        }
        //确定下一个目标点
        if(nowFloor == targetFloor)
        {
            MoveSystem.Move(player.getType, player, nowFloor, targetFloor);

            return;
        }
        if (_nextTarget == null || (nowFloor == _nextTarget && MoveSystem.MoveEnd(player,nowFloor)))
        {
            bool canCross = Global.getFindRoadManagerInstance.GetNextFloor(player.getType, data.bigFloorIndex, data.floorIndex, data.bigFloorTarget, data.floorTarget, out _nextTarget);
            if (!canCross)
            {
                Floor nextFloor = Global.getFindRoadManagerInstance.GetGridWithTargetArea(player.getType, data.bigFloorIndex, data.floorIndex, data.bigFloorIndex, data.floorIndex,1);
                if(nextFloor != null)
                {
                    _nextTarget = nextFloor;
                }
                else
                {
                    player.SetNextState(EFloorPlayerState.Wait);
                    _nextTarget = null;
                    return;
                }
            }
            player.SettingPlayerNextGrid(_nextTarget.getFloorData.bigCellIndex, _nextTarget.getFloorData.cellIndex);
        }

        MoveSystem.Move(player.getType, player, nowFloor, _nextTarget);


    }
}

[StateFuncAttrubite(typeof(EFloorPlayerState), (int)EFloorPlayerState.FindRoadMove,(int)EFloorPlayerState.Move)]
public class FloorPlayerStateFindRoadMove : StateBase<EFloorPlayerState>
{
    public FloorPlayerStateFindRoadMove(StateController<EFloorPlayerState> control) : base(control) { }

    public override void OnEnter()
    {

    }

    public override void OnExit()
    {

    }

    public override void OnUpdate()
    {
        FloorPlayer player = _stateControl.getMainBody as FloorPlayer;
        PlayerData data = player.getPlayerData;
        //获取下一个节点的位置
        Floor nextFloor;
        Floor nowFloor = Global.getFindRoadManagerInstance.GetFloor(player.getType, data.bigFloorIndex, data.floorIndex);
        Global.getFindRoadManagerInstance.GetNextFloor(player.getType, data.bigFloorIndex, data.floorIndex,out nextFloor);
        player.SettingPlayerNextGrid(nextFloor.getFloorData.bigCellIndex, nextFloor.getFloorData.cellIndex);
        MoveSystem.Move(player.getType, player, nowFloor, nextFloor);
    }
}
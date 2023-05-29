using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Framework.FindRoad;
using Framework.FSM;


public class FloorPlayerController : ControlBase
{
    private List<FloorPlayer> _playerMap;
    public List<FloorPlayer> getPlayerMap => _playerMap;

    private FindRoadManager _manager;
    private HashSet<int> _nextTargetMap = new HashSet<int>();
    private ControllerType _type;
    private int _probability = 10;//概率，有多少概率会让这个角色处于散步寻路状态
    private float _probailityCD = 5f;
    private float _nowProbaility = 0f;
    public FloorPlayerController(ControllerType type)
    {
        _type = type;
    }

    public override void OnDestoryControl()
    {
        _manager = null;
        _playerMap.Clear();
    }

    public override void OnInitlization()
    {
        _manager = Global.getFindRoadManagerInstance;
        _playerMap = new List<FloorPlayer>();
    }

    public override void OnLogicUpdate()
    {
        var input = _manager.getFrameInput;
        if(input.createPlayerCount != 0)
        {
            for(int i = 0; i < input.createPlayerCount; i++)
            {
                CreatePlayer();
            }
        }
        bool isRandomMove = true;
        List<FloorPlayer> waitlist = new List<FloorPlayer>();
        for(int i = 0; i < _playerMap.Count; i++)
        {
            var nextState = _playerMap[i].GetNextState();

            var nowState = _playerMap[i].GetNowState();
            if(nextState != nowState)
            {
                Global.getFsmManagerInstance.TryStateChange<EFloorPlayerState>(_playerMap[i].GetControlName(), nextState);
            }
            if(_playerMap[i].GetNowState() != EFloorPlayerState.Idel)
            {
                isRandomMove = false;
            }
            if(_playerMap[i].GetNowState() == EFloorPlayerState.Wait )
            {
                waitlist.Add(_playerMap[i]);
            }
        }
        if (isRandomMove)
        {
            _manager.GetControl(_type).SetTargetIndex(null);
            _nowProbaility += Time.deltaTime;
            if(_nowProbaility >=_probailityCD && Random.Range(0, 100) < _probability)
            {

                Floor randomFloor = _manager.GetRandomFloor(_type);
                for (int i = 0; i < _playerMap.Count; i++)
                {
                    _playerMap[i].SettingPlayerTarget(randomFloor.getFloorData.bigCellIndex, randomFloor.getFloorData.cellIndex);
                    _playerMap[i].SetNextState(EFloorPlayerState.Move);
                }
                _nowProbaility = 0;
                _nextTargetMap.Clear();
                _manager.GetControl(_type).SetTargetIndex(randomFloor);
            }
        }
        if(waitlist.Count != 0 && waitlist[0].GetLastState() == EFloorPlayerState.Move)
        {
            if(_nowProbaility >= _probailityCD)
            {
                Floor randomFloor = _manager.GetRandomFloor(_type);
                for (int i = 0; i < waitlist.Count; i++)
                {
                    waitlist[i].SettingPlayerTarget(randomFloor.getFloorData.bigCellIndex, randomFloor.getFloorData.cellIndex);
                    waitlist[i].SetNextState(EFloorPlayerState.Move);
                }
                _manager.GetControl(_type).SetTargetIndex(randomFloor);
                _nowProbaility = 0;
            }
            _nowProbaility += Time.deltaTime;
          

        }
    }

    public override void OnRenderUpdate()
    {
        Color start = Color.white;
        Color end = Color.black;
        float lerp = 0f;
        for(int i = 0; i < _playerMap.Count; i++)
        {

            if(_playerMap[i].getPlayerObject == null)
            {
                _manager.StartCoroutine(_playerMap[i].CreateGameObject());
            }
            else
            {
                _playerMap[i].getPlayerObject.transform.position = _playerMap[i].getPlayerData.pos;
                if (_playerMap[i].GetNowState() == EFloorPlayerState.FindRoadMove || _playerMap[i].GetNowState() == EFloorPlayerState.Move)
                {
                    DrawLine(_playerMap[i].getPlayerObject.transform, _playerMap[i]);
                    lerp += 0.1f;

                }
            }

        }
    }

    private void DrawLine(Transform trans,FloorPlayer player)
    {
        LineRenderer line = trans.Find("LineRenderer").GetComponent<LineRenderer>(); 
        List<Vector3> pointList = new List<Vector3>();
        Floor targetFloor = _manager.GetFloor(_type, player.getPlayerData.bigFloorTarget, player.getPlayerData.floorTarget);
        Floor nextFloor = null;
        bool isCross = _manager.GetControl(_type).TryGetPlayerNextTarget(player, out nextFloor);
        pointList.Add(trans.position);
        while (nextFloor != targetFloor && nextFloor != null )
        {
            pointList.Add(nextFloor.getFloorData.aabb.Center);
             _manager.GetNextFloor(_type, nextFloor.getFloorData.bigCellIndex, nextFloor.getFloorData.cellIndex,out nextFloor);
        }
        if(nextFloor != null)
        {
            pointList.Add(nextFloor.getFloorData.aabb.Center);
        }
        line.positionCount = 0;
        line.positionCount = pointList.Count;
        
        for(int i = 0; i < pointList.Count; i++)
        {
            pointList[i] += new Vector3(0f,1f,0f);
        }

        line.SetPositions(pointList.ToArray());
    }
    #region Logic
    /// <summary>
    /// 点击到地块，开始移动所有的玩家
    /// </summary>
    /// <param name="target"></param>
    public void StartMove(Floor target)
    {
        for(int i = 0; i < _playerMap.Count; i++)
        {
            _playerMap[i].SetNextState(EFloorPlayerState.FindRoadMove);
            _playerMap[i].SettingPlayerTarget(target.getFloorData.bigCellIndex, target.getFloorData.cellIndex);
        }
    }

    private void CreatePlayer()
    {
        var ffpData = _manager.GetRandomFloor(_type);
        var playerData = new PlayerData();
        playerData.bigFloorIndex = ffpData.getFloorData.bigCellIndex;
        playerData.floorIndex = ffpData.getFloorData.cellIndex;
        playerData.playerIndex = _playerMap.Count;
        playerData.normalSpeed = 1;
        playerData.pos = ffpData.getFloorData.aabb.Center;
        var player = new FloorPlayer(playerData,this,_type);
        _playerMap.Add(player);
        Global.getFsmManagerInstance.OnInitFSMController<EFloorPlayerState>(player,true);
    }
    

    /// <summary>
    /// 是否所有的Player都已经到了目标位置
    /// </summary>
    /// <returns></returns>
    public bool FindRoadFinsh()
    {
        for(int i = 0; i < getPlayerMap.Count; i++)
        {
            if(getPlayerMap[i].GetNowState() == EFloorPlayerState.FindRoadMove)
            {
                return false;
            }
        }
        return true;
    }

    public int[][] GetPlayerFloorIndex()
    {
        int[][] map = new int[getPlayerMap.Count][];
        for(int i = 0; i < getPlayerMap.Count; i++)
        {
            map[i] = new int[2];
            map[i][0] = getPlayerMap[i].getPlayerData.bigFloorIndex;
            map[i][1] = getPlayerMap[i].getPlayerData.floorIndex;
        }
        return map;
    }


    #endregion


    #region Render

    #endregion
}

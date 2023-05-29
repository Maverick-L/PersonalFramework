using Framework.FSM;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Framework.FindRoad;
using Unity.Mathematics;

public class FindRoadControl:ControlBase
{
    private ControllerType _controlType;
    private FloorPlayerController _playerControl;
    private FloorControllerBase _floorControl;

    public ControllerType getControlType => _controlType;
    public FloorPlayerController getPlayerControl => _playerControl;
    public FloorControllerBase getFloorControl => _floorControl;

    private Dictionary<string, int> _nextTargetMap = new Dictionary<string, int>();
    private int _targetFloorIndex = -1;
    public FindRoadControl(ControllerType type,FloorPlayerController playerControl,FloorControllerBase floorControl)
    {
        _controlType = type;
        _playerControl = playerControl;
        _floorControl = floorControl;
        _targetFloorIndex = -1;
    }

    public override void OnInitlization()
    {
        _playerControl.OnInitlization();
        _floorControl.OnInitlization();
        _floorControl.clickPoint += ClickFloor;

    }

    public override void OnDestoryControl()
    {
        _playerControl = null;
        _floorControl = null;
        _floorControl.clickPoint -= ClickFloor;

    }

    public override void OnLogicUpdate()
    {
        _floorControl.OnLogicUpdate();
        _playerControl.OnLogicUpdate();
    }

    public override void OnRenderUpdate()
    {
        _floorControl.OnRenderUpdate();
        _playerControl.OnRenderUpdate();
    }

    #region 接口

    private void ClickFloor(Floor floor)
    {
        _playerControl.StartMove(floor);
        SetTargetIndex(floor);
    }

    public void SettingNextTarget(FloorPlayer player,Floor nextFloor)
    {
        var name = player.GetControlName();
        if (!_nextTargetMap.ContainsKey(name))
        {
            _nextTargetMap.Add(name, -1);
        }
        int lastIndex = _nextTargetMap[name];
        if(lastIndex != -1)
        {
            Floor lastFloor = _floorControl.GetFloor(lastIndex);
            lastFloor.SetCross(true);
        }
        _nextTargetMap[name] = nextFloor == null ? -1 : nextFloor.getFloorData.index;
        nextFloor?.SetCross(false);
    }
   

    public List<Floor> GetAllNextList(FloorPlayer player = null)
    {
        List<Floor> list = new List<Floor>();
        foreach(var next in _nextTargetMap)
        {
            var key = next.Key;
            var value = next.Value;
            if(value != -1 || (player != null && player.GetControlName() != key))
            {
                list.Add(_floorControl.GetFloor(value));
            }
        }
        return list;
    }
    public bool TryGetPlayerNextTarget(FloorPlayer player,out Floor nextFloor)
    {
        var name = player.GetControlName();
        nextFloor = null;
        if (_nextTargetMap.ContainsKey(name) && _nextTargetMap[name] != -1)
        {
            nextFloor = _floorControl.GetFloor(_nextTargetMap[name]);
            return true;
        }
        return false;
    }

    public void SetTargetIndex(Floor floor)
    {
        _targetFloorIndex = floor == null ? -1 : floor.getFloorData.index;
    }

    public Floor GetTargetFloor()
    {
        if(_targetFloorIndex == -1)
        {
            return null;
        }
        return _floorControl.GetFloor(_targetFloorIndex);
    }

    #endregion
    
}

public class FindRoadManager : ManagerBase
{
    private Dictionary<ControllerType, FindRoadControl>  _playerContrlMap;
    
    public Camera getRenderCamera
    {
        get
        {
            return Camera.main;
        }
    }

    public Transform getPlayerRoot
    {
        get {
            if (_managerRoot == null)
            {
                _managerRoot = new GameObject("PlayerRoot").transform;
                _managerRoot.position = Vector3.zero;
            }
            return _managerRoot;
        }
    }

    private MyInput _input;
    public MyInput getFrameInput => _input;

    public override void Initialization()
    {
        isInit = true;
        _controlList = new List<ControlBase>();
        _playerContrlMap = new Dictionary<ControllerType, FindRoadControl>();
    }


    public void InitController<TControl>() where TControl :FloorControllerBase
    {
        var type =Util.GetControllerType<TControl>();
        InitController(type);
    }

    public void InitController(ControllerType type)
    {
        ControlBase controller = Util.GetControlInstanveWithAttrubate(type);
        if (controller != null)
        {
            var control = new FindRoadControl(type,new FloorPlayerController(type),controller as FloorControllerBase);
            AddControl(control);
            StartCoroutine(AsynInitlization(controller));
            _playerContrlMap.Add(type, control);
        }
        else
        {
            Debug.LogError("实例化失败：" + type);
        }
    }

    protected override void OnLogicUpdate()
    {
        _input = Global.getInputManagerInstance.GetInput();
        base.OnLogicUpdate();
    }

    public void CreatePlayerData(int count)
    {
        Global.getInputManagerInstance.createPlayerCount = count;
    }

    private IEnumerator AsynInitlization(ControlBase control)
    {
        yield return control.AsynInitlization();
    }


    public override void OnDestoryManager()
    {
        _playerContrlMap.Clear();
    }




    #region 对外接口
    public void OnShowCoordinate(bool enable)
    {
        StopCoroutine("CoShowCordinate");
        StartCoroutine(CoShowCordinate(enable));
    }

    private IEnumerator CoShowCordinate(bool enable)
    {
        for (int i = 0; i < _controlList.Count; i++)
        {
            if(_controlList[i] is FindRoadControl)
            {
                yield return (_controlList[i] as FindRoadControl).getFloorControl.FloorCoordinate(enable);
            }
        }
    }

    public FindRoadControl GetControl(ControllerType type)
    {
        if (_playerContrlMap.ContainsKey(type))
        {
            return _playerContrlMap[type];
        }
        return null;
    }

    public Floor GetRandomFloor(ControllerType type)
    {
        return GetControl(type).getFloorControl.GetRandomCanCrossPoint();
    }

    public Floor GetFloor(ControllerType type,int bigIndex, int index)
    {
        return GetControl(type).getFloorControl.GetFloor(bigIndex, index);
    }

    public TFloor GetFloor<TFloor>(ControllerType type,int bigIndex, int index) where TFloor : Floor
    {
        return GetControl(type).getFloorControl.GetFloor(bigIndex, index) as TFloor;
    }

    /// <summary>
    /// 获取所有的角色所在的地块
    /// </summary>
    /// <returns></returns>
    public int[][] GetAllPlayerFloor(ControllerType type)
    {
        var control = GetControl(type);
        if (control!=null)
        {
            return control.getPlayerControl.GetPlayerFloorIndex();
        }
        return new int[0][];
    }

    /// <summary>
    /// 获取移动的下一个目标
    /// </summary>
    /// <returns></returns>
    public bool GetNextFloor(ControllerType type,int nowBigIndex,int nowIndex,out Floor nextFloor)
    {

        return GetControl(type).getFloorControl.GetNextFloor(nowBigIndex, nowIndex, true, out nextFloor);
    }

    public bool GetNextFloor(ControllerType type,int nowBigIndex,int nowIndex,int targetBigIndex,int targetIndex, out Floor nextFloor)
    {
        return GetControl(type).getFloorControl.GetNextFloor(nowBigIndex, nowIndex, false, out nextFloor, targetBigIndex,targetIndex);
    }

    public Floor GetGridWithTargetArea(ControllerType type,int nowBigIndex,int nowIndex,int targetBigIndex,int targetIndex,int checkArea =-1)
    {
        return GetControl(type).getFloorControl.GetGridWithTargetArea(nowBigIndex, nowIndex, targetBigIndex, targetIndex,checkArea);
    }

    /// <summary>
    /// 检测地块是否可以通过
    /// </summary>
    /// <param name="type"></param>
    /// <param name="bigIndex"></param>
    /// <param name="index"></param>
    /// <param name="checkPlayerFloor"></param>
    /// <returns></returns>
   public bool CheckFloorCanCross(ControllerType type,int bigIndex,int index,bool checkPlayerFloor)
    {
        return GetControl(type).getFloorControl.CheckFloorCanCross(bigIndex, index, checkPlayerFloor);
    }

    #endregion
}

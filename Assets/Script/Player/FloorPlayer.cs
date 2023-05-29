using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Framework.FSM;
using Framework.FindRoad;
using Unity.Mathematics;
public class FloorPlayer : IStateMainBody<EFloorPlayerState>
{
    private GameObject _gameObject;
    public GameObject getPlayerObject => _gameObject;

    private PlayerData _playerData;
    public PlayerData getPlayerData =>  _playerData;

    private FloorPlayerController _control;

    public FloorPlayerController getPlayerControl => _control;

    private ControllerType _type;
    public ControllerType getType =>_type;
    public  FloorPlayer(PlayerData playerData,FloorPlayerController control,ControllerType type)
    {
        _playerData = playerData;
        _control = control;
        _type = type;
        Debug.LogError("init playerData:" + playerData.bigFloorIndex + "\t" + playerData.floorIndex);
    }

    public IEnumerator CreateGameObject()
    {
        _gameObject = new GameObject();
        _gameObject.name = "Player_" + _playerData.playerIndex;
        _gameObject.transform.position = _playerData.pos;
        _gameObject.transform.SetParent(Global.getFindRoadManagerInstance.getPlayerRoot);
        GameObject linrender = new GameObject("LineRenderer");
        linrender.transform.SetParent(_gameObject.transform);
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.transform.SetParent(_gameObject.transform);
        player.name = "Root";
        player.transform.localPosition = Vector3.zero + Vector3.up;
        var line = linrender.AddComponent<LineRenderer>();
        linrender.transform.localPosition = Vector3.zero;
        line.material = new Material(Shader.Find("Standard"));
        line.startWidth = 0.2f;
        line.endWidth = 0.2f;
        line.material.color = new Color(1, UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
        player.GetComponent<MeshRenderer>().material.color = line.material.color;
        yield return null;
    }

    public void SettingPlayerTarget(int bigFloor,int floor)
    {
        _playerData.bigFloorTarget = bigFloor;
        _playerData.floorTarget = floor;
        Debug.LogError("Settting Target:" + this.GetControlName() + "\t" + bigFloor + "\t" + floor);
    }

    public void SettingPlayerNextGrid(int bigIndex,int index)
    {
        _playerData.nextBigFloorIndex = bigIndex;
        _playerData.nextFloorIndex = index;
        Floor nextFloor = bigIndex == -1 ? null : Global.getFindRoadManagerInstance.GetFloor(_type, bigIndex, index);
        Global.getFindRoadManagerInstance.GetControl(_type).SettingNextTarget(this, nextFloor);
    }
    /// <summary>
    /// 设定当前角色所在的网格
    /// </summary>
    /// <param name="bigfloor"></param>
    /// <param name="floor"></param>
    public void RefreshPlayerInGrid(int bigfloor,int floor)
    {
        _playerData.bigFloorIndex = bigfloor;
        _playerData.floorIndex = floor;
    }

    public void UpdatePos(float3 pos)
    {
        _playerData.pos = pos;
    }

    public void Destory()
    {
        if(_gameObject != null)
        {
            GameObject.Destroy(_gameObject);
        }
        
    }

    #region FSM
    public void SetNextState(EFloorPlayerState state)
    {
        Debug.LogError(GetControlName() + "------->" + state);
        _playerData.nextState = state;
    }

    public void SetLastState(EFloorPlayerState state)
    {
        _playerData.lastState = state;
    }

    public void SetNowState(EFloorPlayerState state)
    {
        _playerData.nowState = state;
    }
    public string GetControlName()
    {
        return "Player_" + _playerData.playerIndex;
    }

    public EFloorPlayerState GetNextState()
    {
        return _playerData.nextState;
    }


    public EFloorPlayerState GetNowState()
    {
        return _playerData.nowState;

    }

    public EFloorPlayerState GetLastState()
    {
        return _playerData.lastState;

    }
    #endregion
}

public struct PlayerData : IComponentData
{
    public int bigFloorIndex;
    public int floorIndex;
    public int bigFloorTarget;
    public int floorTarget;
    public int playerIndex;
    public int nextBigFloorIndex;
    public int nextFloorIndex;
    public EFloorPlayerState nextState;
    public EFloorPlayerState lastState;
    public EFloorPlayerState nowState;
    public float3 pos;
    public float normalSpeed;
}

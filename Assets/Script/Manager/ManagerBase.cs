using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public abstract class ManagerBase : MonoBehaviour 
{
    protected bool isInit;
    public bool IsInit { get { return isInit; } }
    protected Transform _managerRoot;
    protected List<ControlBase> _controlList;


    public abstract void Initialization();
    public virtual void OnDestoryManager()
    {
        for(int i = 0; i < _controlList.Count; i++)
        {
            _controlList[i].OnDestoryControl();
        }
        _controlList = null;
        GC.Collect();
    }

    public virtual void ConvertToEntity() {
        if(_managerRoot  != null){
            GameObject.DestroyImmediate(_managerRoot);
        }
        OnDestoryManager();
    }

    public virtual void FixedUpdate()
    {
        OnLogicUpdate();
    }

    protected virtual void OnLogicUpdate()
    {
        if (_controlList != null)
        {
            for (int i = 0; i < _controlList.Count; i++)
            {
                _controlList[i].OnLogicUpdate();
            }
        }
    }

    public virtual void Update()
    {
        OnRenderUpdate();
    }

    protected virtual void OnRenderUpdate()
    {
        if (_controlList != null)
        {
            for (int i = 0; i < _controlList.Count; i++)
            {
                _controlList[i].OnRenderUpdate();
            }
        }

    }
    public virtual void OnDisable()
    {
        OnDestoryManager();
    }

    protected void AddControl(ControlBase control)
    {
        if(_controlList == null)
        {
            _controlList = new List<ControlBase>();
        }
        _controlList.Add(control);
        control.OnInitlization();

    }

    protected string GetError(string value)
    {
        
        return string.Format("[Manager Log]:%s--->%s", this.name, value);
    }

}

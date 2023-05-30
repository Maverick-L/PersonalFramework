using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine;
using System.Text;
using System.IO;
public abstract class BaseGeneratorEditor
{
    protected GameObject _myObj;
    protected StreamWriter _write;
    protected string _tab = "    ";
    protected string _findPath = "";
    public abstract bool Check();

    public virtual void OnInit(GameObject go,StreamWriter write)
    {
        
        _myObj = go;
        _write = write;
        Stack<string> pathStack = new Stack<string>();
        Transform next = go.transform;
        while(next != _myObj.transform.root)
        {
            pathStack.Push(next.name);
            next = next.parent;
        }
        while(pathStack.Count > 0)
        {
            _findPath += (pathStack.Count > 1 ? "/" : "") + pathStack.Pop();
        }
    }
    /// <summary>
    /// 写入声明
    /// </summary>
    public abstract void WriteField(string tab);

    /// <summary>
    /// 写入引用
    /// </summary>
    public virtual void WriteUsing() { }
    /// <summary>
    /// 写入方法
    /// </summary>
    public abstract void WriteMethod(string tab);

    /// <summary>
    /// 写入赋值，在Awake的时候调用
    /// </summary>
    /// <param name="tab"></param>
    public abstract void WriteValuation(string tab);

    public bool TryGetContraller<TContraller>(out TContraller value) where TContraller :MonoBehaviour
    {
        value = _myObj.GetComponent<TContraller>();
        return value == null;
    }
}

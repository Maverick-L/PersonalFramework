using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine;
using System.Text;
using System.IO;
using System.Linq;
using System.Reflection;

public abstract class BaseGeneratorEditor
{
    protected GameObject _myObj;
    protected StreamWriter _write;
    protected string _tab = "    ";
    protected string _findPath = "";
    /// <summary>
    /// 处理成首字母小写的名字
    /// </summary>
    protected string _fieldName = "";
    /// <summary>
    /// 处理成首字母大写的名字
    /// </summary>
    protected string _fieldFuncName = "";
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
            _findPath += pathStack.Pop() + (pathStack.Count > 0 ? "/" : "");
        }
        SetFieldName();
    }

    /// <summary>
    /// 处理声明名字
    /// </summary>
    /// <param name="name"></param>
    protected virtual void SetFieldName()
    {
        string name =_myObj.name+this.GetType().GetCustomAttribute<GeneratorWindowAttrubity>().uGUiType.Name;
        char onec = name.ToCharArray()[0];
        var len = name.Length;
        string tempName = name.Substring(1, len -1);
        _fieldName = char.ToLower(onec) + tempName;
        _fieldFuncName = char.ToUpper(onec) + tempName;
    }
    /// <summary>
    /// 写入声明
    /// </summary>
    public abstract void WriteField(string tab);

    /// <summary>
    /// 写入引用
    /// </summary>
    public virtual void WriteUsing(string tab) { }
    /// <summary>
    /// 写入方法
    /// </summary>
    public abstract void WriteMethod(string tab);

    /// <summary>
    /// 写入赋值，在Awake的时候调用
    /// </summary>
    /// <param name="tab"></param>
    public abstract void WriteAwake(string tab);

    /// <summary>
    /// 写入摧毁需要处理的内容
    /// </summary>
    /// <param name="tab"></param>
    public virtual void WriteOnDestroy(string tab) { }
    /// <summary>
    /// onEnable事件
    /// </summary>
    /// <param name="tab"></param>
    public virtual void WriteOnEnable(string tab) { }
    /// <summary>
    /// onDisable事件
    /// </summary>
    /// <param name="tab"></param>
    public virtual void WriteOnDisable(string tab) { }
    /// <summary>
    /// onStart事件
    /// </summary>
    /// <param name="tab"></param>
    public virtual void WriteStart(string tab) { }

    /// <summary>
    /// onUpdate事件
    /// </summary>
    /// <param name="tab"></param>
    public virtual void WriteUpdate(string tab) { }

    /// <summary>
    /// 输入换行
    /// </summary>
    protected void MoveToNext()
    {
        _write.WriteLine();
    }
    public bool TryGetContraller<TContraller>(out TContraller value) where TContraller :MonoBehaviour
    {
        value = _myObj.GetComponent<TContraller>();
        return value != null;
    }

    public bool TryGetContraller<TContraller>() where TContraller : MonoBehaviour
    {
        return _myObj.GetComponent<TContraller>() != null;
    }
}

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine;
using System.Text;
using System.IO;
public abstract class BaseCreateEditor
{
    protected GameObject _myObj;
    protected StreamWriter _write;
    protected static string _tab = "";
    public BaseCreateEditor(GameObject go,StreamWriter write)
    {
        _myObj = go;
        _write = write;
    }

    public void SetStart()
    {
        _write.Flush();
        _write.WriteLine($"using UnityEngine");
        _write.WriteLine($"using UnityEngine.UI");
        _write.WriteLine($"using System");
        _write.WriteLine($"using System.Collections");
        _write.WriteLine($"using System.Collections.Generic");
        SetClass();
    }

    public void  SetClass()
    {
        _write.WriteLine($"public class {_myObj.transform.root.name} : Framework.MVC.BaseViewWindow");
        _write.WriteLine(@"{");
        _tab += "    ";
    }

    public void SetEnd()
    {
        _write.WriteLine(@"}");
    }



    public bool TryGetContraller<TContraller>(out TContraller value) where TContraller :MonoBehaviour
    {
        value = _myObj.GetComponent<TContraller>();
        return value == null;
    }
    public abstract bool Check();
    /// <summary>
    /// 写入变量
    /// </summary>
    public abstract void WriteField();
    /// <summary>
    /// 写入方法
    /// </summary>
    public abstract void WriteMethod();
    /// <summary>
    /// 写入赋值
    /// </summary>
    public abstract void WriteAssignment();
}

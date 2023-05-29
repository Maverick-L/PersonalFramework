using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.MVC;
using Framework;
using System;
#if UNITY_EDITOR
[GeneratorAttrubity("测试MVC-Model")]
#endif
public struct MVCMoelExample_Model : IBaseModelStruct
{
    public int a;
    public char b;
    public char c;

    public override string ToString()
    {
       return ($"a:{a}---b:{b}---c:{c}");

    }
    public void SendMessage()
    {
    }
}

public struct MVCModelExample_Model2 : IBaseModelStruct
{
    public int cccc;
    public char dddd;
    public int eeee;

     public override string ToString()
    {
       return ($"cccc:{cccc}---dddd:{dddd}---eeee:{eeee}");

    }
    public void SendMessage()
    {
    }
}

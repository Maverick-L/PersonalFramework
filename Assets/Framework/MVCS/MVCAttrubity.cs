using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// 使用此方法，用来标记生成出来的内容的描述
/// </summary>
public class GeneratorAttrubity : System.Attribute
{
    public string des;
    public GeneratorAttrubity(string des)
    {
        this.des = des;
    }
}


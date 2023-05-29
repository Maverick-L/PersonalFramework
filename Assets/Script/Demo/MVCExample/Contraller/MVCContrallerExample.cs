using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;
using Framework.MVC;

#if UNITY_EDITOR
[GeneratorAttrubity("测试MVC——Contraller")]
#endif
public class MVCContrallerExample : BaseContraller
{

    public override void OnInitlization()
    {
        L.Log($"Contraller Init");
    }
}

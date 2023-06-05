using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.MVC;
public class ViewExample : BaseViewMediator
{
    protected override void OnRealCreate()
    {
        MVCExample_View1 view = Window<MVCExample_View1>();
        view.SetTitleText("MVCExample_View1");
        view.AddOpenView2ButtonListence(OnOpenView2);
        view.AddOpenView3ButtonListence(OnOpenView3);
    }

    protected override void OnRealDestory()
    {
    }

    private void OnOpenView2()
    {

    }

    private void OnOpenView3()
    {

    }
}

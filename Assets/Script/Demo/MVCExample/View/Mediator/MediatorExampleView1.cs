using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.MVC;

public class ExampleView1Params : BaseWindowParams
{
    public EWindow click2GoTo;
    public EWindow click3GoTo;
    public EWindow clickCloseGoTo;
    public string title;
}

public class MediatorExampleView1 : BaseViewMediator
{
    protected override void OnRealCreate()
    {
        MVCExample_View1 view = Window<MVCExample_View1>();
        ExampleView1Params param;
        GetParams<ExampleView1Params>(out param);
        view.SetTitleText(param == null ? "MVCExample_View1" : param.title);
        view.AddOpenView2ButtonListence(OnOpenView2);
        view.AddOpenView3ButtonListence(OnOpenView3);
        view.AddCloseButtonButtonListence(OnClosePanel);
    }

    protected override void OnRealDestory()
    {
    }

    private void OnOpenView2()
    {
        ExampleView1Params param;
        if(GetParams<ExampleView1Params>(out param))
        {
            ViewManager.instance.OnShow(param.click2GoTo);
        }
        else
        {
            ViewManager.instance.OnShow(EWindow.MVCExample_View2, new ExampleView1Params() {
                 click2GoTo = EWindow.MVCExample_View1,
                  click3GoTo = EWindow.MVCExample_View3,
                   clickCloseGoTo = EWindow.MVCExample_View1,
                    title = "MVCExample_View2",
            
            });
        }
    }

    private void OnOpenView3()
    {
        ExampleView1Params param;
        if (GetParams<ExampleView1Params>(out param))
        {
            ViewManager.instance.OnShow(param.click3GoTo);

        }
        else
        {
            ViewManager.instance.OnShow(EWindow.MVCExample_View2, new ExampleView1Params()
            {
                click2GoTo = EWindow.MVCExample_View1,
                click3GoTo = EWindow.MVCExample_View2,
                clickCloseGoTo = EWindow.MVCExample_View1,
                title = "MVCExample_View3",

            });
        }
    }

    private void OnClosePanel()
    {
        ExampleView1Params param;
        if (GetParams<ExampleView1Params>(out param))
        {
            ViewManager.instance.OnShow(param.clickCloseGoTo);
        }
    }
}

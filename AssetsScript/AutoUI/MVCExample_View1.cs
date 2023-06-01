using UnityEngine
using UnityEngine.UI
using System
using System.Collections
using System.Collections.Generic
public class MVCExample_View1 : Framework.MVC.BaseViewWindow
{
    public Text TitleText;
    public Button CloseButtonBtn;
    public Button OpenView2Btn;
    public Button OpenView3Btn;
    public override void Awake()
    {
        base.Awake();
        TitleText = transform.Find(/rawImageTitle).GetComponent<Text>();
        CloseButtonBtn = transform.Find(/rawImageCloseButton).GetComponent<Button>();
        OpenView2Btn = transform.Find(/rawImageOpenView2).GetComponent<Button>();
        OpenView3Btn = transform.Find(/rawImageOpenView3).GetComponent<Button>();
    }


    public override void OnEnable()
    {
        base.OnEnable();
    }


    public override void OnDisable()
    {
        base.OnDisable();
    }


    public override void Start()
    {
        base.Start();
    }


    public override void OnDestroy()
    {
        base.OnDestroy();
        CloseButtonBtn.onClick.RemoveAllListener()
        OpenView2Btn.onClick.RemoveAllListener()
        OpenView3Btn.onClick.RemoveAllListener()
    }


    public override void Update()
    {
        base.Update();
    }


    public void SetTitleTextText(string value)
    {
        TitleText.text = value;
    }
    public void AddCloseButtonBtnListence(UnityEngine.Events.UnityAction action)
    {
        CloseButtonBtn.onClick.AddListener(action)
    }


    public void RemoveCloseButtonBtnListence(UnityEngine.Events.UnityAction action)
    {
        CloseButtonBtn.onClick.RemoveListener(action)
    }
    public void AddOpenView2BtnListence(UnityEngine.Events.UnityAction action)
    {
        OpenView2Btn.onClick.AddListener(action)
    }


    public void RemoveOpenView2BtnListence(UnityEngine.Events.UnityAction action)
    {
        OpenView2Btn.onClick.RemoveListener(action)
    }
    public void AddOpenView3BtnListence(UnityEngine.Events.UnityAction action)
    {
        OpenView3Btn.onClick.AddListener(action)
    }


    public void RemoveOpenView3BtnListence(UnityEngine.Events.UnityAction action)
    {
        OpenView3Btn.onClick.RemoveListener(action)
    }
}

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
public class MVCExample_View1 : Framework.MVC.BaseViewWindow
{
    public Text titleText;
    public Button closeButtonBtn;
    public Button openView2Btn;
    public Button openView3Btn;
    public override void Awake()
    {
        base.Awake();
        titleText = transform.Find("rawImage/title").GetComponent<Text>();
        closeButtonBtn = transform.Find("rawImage/closeButton").GetComponent<Button>();
        openView2Btn = transform.Find("rawImage/openView2").GetComponent<Button>();
        openView3Btn = transform.Find("rawImage/openView3").GetComponent<Button>();
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
        closeButtonBtn.onClick.RemoveAllListeners();
        openView2Btn.onClick.RemoveAllListeners();
        openView3Btn.onClick.RemoveAllListeners();
    }


    public override void Update()
    {
        base.Update();
    }


    public void SetTitleTextText(string value)
    {
        titleText.text = value;
    }
    public void AddCloseButtonButtonListence(UnityEngine.Events.UnityAction action)
    {
        closeButtonBtn.onClick.AddListener(action);
    }


    public void RemoveCloseButtonButtonListence(UnityEngine.Events.UnityAction action)
    {
        closeButtonBtn.onClick.RemoveListener(action);
    }
    public void AddOpenView2ButtonListence(UnityEngine.Events.UnityAction action)
    {
        openView2Btn.onClick.AddListener(action);
    }


    public void RemoveOpenView2ButtonListence(UnityEngine.Events.UnityAction action)
    {
        openView2Btn.onClick.RemoveListener(action);
    }
    public void AddOpenView3ButtonListence(UnityEngine.Events.UnityAction action)
    {
        openView3Btn.onClick.AddListener(action);
    }


    public void RemoveOpenView3ButtonListence(UnityEngine.Events.UnityAction action)
    {
        openView3Btn.onClick.RemoveListener(action);
    }
}

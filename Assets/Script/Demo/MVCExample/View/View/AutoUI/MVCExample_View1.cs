using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public partial class MVCExample_View1 : Framework.MVC.BaseViewWindow
{
    public Text titleText;
    public Button closeButtonButton;
    public Button openView2Button;
    public Button openView3Button;
    public override void Awake()
    {
        base.Awake();
        titleText = transform.Find("rawImage/title").GetComponent<Text>();
        closeButtonButton = transform.Find("rawImage/closeButton").GetComponent<Button>();
        openView2Button = transform.Find("rawImage/openView2").GetComponent<Button>();
        openView3Button = transform.Find("rawImage/openView3").GetComponent<Button>();
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
        closeButtonButton.onClick.RemoveAllListeners();
        openView2Button.onClick.RemoveAllListeners();
        openView3Button.onClick.RemoveAllListeners();
    }


    public override void Update()
    {
        base.Update();
    }


    public void SetTitleText(string value)
    {
        titleText.text = value;
    }

    public void AddCloseButtonButtonListence(UnityEngine.Events.UnityAction action)
    {
        closeButtonButton.onClick.AddListener(action);
    }

    public void RemoveCloseButtonButtonListence(UnityEngine.Events.UnityAction action)
    {
        closeButtonButton.onClick.RemoveListener(action);
    }

    public void AddOpenView2ButtonListence(UnityEngine.Events.UnityAction action)
    {
        openView2Button.onClick.AddListener(action);
    }

    public void RemoveOpenView2ButtonListence(UnityEngine.Events.UnityAction action)
    {
        openView2Button.onClick.RemoveListener(action);
    }

    public void AddOpenView3ButtonListence(UnityEngine.Events.UnityAction action)
    {
        openView3Button.onClick.AddListener(action);
    }

    public void RemoveOpenView3ButtonListence(UnityEngine.Events.UnityAction action)
    {
        openView3Button.onClick.RemoveListener(action);
    }

}

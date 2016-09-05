using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UIFramework;

public class TopWindow : Window
{
    public GameObject backBtn;
    public GameObject raiselBtn;
    public GameObject gotoLoginBtn;

    public TopWindow() : base(WindowType.Fixed, HideMode.HideNothing, "UIPrefabs/UITopWindow", true) { }

    public override void Init()
    {
        base.Init();

        backBtn = uiTransform.Find("BtnBack").gameObject;
        raiselBtn = uiTransform.Find("BtnRaise").gameObject;
        gotoLoginBtn = uiTransform.Find("BtnGotoLogin").gameObject;


        UIUtility.RegisterClickedEvent(backBtn, () => { WindowManager.Instance.GoBack(); });
        UIUtility.RegisterClickedEvent(raiselBtn, () => { WindowManager.Instance.LoadWindow<RaiseWindow>(); });
        UIUtility.RegisterClickedEvent(gotoLoginBtn, () => { GameManager.Instance.GotoLogin(); });
    }

    public override void Show()
    {
        base.Show();
    }

    public override void Hide()
    {
        base.Hide();
    }

    public override void Destroy()
    {
        base.Destroy();
    }
}

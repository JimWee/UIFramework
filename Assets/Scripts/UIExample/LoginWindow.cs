using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UIFramework;

public class LoginWindow : Window
{
    public GameObject loginBtn;

    public LoginWindow() : base(WindowType.Normal, HideMode.HideNothing, "UIPrefabs/UILoginWindow") { }

    public override void Init()
    {
        base.Init();

        loginBtn = uiTransform.Find("ButtonLogin").gameObject;

        UIUtility.RegisterClickedEvent(loginBtn, () => { GameManager.Instance.GotoHome(); });
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

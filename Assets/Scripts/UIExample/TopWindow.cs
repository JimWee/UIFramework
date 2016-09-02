using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UIFramework;

public class TopWindow : Window
{
    public Button backBtn;
    public Button raiselBtn;

    public TopWindow() : base(WindowType.Fixed, HideMode.HideNothing, "UIPrefabs/UITopWindow", true) { }

    public override void Init()
    {
        base.Init();

        backBtn = uiTransform.Find("BtnBack").GetComponent<Button>();
        raiselBtn = uiTransform.Find("BtnRaise").GetComponent<Button>();

        backBtn.onClick.AddListener(() => { WindowManager.Instance.GoBack(); });
        raiselBtn.onClick.AddListener(() => { WindowManager.Instance.LoadWindow<RaiseWindow>(); });
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

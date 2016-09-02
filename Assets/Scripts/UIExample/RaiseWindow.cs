using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UIFramework;

public class RaiseWindow : Window
{
    public Button skillBtn;

    public RaiseWindow() : base(WindowType.Normal, HideMode.HideNothing, "UIPrefabs/UIRaiseWindow")
    {
        SetCacheUIs(new string[] { "UIPrefabs/UISkillWindow" });
        SetZSpace(1000);       
    }

    public override void Init()
    {
        base.Init();

        skillBtn = uiTransform.Find("BtnSkill").GetComponent<Button>();
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

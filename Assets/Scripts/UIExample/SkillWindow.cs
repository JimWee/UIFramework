using UnityEngine;
using System.Collections;
using UIFramework;

public class SkillWindow : Window
{
    public SkillWindow() : base(WindowType.Normal, HideMode.HidePrevious, "UIPrefab/UIMainWindow")
    {
        SetZSpace(500);
    }

    public override void Init()
    {
        base.Init();
        SetCloseBtns(new GameObject[] { uiTransform.Find("CloseBtn").gameObject });
    }

    public override void Show()
    {
        base.Show();
        LoadFX(uiTransform.Find("fxNode").gameObject, "UIFx");
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

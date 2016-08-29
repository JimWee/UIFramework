using UnityEngine;
using System.Collections;
using UIFramework;

public class MainWindow : Window
{
    public MainWindow() : base(WindowType.Normal, HideMode.HideNothing, "UIPrefab/UIMainWindow", false)
    {
        SetCacheUIs(new string[] { "UIPrefab/UISkillWindow" });
        SetZSpace(500);
    }

    public override void Init()
    {
        base.Init();
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

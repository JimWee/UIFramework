using UnityEngine;
using System.Collections;
using UIFramework;

public class TopWindow : Window
{
    public TopWindow() : base(WindowType.Fixed, HideMode.HideNothing, "UIPrefab/UITopWindow", false) { }

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

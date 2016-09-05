using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UIFramework;

public class MapWindow : Window
{
    public GameObject missionBtn;
    public GameObject argsText;

    public MapWindow() : base(WindowType.Normal, HideMode.HideNothing, "UIPrefabs/UIMapWindow") { }

    public override void Init()
    {
        base.Init();

        missionBtn = uiTransform.Find("ButtonMission").gameObject;
        argsText = uiTransform.Find("TextArgs").gameObject;

        UIUtility.RegisterClickedEvent(missionBtn, () => { WindowManager.Instance.LoadWindow<MissionWindow>(); });

        if (args != null)
        {
            UIUtility.SetText(argsText, args as string);
        }
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

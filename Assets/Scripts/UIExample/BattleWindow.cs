using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UIFramework;

public class BattleWindow : Window
{
    public GameObject gobackBtn;

    public BattleWindow() : base(WindowType.Normal, HideMode.HideNothing, "UIPrefabs/UIBattleWindow") { }

    public override void Init()
    {
        base.Init();

        gobackBtn = uiTransform.Find("ButtonGoback").gameObject;

        UIUtility.RegisterClickedEvent(gobackBtn, () => { GameManager.Instance.GotoHome(); });
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

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UIFramework;

public class MissionWindow : Window
{
    public GameObject gotoBattleBtn;
    public GameObject closeBtn;

    public MissionWindow() : base(WindowType.Normal, HideMode.HideNothing, "UIPrefabs/UIMissionWindow") { }

    public override void Init()
    {
        base.Init();

        SetIsSaved(false);

        gotoBattleBtn = uiTransform.Find("ButtonGotoBattle").gameObject;
        closeBtn = uiTransform.Find("ButtonClose").gameObject;

        UIUtility.RegisterClickedEvent(gotoBattleBtn, () => { GameManager.Instance.GotoBattle(); });

        SetCloseBtns(new GameObject[]{ closeBtn });
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

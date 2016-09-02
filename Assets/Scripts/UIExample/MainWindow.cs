using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UIFramework;

public class MainWindow : Window
{
    public GameObject mapBtn;
    public GameObject raiseBtn;
    public GameObject friendBtn;
    public GameObject modelNode;

    public MainWindow() : base(WindowType.Normal, HideMode.HideNothing, "UIPrefabs/UIMainWindow", false)
    {
        SetCacheUIs(new string[] { "UIPrefabs/UIRaiseWindow" });
        SetZSpace(1000);
    }

    public override void Init()
    {
        base.Init();

        mapBtn = uiTransform.Find("BtnMap").gameObject;
        raiseBtn = uiTransform.Find("BtnRaise").gameObject;
        friendBtn = uiTransform.Find("BtnFriend").gameObject;
        modelNode = uiTransform.Find("ModelNode").gameObject;

        UIUtility.RegisterClickedEvent(mapBtn, () => { });
        UIUtility.RegisterClickedEvent(raiseBtn, () => { WindowManager.Instance.LoadWindow<RaiseWindow>(); });
        UIUtility.RegisterClickedEvent(friendBtn, () => { });

        ResourceManager.Instance.LoadAssetAsync(
            "Models/HomeModel", 
            (Object obj) => 
            {
                GameObject model = GameObject.Instantiate(obj) as GameObject;
                if (model != null)
                {
                    model.transform.SetParent(modelNode.transform, false);
                }
            });
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

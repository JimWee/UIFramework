using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UIFramework;

public class RaiseWindow : Window
{
    public GameObject skillBtn;
    public Transform modelNode;
    public Transform fxNode;

    public RaiseWindow() : base(WindowType.Normal, HideMode.HideNothing, "UIPrefabs/UIRaiseWindow"){}

    public override void Init()
    {
        base.Init();
        
        SetCacheUIs(new string[] { "UIPrefabs/UISkillWindow" });
        SetZSpace(1000);

        skillBtn = uiTransform.Find("BtnSkill").gameObject;
        modelNode = uiTransform.Find("ModelNode");
        fxNode = uiTransform.Find("FXNode");

        UIUtility.RegisterClickedEvent(skillBtn, () => { WindowManager.Instance.LoadWindow<SkillWindow>(); });

        ResourceManager.Instance.LoadAssetAsync(
            "Models/RaiseModel",
            (Object obj) =>
            {
                GameObject model = GameObject.Instantiate(obj) as GameObject;
                if (model != null && modelNode != null)
                {
                    model.transform.SetParent(modelNode, false);
                }
            });

        ResourceManager.Instance.LoadFXAsync(
            this,
            "FX/RaiseFX",
            (GameObject fx) =>
            {
                if (fx != null && fxNode != null)
                {
                    fx.transform.SetParent(fxNode, false);
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

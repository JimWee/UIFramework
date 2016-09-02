using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UIFramework;

public class MessageWindowArgs
{
    public int btnNumber = 1;
    public string messageText = string.Empty;
    public UnityAction onOkClicked;
    public UnityAction onCancelClicked;
}

public class MessageWindow : Window
{
    public MessageWindowArgs msgWindowArgs;
    public GameObject okBtn;
    public GameObject cancelBtn;
    public GameObject ok2Btn;
    public GameObject msgText;

    public MessageWindow() : base(WindowType.Popup, HideMode.HideNothing, "UIPrefabs/UIMessageWindow") { }

    public override void Init()
    {
        base.Init();

        msgWindowArgs = args as MessageWindowArgs;
        if (msgWindowArgs == null)
        {
            msgWindowArgs = new MessageWindowArgs();
        }

        okBtn = uiTransform.Find("ButtonOK").gameObject;
        cancelBtn = uiTransform.Find("ButtonCaccel").gameObject;
        ok2Btn = uiTransform.Find("ButtonOK2").gameObject;
        msgText = uiTransform.Find("TextMessage").gameObject;

        UIUtility.SetText(msgText, msgWindowArgs.messageText);

        if (msgWindowArgs.btnNumber == 2)
        {
            ok2Btn.SetActive(false);
            okBtn.SetActive(true);
            cancelBtn.SetActive(true);

            UIUtility.RegisterClickedEvent(
                okBtn,
                () =>
                {
                    WindowManager.Instance.GoBack();
                    if (msgWindowArgs.onOkClicked != null)
                    {
                        msgWindowArgs.onOkClicked();
                    }                    
                });

            UIUtility.RegisterClickedEvent(
                cancelBtn,
                () =>
                {
                    WindowManager.Instance.GoBack();
                    if (msgWindowArgs.onCancelClicked != null)
                    {
                        msgWindowArgs.onCancelClicked();
                    }                    
                });
        }
        else
        {
            ok2Btn.SetActive(true);
            okBtn.SetActive(false);
            cancelBtn.SetActive(false);

            UIUtility.RegisterClickedEvent(
                ok2Btn,
                () =>
                {
                    if (msgWindowArgs.onOkClicked != null)
                    {
                        msgWindowArgs.onOkClicked();
                    }
                    WindowManager.Instance.GoBack();
                });
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

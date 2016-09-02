using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

namespace UIFramework
{
    public static class UIUtility
    {
        public static UnityAction goBackAction = () => { WindowManager.Instance.GoBack(); };

        public static void RegisterClickedEvent(GameObject uiBtn, UnityAction clickedAction)
        {
            if (uiBtn == null)
            {
                Debug.LogErrorFormat("RegisterClickedEvent uiBtn is null");
                return;
            }

            Button btnCpt = uiBtn.GetComponent<Button>();
            if (btnCpt == null)
            {
                Debug.LogErrorFormat("GameObject doesn't have a Button Component : {0}", uiBtn.name);
                return;
            }

            if (clickedAction == null)
            {
                Debug.LogErrorFormat("ClickedAction is null : {0}", uiBtn.name);
                return;
            }

            btnCpt.onClick.AddListener(clickedAction);
        }


        public static void SetText(GameObject uiText, string text)
        {
            if (uiText == null)
            {
                Debug.LogErrorFormat("SetText uiText is null");
                return;
            }

            Text textCpt = uiText.GetComponent<Text>();
            if (textCpt == null)
            {
                Debug.LogErrorFormat("GameObject doesn't have a Text Component : {0}", uiText.name);
                return;
            }

            if (text == null)
            {
                Debug.LogErrorFormat("text is null : {0}", uiText.name);
                return;
            }

            textCpt.text = text;
        }
    }
}

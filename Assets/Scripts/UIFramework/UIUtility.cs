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
            Button btnCpt = uiBtn.GetComponent<Button>();
            if (btnCpt == null)
            {
                btnCpt = uiBtn.AddComponent<Button>();
            }
            btnCpt.onClick.AddListener(clickedAction);
        }

    }
}

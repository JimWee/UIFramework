using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Object = UnityEngine.Object;

namespace UIFramework
{
    public class CacheUI
    {
        public string path;
        public GameObject gameObject;
        public bool isLoaded;

        public CacheUI(string path, GameObject gameObject, bool isLoaded)
        {
            this.path = path;
            this.gameObject = gameObject;
            this.isLoaded = isLoaded;
        }
    }

    public class WindowManager : MonoBehaviour
    {
        public Transform normalRoot;
        public Transform fixedRoot;
        public Transform popupRoot;
        public Camera uiCamera;
        public GameObject loadingTipUI;

        public static WindowManager Instance
        {
            get { return instance; }
        }
        private static WindowManager instance;

        /// <summary>
        /// 等待UI加载完毕的超时时间，单位秒
        /// </summary>
        public const float loadUIExpireTime = 1f;
        /// <summary>
        /// 两个UI间的canvas order 间隙
        /// </summary>
        public const int canvasOrderGap = 10;

        /// <summary>
        /// 缓存的UI
        /// </summary>
        public Dictionary<string, CacheUI> cacheUIs;
        /// <summary>
        /// 所有类型的窗口
        /// </summary>
        public Dictionary<WindowType, List<Window>> windows;
        /// <summary>
        /// 记录的窗口参数，用于恢复窗口
        /// </summary>  
        public Dictionary<string, WindowArgs> windowArgsDic;

        void Awake()
        {
            instance = this;
            cacheUIs = new Dictionary<string, CacheUI>();
            windows = new Dictionary<WindowType, List<Window>>();
            foreach (WindowType type in Enum.GetValues(typeof(WindowType)))
            {
                windows.Add(type, new List<Window>());
            }
            windowArgsDic = new Dictionary<string, WindowArgs>();
        }

        public void CacheUI(string uiPath)
        {
            if (cacheUIs.ContainsKey(uiPath))
            {
                return;
            }

            cacheUIs.Add(uiPath, new CacheUI(uiPath, null, false));
            ResourceManager.LoadAssetAsync(
                uiPath,
                (Object obj) =>
                {
                    if (obj == null)
                    {
                        Debug.LogError(string.Format("cache UI failed: {0}", uiPath));
                    }
                    CacheUI cacheUI = cacheUIs[uiPath];
                    if (cacheUI == null)
                    {
                        cacheUIs[uiPath] = new CacheUI(uiPath, obj as GameObject, true);
                    }
                    else
                    {
                        cacheUI.gameObject = obj as GameObject;
                        cacheUI.isLoaded = true;
                    }
                });
        }

        public void LoadWindow<T>(WindowArgs windowArgs = null, bool isAsync = false, Action callback = null) where T : Window, new()
        {
            StartCoroutine(_LoadWindow<T>(windowArgs, isAsync, callback));
        }

        private IEnumerator _LoadWindow<T>(WindowArgs windowArgs = null, bool isAsync = false, Action callback = null) where T : Window, new()
        {
            //显示异步加载UI
            if (isAsync)
            {
                loadingTipUI.SetActive(true);
            }

            T window = new T();

            //显示normal窗口时，清空popup窗口
            if (window.type == WindowType.Normal)
            {
                ClearWindows(windows[WindowType.Popup]);
            }

            var curTypeWindows = windows[window.type];

            //除normal类型窗口都新建，normal类型在不存在时新建，存在则提到栈顶
            if (window.type != WindowType.Normal || (window.type == WindowType.Normal && !CheckNormalWindowExist(typeof(T).ToString())))
            {
                yield return StartCoroutine(LoadUI(window, isAsync));
                if (!string.IsNullOrEmpty(window.error))
                {
                    Debug.LogError(window.error);
                    if (isAsync)
                    {
                        loadingTipUI.SetActive(false);
                    }
                    yield break;
                }

                curTypeWindows.Add(window);
            }

            Window curWindow = curTypeWindows[curTypeWindows.Count - 1];

            //如果需要，隐藏前一个窗口
            Window preWindow = null;
            if (curWindow.hideMode == HideMode.HidePrevious && curTypeWindows.Count > 1)
            {
                preWindow = curTypeWindows[curTypeWindows.Count - 2];
                preWindow.Hide();
            }

            //设置canvas order
            int minCanvasOrder = preWindow == null ? 0 : preWindow.maxCanvasOrder + canvasOrderGap;
            curWindow.minCanvasOrder = minCanvasOrder;
            curWindow.uiGameObject.GetComponent<Canvas>().sortingOrder = minCanvasOrder;
            ParticleSystem[] particleSystems = curWindow.uiGameObject.GetComponentsInChildren<ParticleSystem>(true);
            int maxCanvasOrder = minCanvasOrder;
            for (int i = 0; i < particleSystems.Length; i++)
            {
                Renderer renderer = particleSystems[i].GetComponent<Renderer>();
                int canvasOrder = renderer.sortingOrder + minCanvasOrder;
                maxCanvasOrder = Mathf.Max(canvasOrder, maxCanvasOrder);
                renderer.sortingOrder = canvasOrder;
            }
            curWindow.maxCanvasOrder = maxCanvasOrder;

            //设置当前窗口Z值
            float deltaZ = preWindow == null ? 0 : preWindow.zSpace;
            var pos = curWindow.uiTransform.localPosition;
            curWindow.uiTransform.localPosition = new Vector3(pos.x, pos.y, pos.z);

            //更新UIRoot和UICamera的Z值
            SetNodeZAboveWindows(fixedRoot, windows[WindowType.Normal], normalRoot);
            SetNodeZAboveWindows(popupRoot, windows[WindowType.Fixed], fixedRoot);
            SetNodeZAboveWindows(uiCamera.transform, windows[WindowType.Popup], popupRoot);

            //窗口参数
            if (windowArgs != null)
            {
                curWindow.args = windowArgs;
                if (curWindow.type == WindowType.Normal)
                {
                    windowArgsDic[curWindow.uiName] = windowArgs;
                }
            }

            //窗口初始化
            curWindow.Init();
            curWindow.Show();

            //隐藏异步加载UI，触发回调
            if (isAsync)
            {
                loadingTipUI.SetActive(false);
                if (callback != null)
                {
                    callback();
                }
            }

        }

        private IEnumerator LoadUI(Window window, bool isAsync = false)
        {
            GameObject uiGameObject = null;
            if (cacheUIs.ContainsKey(window.uiPath))
            {
                CacheUI cacheUI = cacheUIs[window.uiPath];
                float startTime = Time.realtimeSinceStartup;
                while (!cacheUI.isLoaded)
                {
                    yield return null;
                    if (Time.realtimeSinceStartup - startTime > loadUIExpireTime)
                    {
                        window.error = string.Format("wait for cache ui load time expire : {0}", cacheUI.path);
                        yield break;
                    }
                }
                uiGameObject = cacheUI.gameObject;
            }
            else if (isAsync)
            {
                yield return StartCoroutine(ResourceManager.LoadAssetAsync(window.uiPath, (Object obj) => { uiGameObject = obj as GameObject; }));
            }
            else
            {
                uiGameObject = ResourceManager.LoadAsset(window.uiPath) as GameObject;
            }

            if (uiGameObject == null)
            {
                window.error = string.Format("load ui failed : {0}", window.uiPath);
                yield break;
            }
            cacheUIs[window.uiPath] = new CacheUI(window.uiPath, uiGameObject, true);

            AnchorUIGameObject(uiGameObject, window.type);
            window.uiGameObject = uiGameObject;
            window.uiTransform = uiGameObject.transform;
        }

        /// <summary>
        /// 返回操作
        /// </summary>
        public void GoBack()
        {
            ///先处理popup窗口栈
            var popupwindows = windows[WindowType.Popup];            
            if (popupwindows.Count > 0)
            {
                PopWindow(popupwindows);
                return;
            }

            ///popup窗口栈为空时，再处理normal窗口栈
            var normalwindows = windows[WindowType.Normal];
            PopWindow(normalwindows);

        }

        /// <summary>
        /// 关闭窗口栈栈顶的窗口
        /// </summary>
        /// <param name="windows"></param>
        private void PopWindow(List<Window> windows)
        {
            if (windows.Count == 0)
            {
                return;
            }

            Window curWindow = windows[windows.Count - 1]; 
                       
            if (curWindow.isResponseBackEvent)
            {
                //弹出销毁栈顶元素
                windows.RemoveAt(windows.Count - 1);
                curWindow.Hide();
                curWindow.Destroy();

                //如果需要，清除记录的窗口参数
                if (curWindow.args != null && curWindow.type == WindowType.Normal)
                {
                    windowArgsDic.Remove(curWindow.uiName);
                }

                //显示当前栈顶窗口
                if (windows.Count > 0)
                {
                    windows[windows.Count - 1].Show();
                }
            }
            else
            {
                //弹出提示退出游戏的界面
            }
        }

        /// <summary>
        /// 清空窗口栈
        /// </summary>
        /// <param name="windows"></param>
        private void ClearWindows(List<Window> windows)
        {
            for (int i = windows.Count - 1; i >= 0; i--)
            {
                var popupWindow = windows[i];
                popupWindow.Hide();
                popupWindow.Destroy();
            }
            windows.Clear();
        }

        /// <summary>
        /// 根据windowType将UI挂到正确的root节点
        /// </summary>
        /// <param name="ui"></param>
        /// <param name="windowType"></param>
        private void AnchorUIGameObject(GameObject ui, WindowType windowType)
        {
            switch (windowType)
            {
                case WindowType.Fixed:
                    ui.transform.SetParent(fixedRoot, false);
                    break;
                case WindowType.Normal:
                    ui.transform.SetParent(normalRoot, false);
                    break;
                case WindowType.Popup:
                    ui.transform.SetParent(popupRoot, false);
                    break;
            }
        }

        /// <summary>
        /// 设置node的Z值在比所有windows的Z值高
        /// </summary>
        /// <param name="node"></param>
        /// <param name="windows"></param>
        /// <param name="windowsNode"></param>
        private void SetNodeZAboveWindows(Transform node, List<Window> windows, Transform windowsNode)
        {
            float zValue = windowsNode.localPosition.z;
            if (windows.Count > 0)
            {
                var topWindow = windows[windows.Count - 1];
                zValue += topWindow.uiTransform.localPosition.z + topWindow.zSpace;
            }
            var pos = node.localPosition;
            node.localPosition = new Vector3(pos.x, pos.y, zValue);
        }

        /// <summary>
        /// 检查窗口是否存在，窗口如果已经存在于栈中，提到栈顶
        /// </summary>
        /// <param name="uiName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private bool CheckNormalWindowExist(string uiName)
        {
            var normalWindows = windows[WindowType.Normal];
            for (int i = 0; i < normalWindows.Count; i++)
            {
                Window window = normalWindows[i];
                if (window.uiName == uiName)
                {
                    normalWindows.RemoveAt(i);
                    normalWindows.Add(window);
                    return true;
                }
            }
            return false;
        }
    }
}

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
        public Transform cacheRoot;
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
        public float loadUIExpireTime = 1f;
        /// <summary>
        /// 两个UI间的canvas order 间隙
        /// </summary>
        public int canvasOrderGap = 1;
        /// <summary>
        /// 缓存的UI
        /// </summary>
        public Dictionary<string, CacheUI> cacheUIs;
        /// <summary>
        /// 缓存的窗口
        /// </summary>
        public Dictionary<string, Window> cacheWindows;
        /// <summary>
        /// 所有类型的窗口
        /// </summary>
        public List<Window> windows;
        /// <summary>
        /// 记录的窗口参数，用于恢复窗口
        /// </summary>  
        public Dictionary<string, object> windowArgsDic;

        void Awake()
        {
            instance = this;
            cacheUIs = new Dictionary<string, CacheUI>();
            cacheWindows = new Dictionary<string, Window>();
            windows = new List<Window>();
            windowArgsDic = new Dictionary<string, object>();
            DontDestroyOnLoad(gameObject);
        }

        public void CacheUI(string uiPath)
        {
            if (cacheUIs.ContainsKey(uiPath))
            {
                return;
            }

            cacheUIs.Add(uiPath, new CacheUI(uiPath, null, false));
            ResourceManager.Instance.LoadAssetAsync(
                uiPath,
                (Object obj) =>
                {
                    if (obj == null)
                    {
                        Debug.LogError(string.Format("cache UI failed: {0}", uiPath));
                    }

                    GameObject instance = GameObject.Instantiate(obj) as GameObject;
                    instance.transform.SetParent(cacheRoot, false);

                    CacheUI cacheUI = cacheUIs[uiPath];
                    if (cacheUI == null)
                    {
                        cacheUIs[uiPath] = new CacheUI(uiPath, instance, true);
                    }
                    else
                    {
                        cacheUI.gameObject = instance;
                        cacheUI.isLoaded = true;
                    }
                });
        }

        public void LoadWindow<T>(object windowArgs = null, bool isAsync = false, Action callback = null) where T : Window, new()
        {
            StartCoroutine(_LoadWindow<T>(windowArgs, isAsync, callback));
        }

        private IEnumerator _LoadWindow<T>(object windowArgs = null, bool isAsync = false, Action callback = null) where T : Window, new()
        {
            //显示异步加载UI
            if (isAsync)
            {
                loadingTipUI.SetActive(true);
            }

            Window window;

            int index = FindWindow(typeof(T).ToString());
            if (index >= 0)//如果窗口已存在，则移到顶端,根窗口除外
            {
                window = windows[index] as T;
                if (window.isRoot)
                {
                    Debug.LogErrorFormat("can not load existing root window : {0}", window.windowName);
                    yield break;
                }
                windows.RemoveAt(index);
                windows.Add(window);
            }
            else//否则新建窗口，加入顶端
            {
                window = NewWindow<T>();
                windows.Add(window);
                yield return StartCoroutine(LoadUI(window, isAsync));
                if (!string.IsNullOrEmpty(window.error))
                {
                    Debug.LogErrorFormat("Load Window Fail - {0}", window.error);
                    int errorIndex = FindWindow(window.windowName);
                    if (errorIndex >= 0)
                    {
                        Window errorWindow = windows[errorIndex];
                        windows.RemoveAt(errorIndex);
                        DeleteWindow(errorWindow);
                    }
                    yield break;
                }
                
            }
            //窗口参数
            if (windowArgs != null)
            {
                window.args = windowArgs;
                windowArgsDic[window.windowName] = windowArgs;
            }

            window.Init();

            //如果需要，隐藏前一个窗口
            Window preWindow = windows.Count > 1 ? windows[windows.Count - 2] : null;

            if (window.hideMode == HideMode.HidePrevious && preWindow != null)
            {
                preWindow.Hide();
            }

            //设置canvas order
            int minCanvasOrder = preWindow == null ? 0 : preWindow.maxCanvasOrder + canvasOrderGap;
            window.minCanvasOrder = minCanvasOrder;
            window.uiGameObject.GetComponent<Canvas>().sortingOrder = minCanvasOrder;
            ParticleSystem[] particleSystems = window.uiGameObject.GetComponentsInChildren<ParticleSystem>(true);
            int maxCanvasOrder = minCanvasOrder;
            for (int i = 0; i < particleSystems.Length; i++)
            {
                Renderer renderer = particleSystems[i].GetComponent<Renderer>();
                int canvasOrder = renderer.sortingOrder + minCanvasOrder;
                maxCanvasOrder = Mathf.Max(canvasOrder, maxCanvasOrder);
                renderer.sortingOrder = canvasOrder;
            }
            window.maxCanvasOrder = maxCanvasOrder;

            //设置当前窗口Z值
            if (preWindow != null)
            {
                float deltaZ = preWindow.zSpace;
                var windowPos = window.uiTransform.localPosition;
                window.uiTransform.localPosition = new Vector3(windowPos.x, windowPos.y, preWindow.uiTransform.localPosition.z - deltaZ);
            }

            //更新相机和加载提示UI的Z值
            float z = window.uiTransform.localPosition.z - window.zSpace - 1000;
            var pos = loadingTipUI.transform.localPosition;
            loadingTipUI.transform.localPosition = new Vector3(pos.x, pos.y, z);
            pos = uiCamera.transform.localPosition;
            uiCamera.transform.localPosition = new Vector3(pos.x, pos.y, z - 1000);

            //显示窗口
            window.Show();

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
            if (cacheUIs.ContainsKey(window.uiPath))//如果UI已缓存，等待UI加载完毕
            {
                CacheUI cacheUI = cacheUIs[window.uiPath];
                float startTime = Time.realtimeSinceStartup;
                while (!cacheUI.isLoaded)
                {
                    loadingTipUI.SetActive(true);
                    yield return null;
                    if (Time.realtimeSinceStartup - startTime > loadUIExpireTime)
                    {
                        window.error = string.Format("wait for cache ui load time expire : {0}", cacheUI.path);
                        loadingTipUI.SetActive(false);
                        yield break;
                    }
                }
                loadingTipUI.SetActive(false);
                uiGameObject = cacheUI.gameObject;
                cacheUIs.Remove(window.uiPath);
            }
            else if (isAsync)
            {
                yield return StartCoroutine(ResourceManager.Instance._LoadAssetAsync(window.uiPath, (Object obj) => { uiGameObject = GameObject.Instantiate(obj) as GameObject; }));
            }
            else
            {
                uiGameObject = GameObject.Instantiate(ResourceManager.Instance.LoadAsset(window.uiPath)) as GameObject;
            }

            if (uiGameObject == null)
            {
                window.error = string.Format("load ui failed : {0}", window.uiPath);
                yield break;
            }

            AnchorUIGameObject(uiGameObject, window.type);
            window.uiGameObject = uiGameObject;
            window.uiTransform = uiGameObject.transform;
        }

        /// <summary>
        /// 返回操作
        /// </summary>
        public void GoBack()
        {
            PopWindow();
        }

        /// <summary>
        /// 关闭窗口栈栈顶的窗口
        /// </summary>
        /// <param name="windows"></param>
        private void PopWindow()
        {
            if (windows.Count == 0)
            {
                return;
            }

            Window window = windows[windows.Count - 1]; 
                       
            if (!window.isRoot)
            {
                //弹出栈顶窗口
                windows.RemoveAt(windows.Count - 1);
                window.Hide();                

                //如果需要，清除记录的窗口参数
                if (window.args != null)
                {
                    windowArgsDic.Remove(window.windowName);
                }

                //加入UI缓存
                if (!cacheUIs.ContainsKey(window.uiPath))
                {
                    cacheUIs.Add(window.uiPath, new CacheUI(window.uiPath, window.uiGameObject, true));
                    window.uiGameObject.transform.SetParent(cacheRoot, false);                 
                }
                else
                {
                    GameObject.Destroy(window.uiGameObject);
                }

                window.Destroy();

                DeleteWindow(window);

                //显示当前栈顶窗口
                if (windows.Count > 0)
                {
                    windows[windows.Count - 1].Show();
                }
            }
            else
            {
                //弹出提示退出游戏的界面
                MessageWindowArgs args = new MessageWindowArgs();
                args.btnNumber = 2;
                args.messageText = "确认退出游戏";
                args.onOkClicked = () => { Application.Quit(); };
                LoadWindow<MessageWindow>(args);
            }
        }

        /// <summary>
        /// 清空窗口栈
        /// </summary>
        /// <param name="windows"></param>
        private void ClearWindows()
        {
            for (int i = windows.Count - 1; i >= 0; i--)
            {
                Window window = windows[i];
                window.Hide();
                window.Destroy();
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
        /// 返回指定window在windows中的位置，-1表示不存在
        /// </summary>
        /// <param name="windowName"></param>
        /// <returns></returns>
        private int FindWindow(string windowName)
        {
            for (int i = 0; i < windows.Count; i++)
            {
                Window window = windows[i];
                if (window.windowName == windowName)
                {
                    return i;
                }
            }
            return -1;
        }

        private T NewWindow<T>() where T : Window, new()
        {
            T window;
            string windowName = typeof(T).ToString();
            if (cacheWindows.ContainsKey(windowName))
            {
                window = cacheWindows[windowName] as T;
                cacheWindows.Remove(windowName);
            }
            else
            {
                window = new T();
            }
            return window;
        }

        private void DeleteWindow(Window window)
        {
            window.Clear();
            if (!cacheWindows.ContainsKey(window.windowName))
            {
                cacheWindows.Add(window.windowName, window);
            }
        }
    }
}

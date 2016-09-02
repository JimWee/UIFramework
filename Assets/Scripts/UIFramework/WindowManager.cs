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
            if (index >= 0)//如果窗口已存在，则移到顶端
            {
                window = windows[index] as T;
                windows.RemoveAt(index);
                windows.Add(window);
            }
            else//否则新建窗口，加入顶端
            {
                window = NewWindow<T>();
                yield return LoadUI(window, isAsync);
                if (!string.IsNullOrEmpty(window.error))
                {
                    Debug.LogErrorFormat("Load Window Fail - {0}", window.error);
                }
                windows.Add(window);
            }

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
                var pos = window.uiTransform.localPosition;
                window.uiTransform.localPosition = new Vector3(pos.x, pos.y, preWindow.uiTransform.localPosition.z - deltaZ);
            }

            //窗口参数
            if (windowArgs != null)
            {
                window.args = windowArgs;
                windowArgsDic[window.windowName] = windowArgs;
            }

            //窗口初始化
            window.Init();
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
                    yield return null;
                    if (Time.realtimeSinceStartup - startTime > loadUIExpireTime)
                    {
                        window.error = string.Format("wait for cache ui load time expire : {0}", cacheUI.path);
                        yield break;
                    }
                }
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
                       
            if (window.isResponseBackEvent)
            {
                //弹出销毁栈顶窗口
                windows.RemoveAt(windows.Count - 1);
                window.Hide();
                window.Destroy();

                //如果需要，清除记录的窗口参数
                if (window.args != null)
                {
                    windowArgsDic.Remove(window.windowName);
                    window.args = null;
                }

                //显示当前栈顶窗口
                if (windows.Count > 0)
                {
                    windows[windows.Count - 1].Show();
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
                window.uiGameObject = null;
                window.uiTransform = null;

                //加入窗口缓存
                if (!cacheWindows.ContainsKey(window.windowName))
                {
                    cacheWindows.Add(window.windowName, window);
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
    }
}

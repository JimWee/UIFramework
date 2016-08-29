using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System;

namespace UIFramework
{
    /// <summary>
    /// 窗口类型
    /// </summary>
    public enum WindowType
    {
        Fixed,  //层级居中
        Normal, //层级最下
        Popup,  //层级最上
    }

    /// <summary>
    /// 窗口隐藏模式，显示当前窗口时对其他窗口的操作
    /// </summary>
    public enum HideMode
    {
        HideNothing,    //不隐藏其他窗口
        HidePrevious,   //隐藏前一个窗口
    }

    public class WindowArgs { }

    /// <summary>
    /// 窗口基类
    /// </summary>
    public class Window : MonoBehaviour
    {
        /// <summary>
        /// 窗口类型
        /// </summary>
        public WindowType type = WindowType.Normal;
        /// <summary>
        /// 窗口隐藏模式
        /// </summary>
        public HideMode hideMode = HideMode.HideNothing;
        /// <summary>
        /// prefab路径
        /// </summary>
        public string uiPath;
        /// <summary>
        /// 窗口名字
        /// </summary>
        public string uiName;
        /// <summary>
        /// 加载当前窗口需要预缓存的UI
        /// </summary>
        public string[] cacheUIs;
        /// <summary>
        /// 是否相应返回键事件，不响应则会提示是否退出程序(谷歌要求所有界面能对返回键做出正确响应)
        /// </summary>
        public bool isResponseBackEvent;
        /// <summary>
        /// 窗口中所有触发关闭窗口的按钮，用于同一处理关闭事件
        /// </summary>
        public GameObject[] closeBtns;
        /// <summary>
        /// 传递给当前窗口的参数
        /// </summary>
        public WindowArgs args;
        /// <summary>
        /// UI的gameObject
        /// </summary>
        public GameObject uiGameObject;
        /// <summary>
        /// UI的transform
        /// </summary>
        public Transform uiTransform;
        /// <summary>
        /// 当前窗口是否显示
        /// </summary>
        public bool isActive;
        /// <summary>
        /// 加载窗口时的错误信息
        /// </summary>
        public string error;
        /// <summary>
        /// 窗口UI的Canvas中最大和最小Order in Layer
        /// </summary>
        public int minCanvasOrder;
        public int maxCanvasOrder;
        /// <summary>
        /// 窗口需要的Z值空间（用于防止3D模型穿插）
        /// </summary>
        public float zSpace;

        public Window(WindowType type, HideMode hideMode, string uiPath, bool isResponseBackEvent = true)
        {
            this.type = type;
            this.hideMode = hideMode;
            this.uiPath = uiPath;
            this.isResponseBackEvent = isResponseBackEvent;
            this.uiName = this.GetType().ToString();
            this.isActive = false;
        }

        /// <summary>
        /// 设置加载当前窗口时，需要异步缓存的UI
        /// </summary>
        /// <param name="cacheUIs"></param>
        public void SetCacheUIs(string[] cacheUIs)
        {
            this.cacheUIs = cacheUIs;
            for (int i = 0; i < cacheUIs.Length; i++)
            {
                WindowManager.Instance.CacheUI(cacheUIs[i]);
            }
        }
        /// <summary>
        /// 设置窗口关闭按钮事件
        /// </summary>
        /// <param name="closeBtns"></param>
        public void SetCloseBtns(GameObject[] closeBtns)
        {
            for (int i = 0; i < closeBtns.Length; i++)
            {
                UIUtility.RegisterClickedEvent(closeBtns[i], UIUtility.goBackAction);
            }
        }

        /// <summary>
        /// 设置窗口Z值空间
        /// </summary>
        /// <param name="zSpace"></param>
        public void SetZSpace(float zSpace)
        {
            this.zSpace = zSpace;
        }
        /// <summary>
        /// 动态加载粒子特效，自动设置正确的order in layer
        /// </summary>
        /// <param name="node"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public GameObject LoadFX(GameObject node, string path)
        {
            maxCanvasOrder += 1;
            GameObject fx = ResourceManager.LoadAsset(path) as GameObject;
            fx.GetComponent<Renderer>().sortingOrder = maxCanvasOrder;
            fx.transform.SetParent(node.transform, false);
            return fx;
        }
        /// <summary>
        /// 当前窗口是否显示
        /// </summary>
        /// <returns></returns>
        public bool IsActive()
        {
            return isActive;
        }
        /// <summary>
        /// 窗口初始化，第一次显示窗口时调用
        /// </summary>
        public virtual void Init() { }
        /// <summary>
        /// 窗口刷新，每次显示窗口时调用
        /// </summary>
        public virtual void Show()
        {
            uiGameObject.SetActive(true);
            isActive = true;
        }
        /// <summary>
        ///  窗口隐藏，当隐藏窗口时调用
        /// </summary>
        public virtual void Hide()
        {
            uiGameObject.SetActive(false);
            isActive = false;
        }
        /// <summary>
        /// 窗口销毁，关闭窗口时调用
        /// </summary>
        public virtual void Destroy() { }
    }
}

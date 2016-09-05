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

    /// <summary>
    /// 窗口基类
    /// </summary>
    public class Window
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
        public string windowName;
        /// <summary>
        /// 传递给当前窗口的参数
        /// </summary>
        public object args;
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
        /// <summary>
        /// 是否根窗口，根窗口会在开始是被创建，并且在当前场景不会被销毁
        /// </summary>
        public bool isRoot;
        /// <summary>
        /// 是否在切换场景时保存
        /// </summary>
        public bool isSaved;

        public Window(WindowType type, HideMode hideMode, string uiPath, bool isRoot = false)
        {
            this.type = type;
            this.hideMode = hideMode;
            this.uiPath = uiPath;
            this.windowName = this.GetType().ToString();
            this.isActive = false;
            this.isRoot = isRoot;
            this.isSaved = true;
        }

        /// <summary>
        /// 设置加载当前窗口时，需要异步缓存的UI
        /// </summary>
        /// <param name="cacheUIs"></param>
        public void SetCacheUIs(string[] cacheUIs)
        {
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
        /// 设置是否在场景切换时保存
        /// </summary>
        /// <param name="isSaved"></param>
        public void SetIsSaved(bool isSaved)
        {
            this.isSaved = isSaved;
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
        public virtual void Destroy()
        {
        }

        public void Clear()
        {
            args = null;
            zSpace = 0;
            error = null;
            uiGameObject = null;
            uiTransform = null;
            minCanvasOrder = 0;
            maxCanvasOrder = 0;
        }
    }
}

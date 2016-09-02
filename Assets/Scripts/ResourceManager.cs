using UnityEngine;
using System;
using System.Collections;
using Object = UnityEngine.Object;
using UIFramework;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance
    {
        get { return instance; }
    }
    private static ResourceManager instance;

    void Awake()
    {
        instance = this;
    }

    public Object LoadAsset(string path)
    {
        Object obj = Resources.Load(path);
        if (obj == null)
        {
            Debug.LogErrorFormat("Load asset failed : {0}", path);
        }
        return obj;
    }

    public void LoadAssetAsync(string path, Action<Object> callback)
    {
        StartCoroutine(_LoadAssetAsync(path, callback));
    }

    public IEnumerator _LoadAssetAsync(string path, Action<Object> callback)
    {
        ResourceRequest request = Resources.LoadAsync(path);
        yield return new WaitForSeconds(1);
        yield return request;
        if (request.asset == null)
        {
            Debug.LogErrorFormat("Load asset failed : {0}", path);
        }
        callback(request.asset);
    }

    /// <summary>
    /// 动态加载粒子特效，自动设置正确的order in layer
    /// </summary>
    /// <param name="node"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public GameObject LoadFX(Window window, string path)
    {
        return PrepareFx(ResourceManager.Instance.LoadAsset(path), window);
    }

    public void LoadFXAsync(Window window, string path, Action<GameObject> callback)
    {
        StartCoroutine(_LoadAssetAsync(path, (Object obj) => { callback(PrepareFx(obj, window)); }));
    }

    public GameObject PrepareFx(Object obj, Window window)
    {
        GameObject fx = GameObject.Instantiate(obj) as GameObject;
        if (fx != null)
        {
            Renderer renderer = fx.GetComponent<Renderer>();
            renderer.sortingLayerName = window.type.ToString();
            ++window.maxCanvasOrder;
            renderer.sortingOrder = window.maxCanvasOrder;
        }
        return fx;
    }
}

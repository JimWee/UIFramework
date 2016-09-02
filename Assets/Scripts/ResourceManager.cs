using UnityEngine;
using System;
using System.Collections;
using Object = UnityEngine.Object;

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
        yield return request;
        if (request.asset == null)
        {
            Debug.LogErrorFormat("Load asset failed : {0}", path);
        }
        callback(request.asset);
    }
}

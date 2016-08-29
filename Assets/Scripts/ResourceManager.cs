using UnityEngine;
using System;
using System.Collections;
using Object = UnityEngine.Object;

public class ResourceManager
{
    public static Object LoadAsset(string path)
    {
        return Resources.Load(path);
    }

    public static IEnumerator LoadAssetAsync(string path, Action<Object> callback)
    {
        ResourceRequest request = Resources.LoadAsync(path);
        yield return request;
        callback(request.asset);
    }
}

# Introduction

- [repo](https://github.com/netpyoung/nf.unitylibs.managers.assetbundlemanagement/)

## upm

- <https://docs.unity3d.com/Manual/upm-ui-giturl.html>

```
https://github.com/netpyoung/nf.unitylibs.managers.assetbundlemanagement.git?path=LocalPackage
```

## Example

``` cs
[SerializeField]
AssetBundleManager _bundle = new AssetBundleManager();

async void Start()
{
    string deviceBaseDirFpath = AssetBundleManager_ForEditorBuild0.GetDeviceBaseDirFpath_ForEditorBuild0();
    string manifestName = AssetBundleManager_ForEditorBuild0.MANIFEST_NAME;

    Exception? exOrNull = await _bundle.InitAsync(deviceBaseDirFpath, manifestName);
    if (exOrNull is Exception ex)
    {
        Debug.LogException(ex);
        return;
    }

    TaskBundle<GameObject> task = _bundle.RentBundleOrNull<GameObject>("cube")!;
    while (!task.IsCompleted)
    {
        await Task.Yield();
    }

    Bundle<GameObject> bundle = await task;
    
    Instantiate(bundle.AssetOrNull, Vector3.zero, Quaternion.identity);

    await Task.Delay(3000);

    await _bundle.ReturnBundleAsync(bundle);
}

private void Update()
{
    _bundle.Update();
}

private void OnDestroy()
{
    _bundle.Dispose();
}
```
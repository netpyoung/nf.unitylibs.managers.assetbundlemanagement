#if UNITY_EDITOR
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;

namespace NF.UnityLibs.Managers.AssetBundleManagement
{
    public sealed class AssetBundleManager_ForEditorAsset : AssetBundleManager
    {
        public override async Task<Exception?> InitAsync(string deviceBaseDirFpath, string manifestName)
        {
            await Task.Yield();
            return null;
        }

        public override TaskBundle<T>? RentBundleOrNull<T>(string assetBundleName)
        {
            AssetBundleRef? assetBundleRefOrNull = _GetAssetBundleRefOrNull(assetBundleName);
            if (assetBundleRefOrNull is not AssetBundleRef assetBundleRef)
            {
                return null;
            }
            Bundle<T> bundle = base._bundleFactory.GetBundleFromAssetBundleRef<T>(assetBundleRef);
            return new TaskBundle<T>(bundle);
        }

        public override TaskBundleScene? RentBundleSceneOrNull(string assetBundleName)
        {
            AssetBundleRef? assetBundleRefOrNull = _GetAssetBundleRefOrNull(assetBundleName);
            if (assetBundleRefOrNull is not AssetBundleRef assetBundleRef)
            {
                return null;
            }
            BundleScene bundleScene = base._bundleFactory.GetBundleSceneFromAssetBundleRef(assetBundleRef);
            return new TaskBundleScene(bundleScene);
        }

        public override async Task ReturnBundleAsync(Bundle bundle)
        {
            base._bundleFactory.PrepareReturn(bundle);
            EditorUtility.UnloadUnusedAssetsImmediate();
            GC.Collect();
            await Task.Yield();
        }

        private AssetBundleRef? _GetAssetBundleRefOrNull(string assetBundleName)
        {
            try
            {
                assetBundleName = assetBundleName.ToLower();
                string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName);
                string assetPath = assetPaths[0];
                UnityEngine.Object[] assets;
                if (Path.GetExtension(assetPath) == ".unity")
                {
                    assets = new UnityEngine.Object[] { AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath) };
                }
                else
                {
                    assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                }
                AssetBundleRef assetBundleRef = new AssetBundleRef(assetBundleName, Array.Empty<string>())
                {
                    _unityObjects = assets,
                    _state = AssetBundleRef.E_ASSETBUNDLEREF_STATE.LOADED
                };
                return assetBundleRef;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"_GetAssetBundleRefOrNull({assetBundleName})");
                UnityEngine.Debug.LogException(ex);
                return null;
            }
        }
    }
}
#endif // UNITY_EDITOR
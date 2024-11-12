using NF.UnityLibs.Managers.AssetBundleManagement.Impl;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NF.UnityLibs.Managers.AssetBundleManagement
{
    [Serializable]
    public class AssetBundleManager : IDisposable
    {
        public const string MANIFEST_NAME = "__ASSETBUNDLE";
        private bool _isDisposed = false;
        private HashSet<string> _assetBundleNameSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        [SerializeField]
        private AssetBundle? _ab_AssetBundleManifestOrNull = null;

        [SerializeField]
        private TaskQueueProcessor _taskQueueProcessor = new TaskQueueProcessor();

        [SerializeField]
        private BundleFactory _bundleFactory = new BundleFactory();

        public void Dispose()
        {
            Assert.IsFalse(_isDisposed, "disposed");
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            _taskQueueProcessor.Dispose();
            _bundleFactory.Dispose();

            if (_ab_AssetBundleManifestOrNull is not null)
            {
                _ab_AssetBundleManifestOrNull.Unload(true);
                _ab_AssetBundleManifestOrNull = null;
            }

            _assetBundleNameSet.Clear();
        }

        public virtual async Task<Exception?> InitAsync(string deviceBaseFpath)
        {
            Assert.IsFalse(_isDisposed, "disposed");

            string ab_AssetBundleManifestFpath = $"{deviceBaseFpath}/{MANIFEST_NAME}";
            if (!File.Exists(ab_AssetBundleManifestFpath))
            {
                return new AssetBundleManagerException(E_EXCEPTION_KIND.ERR_ON_INITIALIZE, $"!File.Exists({ab_AssetBundleManifestFpath})");
            }

            AssetBundleCreateRequest abcr = AssetBundle.LoadFromFileAsync(ab_AssetBundleManifestFpath);
            while (!abcr.isDone)
            {
                await Task.Yield();
            }

            AssetBundle? ab_AssetBundleManifestOrNull = abcr.assetBundle;
            if (ab_AssetBundleManifestOrNull is null)
            {
                return new AssetBundleManagerException(E_EXCEPTION_KIND.ERR_ON_INITIALIZE, $"ab_AssetBundleManifestOrNull is null | {ab_AssetBundleManifestFpath}");
            }

            AssetBundleManifest? manifestOrNull = ab_AssetBundleManifestOrNull.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            if (manifestOrNull is null)
            {
                return new AssetBundleManagerException(E_EXCEPTION_KIND.ERR_ON_INITIALIZE, $"manifestOrNull is null | {ab_AssetBundleManifestFpath}");
            }

            _ab_AssetBundleManifestOrNull = ab_AssetBundleManifestOrNull;

            _assetBundleNameSet.Clear();
            string[] bundleNames = manifestOrNull.GetAllAssetBundles();
            foreach (string bundleName in bundleNames)
            {
                _assetBundleNameSet.Add(bundleName);
            }

            _taskQueueProcessor.Init(deviceBaseFpath, manifestOrNull!);
            return null;
        }

        public async Task<Bundle<T>?> RentBundleOrNull<T>(string assetBundleName) where T : Object
        {
            Assert.IsFalse(_isDisposed, "disposed");

            if (!_assetBundleNameSet.Contains(assetBundleName))
            {
                Debug.LogError($"!_assetBundleNameSet.Contains(\"{assetBundleName}\")");
                return null;
            }

            AssetBundleRef assetBundleRef = await _taskQueueProcessor.EnqueueTaskBundleLoad(assetBundleName);
            Bundle<T> bundle = _bundleFactory.GetBundleFromAssetBundleRef<T>(assetBundleRef);
            return bundle;
        }

        public virtual async Task ReturnBundleAsync(Bundle bundle)
        {
            Assert.IsFalse(_isDisposed, "disposed");

            if (bundle.State != Bundle.E_BUNDLE_STATE.RENTED)
            {
                Debug.LogError($"bundle.State != Bundle.E_BUNDLE_STATE.RENTED: {bundle.State}");
                return;
            }

            _bundleFactory.PrepareReturn(bundle);
            await _taskQueueProcessor.EnqueueTaskBundleUnload(bundle);
        }

        public void Update()
        {
            Assert.IsFalse(_isDisposed, "disposed");

            _taskQueueProcessor.Update();
        }
    }
}